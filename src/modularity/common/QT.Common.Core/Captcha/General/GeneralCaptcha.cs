using System.Drawing;
using System.Drawing.Imaging;
using Lazy.Captcha.Core;
using NPOI.SS.Formula.Functions;
using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.DependencyInjection;
using SkiaSharp;

namespace QT.Common.Core.Captcha.General;

/// <summary>
/// 常规验证码.
/// </summary>
public class GeneralCaptcha : IGeneralCaptcha, ITransient
{
    private readonly ICacheManager _cacheManager;
    private readonly ICaptcha _captcha;

    /// <summary>
    /// 构造函数.
    /// </summary>
    /// <param name="cacheManager"></param>
    public GeneralCaptcha(ICacheManager cacheManager, ICaptcha captcha)
    {
        _cacheManager = cacheManager;
        _captcha = captcha;
    }

    /// <summary>
    /// 常规验证码.
    /// </summary>
    /// <param name="timestamp">时间戳.</param>
    /// <param name="width">宽度.</param>
    /// <param name="height">高度.</param>
    /// <param name="length">长度.</param>
    /// <returns></returns>
    public async Task<byte[]> CreateCaptchaImage(string timestamp, int width, int height, int length = 4)
    {
        return await Draw(timestamp, width, height, length);
    }

    /// <summary>
    /// 画.
    /// </summary>
    /// <param name="timestamp">时间抽.</param>
    /// <param name="width">宽度.</param>
    /// <param name="height">高度.</param>
    /// <param name="length">长度.</param>
    /// <returns></returns>
    private async Task<byte[]> Draw(string timestamp, int width, int height, int length = 4)
    {
        int fontSize = 16;

        // 颜色列表，用于验证码、噪线、噪点
        Color[] color = { Color.Black, Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Brown, Color.Brown, Color.DarkBlue };

        // 字体列表，用于验证码
        string[] fonts = new[] { "Times New Roman", "Verdana", "Arial", "Gungsuh", "Impact" };

        // 验证码随机数
        Random codeRandom = new Random();
        string code = codeRandom.NextLetterAndNumberString(length); // 随机字符串集合


        {
            var captcha = _captcha.Generate(timestamp);

            // 缓存验证码正确集合
            await SetCode(timestamp, captcha.Code, TimeSpan.FromMinutes(5));
            return captcha.Bytes;
        }

        //{
        //    // 缓存验证码正确集合
        //    await SetCode(timestamp, code, TimeSpan.FromMinutes(5));

        //    var buffer = Create(code, width, height);
        //    Console.WriteLine("mbuffer度，{0}", buffer.Length);
        //    return buffer;

        //}
        using (Bitmap bmp = new Bitmap(width, height))
        using (Graphics g = Graphics.FromImage(bmp))
        using (MemoryStream ms = new MemoryStream())
        {
            g.Clear(Color.White);
            Random rnd = new Random();

            // 画噪线
            for (int i = 0; i < 1; i++)
            {
                int x1 = rnd.Next(width);
                int y1 = rnd.Next(height);
                int x2 = rnd.Next(width);
                int y2 = rnd.Next(height);
                var clr = color[rnd.Next(color.Length)];
                g.DrawLine(new Pen(clr), x1, y1, x2, y2);
            }

            // 画验证码字符串
            string fnt;
            Font ft;
            for (int i = 0; i < code.Length; i++)
            {
                fnt = fonts[rnd.Next(fonts.Length)];
                ft = new Font(fnt, fontSize);
                var clr = color[rnd.Next(color.Length)];
                g.DrawString(code[i].ToString(), ft, new SolidBrush(clr), ((float)i * 24) + 2, 0);
            }

            // 缓存验证码正确集合
            await SetCode(timestamp, code, TimeSpan.FromMinutes(5));

            // 将验证码图片写入内存流
            bmp.Save(ms, ImageFormat.Jpeg);
            return ms.ToArray();
        }
    }

    /// <summary>
    /// 保存验证码缓存.
    /// </summary>
    /// <param name="timestamp">时间戳.</param>
    /// <param name="code">验证码.</param>
    /// <param name="timeSpan">过期时间.</param>
    public async Task<bool> SetCode(string timestamp, string code, TimeSpan timeSpan)
    {
        string cacheKey = string.Format("{0}{1}", CommonConst.CACHEKEYCODE, timestamp);
        return await _cacheManager.SetAsync(cacheKey, code, timeSpan);
    }

    /// <summary>
    /// 生成验证码图片 LINUX 下生成方式
    /// </summary>
    public static byte[] Create(string captchaCode, int width = 0, int height = 30)
    {
        SKColor[] colors = { SKColors.Black, SKColors.Red, SKColors.DarkBlue, SKColors.Green, SKColors.Orange, SKColors.Brown, SKColors.DarkCyan, SKColors.Purple };
        //string[] fonts = { "Verdana", "Microsoft Sans Serif", "Comic Sans MS", "Arial" };
        string[] fonts = { "Actionj", "Kaiti" };

        if (width == 0) width = captchaCode.Length * 20;

        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var random = new Random();
        for (var i = 0; i < width * height * 0.1; i++)
        {
            var x = random.Next(width);
            var y = random.Next(height);
            var color = new SKColor((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));
            using var pointPaint = new SKPaint { Color = color };
            //canvas.DrawPoint(x, y, pointPaint);
        }

        // 添加干扰线
        for (var i = 0; i < 2; i++)
        {
            var startX = random.Next(width);
            var startY = random.Next(height);
            var endX = random.Next(width);
            var endY = random.Next(height);
            var lineColor = new SKColor((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));
            using var linePaint = new SKPaint { Color = lineColor, StrokeWidth = 1.5f };
            //canvas.DrawLine(startX, startY, endX, endY, linePaint);
        }
        Console.WriteLine("验证码：{0}", captchaCode);
        for (var i = 0; i < captchaCode.Length; i++)
        {
            var cIndex = random.Next(colors.Length);
            var fIndex = random.Next(fonts.Length);
            var font = SKTypeface.FromFamilyName(fonts[fIndex]);
            var fontSize = height / 2;
            using var textPaint = new SKPaint
            {
                Color = colors[cIndex],
                Typeface = font,
                TextSize = fontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Left,
                FakeBoldText = true
            };
            var bounds = new SKRect();
            textPaint.MeasureText(captchaCode[i].ToString(), ref bounds);
            var x = width / (captchaCode.Length + 2);
            var y = (height + bounds.Height) / 2;
            canvas.DrawText(captchaCode[i].ToString(), x + (x * i), y, textPaint);
        }

        using var image = surface.Snapshot();
        using var ms = new MemoryStream();
        image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(ms);
        return ms.ToArray();
    }
}