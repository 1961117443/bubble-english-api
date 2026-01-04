using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Dto.Common.QRCode;
using QT.Systems.Interfaces.System;
using SkiaSharp;
using ZXing;
using ZXing.QrCode;
using ZXing.QrCode.Internal;
using ZXing.SkiaSharp;
using ZXing.SkiaSharp.Rendering;

namespace QT.Systems.Common;

[ApiDescriptionSettings(Tag = "Common", Name = "QRCode", Order = 161)]
[Route("api/QRCode")]
public class QRCodeService : IDynamicApiController,IScoped, IQRCodeService  
{
    private const int DEFAULT_SIZE = 400;
    private const int DEFAULT_MARGIN = 10;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("generate")]
    [AllowAnonymous]
    public GenerateQRCodeOutput GenerateQRCode([FromBody] QRCodeRequest request)
    {
        try
        {
            // 验证输入
            if (string.IsNullOrWhiteSpace(request.Content))
                throw Oops.Oh("编码内容不能为空");

            // 创建二维码
            var qrCode = GenerateBaseQRCode(
                content: request.Content,
                size: DEFAULT_SIZE,
                margin: request.MarginBlocks * 4, // 每个色块4像素
                eccLevel: GetErrorCorrectionLevel(request.ErrorCorrection),
                version: request.Version
            );

            // 应用自定义样式
            ApplyCustomStyle(
                qrCode: qrCode,
                foregroundColor: SKColor.Parse(request.ForegroundColor),
                backgroundColor: SKColor.Parse(request.BackgroundColor),
                dotShape: request.DotShape,
                eyeShape: request.EyeShape,
                eyeColor: SKColor.Parse(request.EyeColor)
            );

            // 添加文字
            if (!string.IsNullOrWhiteSpace(request.TopText))
                AddTextToQRCode(qrCode, request.TopText, Position.Top);
            if (!string.IsNullOrWhiteSpace(request.MiddleText))
                AddTextToQRCode(qrCode, request.MiddleText, Position.Middle);
            if (!string.IsNullOrWhiteSpace(request.BottomText))
                AddTextToQRCode(qrCode, request.BottomText, Position.Bottom);

            // 转换为Base64
            var base64String = ConvertToBase64(qrCode);

            return (new GenerateQRCodeOutput
            {
                image = base64String,
                size = $"{request.LabelWidth}*{request.LabelHeight}mm"
            });
        }
        catch (Exception ex)
        {
            throw Oops.Oh($"生成二维码时出错: {ex.Message}");
        }
    }

    private SKBitmap GenerateBaseQRCode(string content, int size, int margin,
        ErrorCorrectionLevel eccLevel, int version)
    {
        var options = new QrCodeEncodingOptions
        {
            DisableECI = true,
            CharacterSet = "UTF-8",
            Width = size,
            Height = size,
            Margin = margin,
            ErrorCorrection = eccLevel,
            QrVersion = version
        };

        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = options,
            Renderer = new SKBitmapRenderer()
        };

        return writer.Write(content);
    }

    private void ApplyCustomStyle(SKBitmap qrCode, SKColor foregroundColor,
        SKColor backgroundColor, string dotShape, string eyeShape, SKColor eyeColor)
    {
        // 计算模块大小
        int moduleSize = qrCode.Width / 25; // 假设标准二维码是25x25模块

        using (var canvas = new SKCanvas(qrCode))
        {
            // 设置背景色
            canvas.Clear(backgroundColor);

            // 绘制二维码点
            for (int y = 0; y < qrCode.Height; y += moduleSize)
            {
                for (int x = 0; x < qrCode.Width; x += moduleSize)
                {
                    // 只处理黑色像素（二维码点）
                    if (qrCode.GetPixel(x, y) == SKColors.Black)
                    {
                        SKRect rect = new SKRect(x, y, x + moduleSize, y + moduleSize);

                        // 判断是否在定位点区域
                        bool isPositionPattern =
                            (x < 7 * moduleSize && y < 7 * moduleSize) || // 左上角
                            (x > (qrCode.Width - 7 * moduleSize) && y < 7 * moduleSize) || // 右上角
                            (x < 7 * moduleSize && y > (qrCode.Height - 7 * moduleSize)); // 左下角

                        using (var paint = new SKPaint { Color = isPositionPattern ? eyeColor : foregroundColor })
                        {
                            if (isPositionPattern)
                            {
                                // 绘制定位点（码眼）
                                DrawShape(canvas, paint, rect, eyeShape);
                            }
                            else
                            {
                                // 绘制普通点（码点）
                                DrawShape(canvas, paint, rect, dotShape);
                            }
                        }
                    }
                }
            }
        }
    }

    private void DrawShape(SKCanvas canvas, SKPaint paint, SKRect rect, string shapeType)
    {
        switch (shapeType)
        {
            case "square":
                canvas.DrawRect(rect, paint);
                break;
            case "circle":
                canvas.DrawOval(rect, paint);
                break;
            case "rounded":
                canvas.DrawRoundRect(rect, rect.Width / 4, rect.Height / 4, paint);
                break;
            case "dot":
                var dotRect = new SKRect(
                    rect.Left + rect.Width / 4,
                    rect.Top + rect.Height / 4,
                    rect.Right - rect.Width / 4,
                    rect.Bottom - rect.Height / 4
                );
                canvas.DrawOval(dotRect, paint);
                break;
            case "rounded-medium":
                canvas.DrawRoundRect(rect, rect.Width / 6, rect.Height / 6, paint);
                break;
            default: // 默认为方形
                canvas.DrawRect(rect, paint);
                break;
        }
    }

    private void AddTextToQRCode(SKBitmap qrCode, string text, Position position)
    {
        using (var canvas = new SKCanvas(qrCode))
        using (var paint = new SKPaint())
        {
            paint.Color = SKColors.Black;
            paint.IsAntialias = true;
            paint.TextSize = 14;
            paint.Typeface = SKTypeface.FromFamilyName("Microsoft YaHei");
            paint.TextAlign = SKTextAlign.Center;

            SKPoint textPoint;
            switch (position)
            {
                case Position.Top:
                    textPoint = new SKPoint(qrCode.Width / 2, 30);
                    break;
                case Position.Middle:
                    textPoint = new SKPoint(qrCode.Width / 2, qrCode.Height / 2);
                    break;
                case Position.Bottom:
                    textPoint = new SKPoint(qrCode.Width / 2, qrCode.Height - 10);
                    break;
                default:
                    textPoint = new SKPoint(qrCode.Width / 2, 30);
                    break;
            }

            canvas.DrawText(text, textPoint, paint);
        }
    }

    private string ConvertToBase64(SKBitmap image)
    {
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
        using (var stream = new MemoryStream())
        {
            data.SaveTo(stream);
            return $"data:image/png;base64,{Convert.ToBase64String(stream.ToArray())}";
        }
    }

    private ErrorCorrectionLevel GetErrorCorrectionLevel(string level)
    {
        return level switch
        {
            "L" => ErrorCorrectionLevel.L,
            "M" => ErrorCorrectionLevel.M,
            "Q" => ErrorCorrectionLevel.Q,
            "H" => ErrorCorrectionLevel.H,
            _ => ErrorCorrectionLevel.M
        };
    }

    [NonAction]
    public string GenerateToString(string content)
    {
        var output = GenerateQRCode(new QRCodeRequest
        {
            Content = content
        });

        return output.image;
    }

    private enum Position { Top, Middle, Bottom }
}

[ApiDescriptionSettings(Tag = "Common", Name = "QRCode2", Order = 161)]
[Route("api/QRCode2")]

public class QRCodeController : ControllerBase, IDynamicApiController
{
    private const int DEFAULT_SIZE = 400;
    private const int DEFAULT_MARGIN = 10;

    [HttpPost("generate")]
    [AllowAnonymous]
    public IActionResult GenerateQRCode([FromBody] QRCodeRequest request)
    {
        try
        {
            // 验证输入
            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest("编码内容不能为空");

            // 创建二维码
            var qrCode = GenerateBaseQRCode(
                content: request.Content,
                size: DEFAULT_SIZE,
                margin: request.MarginBlocks * 4, // 每个色块4像素
                eccLevel: GetErrorCorrectionLevel(request.ErrorCorrection),
                version: request.Version
            );

            // 应用自定义样式
            ApplyCustomStyle(
                qrCode: qrCode,
                foregroundColor: SKColor.Parse(request.ForegroundColor),
                backgroundColor: SKColor.Parse(request.BackgroundColor),
                dotShape: request.DotShape,
                eyeShape: request.EyeShape,
                eyeColor: SKColor.Parse(request.EyeColor)
            );

            // 添加文字
            if (!string.IsNullOrWhiteSpace(request.TopText))
                AddTextToQRCode(qrCode, request.TopText, Position.Top);
            if (!string.IsNullOrWhiteSpace(request.MiddleText))
                AddTextToQRCode(qrCode, request.MiddleText, Position.Middle);
            if (!string.IsNullOrWhiteSpace(request.BottomText))
                AddTextToQRCode(qrCode, request.BottomText, Position.Bottom);

            // 转换为Base64
            var base64String = ConvertToBase64(qrCode);

            return Ok(new
            {
                success = true,
                image = base64String,
                size = $"{request.LabelWidth}*{request.LabelHeight}mm"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"生成二维码时出错: {ex.Message}");
        }
    }

    private SKBitmap GenerateBaseQRCode(string content, int size, int margin,
        ErrorCorrectionLevel eccLevel, int version)
    {
        var options = new QrCodeEncodingOptions
        {
            DisableECI = true,
            CharacterSet = "UTF-8",
            Width = size,
            Height = size,
            Margin = margin,
            ErrorCorrection = eccLevel,
            QrVersion = version
        };

        var writer = new BarcodeWriter<SKBitmap>
        {
            Format = BarcodeFormat.QR_CODE,
            Options = options,
            Renderer = new SKBitmapRenderer()
        };

        return writer.Write(content);
    }

    private void ApplyCustomStyle(SKBitmap qrCode, SKColor foregroundColor,
    SKColor backgroundColor, string dotShape, string eyeShape, SKColor eyeColor)
    {
        // 创建临时位图保存原始数据
        using (var original = new SKBitmap(qrCode.Width, qrCode.Height))
        {
            qrCode.CopyTo(original);

            using (var canvas = new SKCanvas(qrCode))
            {
                // 设置背景色
                canvas.Clear(backgroundColor);

                // 计算模块大小
                int moduleSize = qrCode.Width / 25; // 标准二维码25x25模块

                // 绘制二维码点
                for (int y = 0; y < qrCode.Height; y += moduleSize)
                {
                    for (int x = 0; x < qrCode.Width; x += moduleSize)
                    {
                        if (original.GetPixel(x, y) == SKColors.Black)
                        {
                            SKRect rect = new SKRect(x, y, x + moduleSize, y + moduleSize);

                            // 判断关键区域 - 必须保持原始方形
                            bool isCriticalArea = IsPositionPattern(x, y, moduleSize, qrCode.Width, qrCode.Height) ||
                                                 IsAlignmentPattern(x, y, moduleSize, qrCode.Width, qrCode.Height) ||
                                                 IsTimingPattern(x, y, moduleSize, qrCode.Width, qrCode.Height);

                            using (var paint = new SKPaint { Color = foregroundColor })
                            {
                                if (isCriticalArea)
                                {
                                    // 关键区域保持方形黑色
                                    canvas.DrawRect(rect, paint);
                                }
                                else
                                {
                                    // 非关键区域可自定义形状
                                    DrawShape(canvas, paint, rect, dotShape);
                                }
                            }
                        }
                    }
                }

                // 单独处理定位点（码眼）外观
                DrawPositionPatterns(qrCode, canvas, eyeColor, eyeShape, moduleSize);
            }
        }
    }

    // 判断是否是定位点
    private bool IsPositionPattern(int x, int y, int moduleSize, int width, int height)
    {
        // 三个定位点位置
        return (x < 7 * moduleSize && y < 7 * moduleSize) || // 左上角
               (x > (width - 7 * moduleSize) && y < 7 * moduleSize) || // 右上角
               (x < 7 * moduleSize && y > (height - 7 * moduleSize)); // 左下角
    }

    // 判断是否是对齐点
    private bool IsAlignmentPattern(int x, int y, int moduleSize, int width, int height)
    {
        // 根据版本确定对齐点位置（简化版，实际应根据版本计算）
        int centerX = width / 2;
        int centerY = height / 2;
        return Math.Abs(x - centerX) < 2 * moduleSize &&
               Math.Abs(y - centerY) < 2 * moduleSize;
    }

    // 判断是否是时序模式
    private bool IsTimingPattern(int x, int y, int moduleSize, int width, int height)
    {
        // 水平和垂直的时序线
        return (Math.Abs(y - 6 * moduleSize) < moduleSize / 2 && x >= 8 * moduleSize && x <= width - 8 * moduleSize) ||
               (Math.Abs(x - 6 * moduleSize) < moduleSize / 2 && y >= 8 * moduleSize && y <= height - 8 * moduleSize);
    }

    // 专门绘制定位点（保持外部黑色，内部可自定义）
    private void DrawPositionPatterns(SKBitmap qrCode, SKCanvas canvas, SKColor eyeColor, string eyeShape, int moduleSize)
    {
        // 左上角定位点
        DrawPositionPattern(canvas, 3 * moduleSize, 3 * moduleSize, moduleSize * 7, eyeColor, eyeShape);

        // 右上角定位点
        DrawPositionPattern(canvas, qrCode.Width - 4 * moduleSize, 3 * moduleSize, moduleSize * 7, eyeColor, eyeShape);

        // 左下角定位点
        DrawPositionPattern(canvas, 3 * moduleSize, qrCode.Height - 4 * moduleSize, moduleSize * 7, eyeColor, eyeShape);
    }

    private void DrawPositionPattern(SKCanvas canvas, int centerX, int centerY, int size, SKColor color, string shape)
    {
        // 外框保持黑色
        using (var blackPaint = new SKPaint { Color = SKColors.Black })
        {
            canvas.DrawRect(new SKRect(centerX - size / 2, centerY - size / 2, centerX + size / 2, centerY + size / 2), blackPaint);
        }

        // 内部可自定义
        using (var paint = new SKPaint { Color = color })
        {
            var innerRect = new SKRect(
                centerX - size / 2 + size / 7,
                centerY - size / 2 + size / 7,
                centerX + size / 2 - size / 7,
                centerY + size / 2 - size / 7
            );

            DrawShape(canvas, paint, innerRect, shape);
        }
    }

    private void DrawShape(SKCanvas canvas, SKPaint paint, SKRect rect, string shapeType)
    {
        switch (shapeType)
        {
            case "square":
                canvas.DrawRect(rect, paint);
                break;
            case "circle":
                canvas.DrawOval(rect, paint);
                break;
            case "rounded":
                canvas.DrawRoundRect(rect, rect.Width / 4, rect.Height / 4, paint);
                break;
            case "dot":
                var dotRect = new SKRect(
                    rect.Left + rect.Width / 4,
                    rect.Top + rect.Height / 4,
                    rect.Right - rect.Width / 4,
                    rect.Bottom - rect.Height / 4
                );
                canvas.DrawOval(dotRect, paint);
                break;
            case "rounded-medium":
                canvas.DrawRoundRect(rect, rect.Width / 6, rect.Height / 6, paint);
                break;
            default: // 默认为方形
                canvas.DrawRect(rect, paint);
                break;
        }
    }

    private void AddTextToQRCode(SKBitmap qrCode, string text, Position position)
    {
        using (var canvas = new SKCanvas(qrCode))
        using (var paint = new SKPaint())
        {
            paint.Color = SKColors.Black;
            paint.IsAntialias = true;
            paint.TextSize = 14;
            paint.Typeface = SKTypeface.FromFamilyName("Microsoft YaHei");
            paint.TextAlign = SKTextAlign.Center;

            SKPoint textPoint;
            switch (position)
            {
                case Position.Top:
                    textPoint = new SKPoint(qrCode.Width / 2, 30);
                    break;
                case Position.Middle:
                    textPoint = new SKPoint(qrCode.Width / 2, qrCode.Height / 2);
                    break;
                case Position.Bottom:
                    textPoint = new SKPoint(qrCode.Width / 2, qrCode.Height - 10);
                    break;
                default:
                    textPoint = new SKPoint(qrCode.Width / 2, 30);
                    break;
            }

            canvas.DrawText(text, textPoint, paint);
        }
    }

    private string ConvertToBase64(SKBitmap image)
    {
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
        using (var stream = new MemoryStream())
        {
            data.SaveTo(stream);
            return $"data:image/png;base64,{Convert.ToBase64String(stream.ToArray())}";
        }
    }

    private ErrorCorrectionLevel GetErrorCorrectionLevel(string level)
    {
        return level switch
        {
            "L" => ErrorCorrectionLevel.L,
            "M" => ErrorCorrectionLevel.M,
            "Q" => ErrorCorrectionLevel.Q,
            "H" => ErrorCorrectionLevel.H,
            _ => ErrorCorrectionLevel.M
        };
    }

    private enum Position { Top, Middle, Bottom }
}


public class QRCodeRequest
{
    public string Content { get; set; } = "A202500000021";
    public string ForegroundColor { get; set; } = "#1a73e8"; // 专业蓝
    public string BackgroundColor { get; set; } = "#FFFFFF";
    public string DotShape { get; set; } = "square";
    public string EyeShape { get; set; } = "rounded-medium";
    public string EyeColor { get; set; } = "#1a73e8"; // 专业蓝
    public int MarginBlocks { get; set; } = 1;
    public string ErrorCorrection { get; set; } = "M";
    public int Version { get; set; } = 2;
    public int LabelWidth { get; set; } = 20;
    public int LabelHeight { get; set; } = 20;
    public string TopText { get; set; }
    public string MiddleText { get; set; }
    public string BottomText { get; set; }
}