using SkiaSharp;

namespace QT.CMS;

public class ImageHelper
{
    /// <summary>
		/// 计算新尺寸
		/// </summary>
		/// <param name="width">原始宽度</param>
		/// <param name="height">原始高度</param>
		/// <param name="maxWidth">最大新宽度</param>
		/// <param name="maxHeight">最大新高度</param>
    private static SKSizeI ResizeImage(int width, int height, int maxWidth, int maxHeight)
    {
        if (maxWidth <= 0)
            maxWidth = width;
        if (maxHeight <= 0)
            maxHeight = height;

        float maxRatio = (float)maxWidth / maxHeight;
        float originalRatio = (float)width / height;

        int newWidth, newHeight;

        if (width > maxWidth || height > maxHeight)
        {
            if (originalRatio > maxRatio)
            {
                newWidth = maxWidth;
                newHeight = (int)Math.Round(maxWidth / originalRatio);
            }
            else
            {
                newWidth = (int)Math.Round(maxHeight * originalRatio);
                newHeight = maxHeight;
            }
        }
        else
        {
            newWidth = width;
            newHeight = height;
        }

        return new SKSizeI(newWidth, newHeight);
    }

    /// <summary>
    /// 得到图片格式
    /// </summary>
    /// <param name="filePath">文件名</param>
    public static SKEncodedImageFormat GetFormat(string filePath)
    {
        string? ext = System.IO.Path.GetExtension(filePath)?.TrimStart('.');
        if (ext != null)
        {
            switch (ext.ToLower())
            {
                case "bmp":
                    return SKEncodedImageFormat.Bmp;
                case "png":
                    return SKEncodedImageFormat.Png;
                case "gif":
                    return SKEncodedImageFormat.Gif;
                case "jpeg":
                case "jpg":
                    return SKEncodedImageFormat.Jpeg;
                case "webp":
                    return SKEncodedImageFormat.Webp;
            }
        }
        return SKEncodedImageFormat.Jpeg;
    }

    /// <summary>
    /// 超出最大尺寸裁剪图片
    /// </summary>
    /// <param name="byteData">文件字节数组</param>
    /// <param name="fileExt">文件扩展名</param>
    /// <param name="maxWidth">最大宽度</param>
    /// <param name="maxHeight">最大高度</param>
    public static byte[] MakeThumbnail(byte[] byteData, string fileExt, int maxWidth, int maxHeight)
    {
        using (MemoryStream memoryStream = new(byteData))
        {
            using SKBitmap originalBitmap = SKBitmap.Decode(memoryStream);
            if (originalBitmap!=null && (originalBitmap.Width > maxWidth || originalBitmap.Height > maxHeight))
            {
                SKSizeI newSize = ResizeImage(originalBitmap.Width, originalBitmap.Height, maxWidth, maxHeight);
                using SKBitmap resizedBitmap = originalBitmap.Resize(new SKImageInfo(newSize.Width, newSize.Height), SKFilterQuality.High);
                using SKImage resizedImage = SKImage.FromBitmap(resizedBitmap);
                using SKData encodedData = resizedImage.Encode(GetFormat(fileExt), 100);
                return encodedData.ToArray();
            }
        }
        return byteData;
    }

    /// <summary>
    /// 根据X轴和Y轴裁剪图片
    /// </summary>
    /// <param name="byteData">文件字节数组</param>
    /// <param name="fileExt">文件扩展名</param>
    /// <param name="maxWidth">缩略图宽度</param>
    /// <param name="maxHeight">缩略图高度</param>
    /// <param name="cropWidth">裁剪宽度</param>
    /// <param name="cropHeight">裁剪高度</param>
    /// <param name="X">X轴</param>
    /// <param name="Y">Y轴</param>
    public static byte[] MakeThumbnail(byte[] byteData, string fileExt, int maxWidth, int maxHeight, int cropWidth, int cropHeight, int X, int Y)
    {
        using MemoryStream memoryStream = new(byteData);
        using SKBitmap originalBitmap = SKBitmap.Decode(memoryStream);
        using SKBitmap croppedBitmap = new(cropWidth, cropHeight);
        using (SKCanvas canvas = new(croppedBitmap))
        {
            canvas.Clear(SKColors.Transparent);
            canvas.DrawBitmap(originalBitmap, new SKRect(0, 0, cropWidth, cropHeight), new SKRect(X, Y, X + cropWidth, Y + cropHeight));
        }

        SKSizeI newSize = ResizeImage(cropWidth, cropHeight, maxWidth, maxHeight);
        using SKBitmap resizedBitmap = croppedBitmap.Resize(new SKImageInfo(newSize.Width, newSize.Height), SKFilterQuality.High);
        using SKImage resizedImage = SKImage.FromBitmap(resizedBitmap);
        using SKData encodedData = resizedImage.Encode(GetFormat(fileExt), 100);
        return encodedData.ToArray();
    }

    /// <summary>
    /// 生成缩略图
    /// </summary>
    /// <param name="byteData">文件字节数组</param>
    /// <param name="fileExt">文件扩展名</param>
    /// <param name="width">缩略图宽度</param>
    /// <param name="height">缩略图高度</param>
    /// <param name="mode">生成缩略图的方式</param>
    public static byte[] MakeThumbnail(byte[] byteData, string fileExt, int width, int height, string? mode)
    {
        using MemoryStream memoryStream = new(byteData);
        using SKBitmap originalBitmap = SKBitmap.Decode(memoryStream);
        int towidth = width;
        int toheight = height;

        int x = 0;
        int y = 0;
        int ow = originalBitmap.Width;
        int oh = originalBitmap.Height;

        switch (mode)
        {
            case "HW":
                if ((double)originalBitmap.Width / (double)originalBitmap.Height > (double)towidth / (double)toheight)
                {
                    ow = originalBitmap.Width;
                    oh = originalBitmap.Width * height / towidth;
                    x = 0;
                    y = (originalBitmap.Height - oh) / 2;
                }
                else
                {
                    oh = originalBitmap.Height;
                    ow = originalBitmap.Height * towidth / toheight;
                    y = 0;
                    x = (originalBitmap.Width - ow) / 2;
                }
                break;
            case "W":
                toheight = originalBitmap.Height * width / originalBitmap.Width;
                break;
            case "H":
                towidth = originalBitmap.Width * height / originalBitmap.Height;
                break;
            case "Cut":
                if ((double)originalBitmap.Width / (double)originalBitmap.Height > (double)towidth / (double)toheight)
                {
                    oh = originalBitmap.Height;
                    ow = originalBitmap.Height * towidth / toheight;
                    y = 0;
                    x = (originalBitmap.Width - ow) / 2;
                }
                else
                {
                    ow = originalBitmap.Width;
                    oh = originalBitmap.Width * height / towidth;
                    x = 0;
                    y = (originalBitmap.Height - oh) / 2;
                }
                break;
            default:
                break;
        }

        using SKBitmap resizedBitmap = originalBitmap.Resize(new SKImageInfo(towidth, toheight), SKFilterQuality.High);
        using SKImage resizedImage = SKImage.FromBitmap(resizedBitmap);
        using SKData encodedData = resizedImage.Encode(GetFormat(fileExt), 100);
        return encodedData.ToArray();
    }

    #region 图片水印
    /// <summary>
    /// 图片水印
    /// </summary>
    /// <param name="byteData">图片字节数组</param>
    /// <param name="fileExt">图片扩展名</param>
    /// <param name="filePath">水印文件物理路径</param>
    /// <param name="location">图片水印位置 0=不使用 1=左上 2=中上 3=右上 4=左中  9=右下</param>
    /// <param name="quality">附加水印图片质量,0-100</param>
    /// <param name="transparency">水印的透明度 1--10 10为不透明</param>
    public static byte[] ImageWatermark(byte[] byteData, string fileExt, string filePath, int location, int quality, int transparency)
    {
        using MemoryStream memoryStream = new MemoryStream(byteData);
        using SKBitmap imgBitmap = SKBitmap.Decode(memoryStream);
        if (!File.Exists(filePath))
        {
            return byteData;
        }

        using (SKBitmap watermarkBitmap = SKBitmap.Decode(filePath))
        {
            if (watermarkBitmap.Height >= imgBitmap.Height || watermarkBitmap.Width >= imgBitmap.Width)
            {
                return byteData;
            }

            using SKCanvas canvas = new SKCanvas(imgBitmap);
            float toTransparency = 0.5f;
            if (transparency >= 1 && transparency <= 10)
            {
                toTransparency = transparency / 10.0f;
            }

            using SKPaint paint = new SKPaint();
            paint.Color = new SKColor(255, 255, 255, (byte)(255 * toTransparency));
            paint.IsAntialias = true;
            paint.FilterQuality = SKFilterQuality.High;
            paint.IsDither = true;

            SKPoint position = GetWatermarkPosition(imgBitmap.Width, imgBitmap.Height, watermarkBitmap.Width, watermarkBitmap.Height, location);

            canvas.DrawBitmap(watermarkBitmap, position, paint);
        }

        using SKImage imgImage = SKImage.FromBitmap(imgBitmap);
        using SKData encodedData = imgImage.Encode(GetFormat(fileExt), quality);
        return encodedData.ToArray();
    }
    #endregion

    #region 文字水印
    /// <summary>
    /// 文字水印
    /// </summary>
    /// <param name="byteData">图片字节数组</param>
    /// <param name="fileExt">图片扩展名</param>
    /// <param name="text">水印文字</param>
    /// <param name="location">图片水印位置 0=不使用 1=左上 2=中上 3=右上 4=左中  9=右下</param>
    /// <param name="quality">附加水印图片质量,0-100</param>
    /// <param name="fontName">字体</param>
    /// <param name="fontSize">字体大小</param>
    public static byte[] LetterWatermark(byte[] byteData, string? fileExt, string? text, int location, int quality, string? fontName, int fontSize)
    {
        using SKBitmap bitmap = SKBitmap.Decode(byteData);
        using (SKCanvas canvas = new(bitmap))
        {
            using SKPaint paint = new();
            paint.TextSize = fontSize;
            paint.IsAntialias = true;
            paint.Color = SKColors.Black;
            paint.Typeface = SKTypeface.FromFamilyName(fontName);

            SKRect bounds = new SKRect();
            paint.MeasureText(text, ref bounds);

            float x = 0;
            float y = 0;
            switch (location)
            {
                case 1:
                    x = bitmap.Width * 0.01f;
                    y = bitmap.Height * 0.01f;
                    break;
                case 2:
                    x = (bitmap.Width * 0.5f) - (bounds.MidX);
                    y = bitmap.Height * 0.01f;
                    break;
                case 3:
                    x = (bitmap.Width * 0.99f) - bounds.Width;
                    y = bitmap.Height * 0.01f;
                    break;
                case 4:
                    x = bitmap.Width * 0.01f;
                    y = (bitmap.Height * 0.5f) - (bounds.Height / 2);
                    break;
                case 5:
                    x = (bitmap.Width * 0.5f) - (bounds.MidX);
                    y = (bitmap.Height * 0.5f) - (bounds.MidY);
                    break;
                case 6:
                    x = (bitmap.Width * 0.99f) - bounds.Width;
                    y = (bitmap.Height * 0.5f) - (bounds.MidY);
                    break;
                case 7:
                    x = bitmap.Width * 0.01f;
                    y = (bitmap.Height * 0.99f) - bounds.Height;
                    break;
                case 8:
                    x = (bitmap.Width * 0.5f) - (bounds.MidX);
                    y = (bitmap.Height * 0.99f) - bounds.Height;
                    break;
                case 9:
                    x = (bitmap.Width * 0.99f) - bounds.Width;
                    y = (bitmap.Height * 0.99f) - bounds.Height;
                    break;
            }

            canvas.DrawText(text, x + 1, y + 1, paint);
            paint.Color = SKColors.White;
            canvas.DrawText(text, x, y, paint);
        }

        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(GetImageFormat(fileExt), quality);
        return data.ToArray();
    }
    #endregion

    #region 私有方法
    /// <summary>
    /// 计算定位
    /// </summary>
    private static SKPoint GetWatermarkPosition(int imageWidth, int imageHeight, int watermarkWidth, int watermarkHeight, int location)
    {
        float x = 0;
        float y = 0;

        switch (location)
        {
            case 1: // 左上
                x = imageWidth * 0.01f;
                y = imageHeight * 0.01f;
                break;
            case 2: // 中上
                x = (imageWidth * 0.5f) - (watermarkWidth * 0.5f);
                y = imageHeight * 0.01f;
                break;
            case 3: // 右上
                x = (imageWidth * 0.99f) - watermarkWidth;
                y = imageHeight * 0.01f;
                break;
            case 4: // 左中
                x = imageWidth * 0.01f;
                y = (imageHeight * 0.5f) - (watermarkHeight * 0.5f);
                break;
            case 5: // 居中
                x = (imageWidth * 0.5f) - (watermarkWidth * 0.5f);
                y = (imageHeight * 0.5f) - (watermarkHeight * 0.5f);
                break;
            case 6: // 右中
                x = (imageWidth * 0.99f) - watermarkWidth;
                y = (imageHeight * 0.5f) - (watermarkHeight * 0.5f);
                break;
            case 7: // 左下
                x = imageWidth * 0.01f;
                y = (imageHeight * 0.99f) - watermarkHeight;
                break;
            case 8: // 中下
                x = (imageWidth * 0.5f) - (watermarkWidth * 0.5f);
                y = (imageHeight * 0.99f) - watermarkHeight;
                break;
            case 9: // 右下
                x = (imageWidth * 0.99f) - watermarkWidth;
                y = (imageHeight * 0.99f) - watermarkHeight;
                break;
        }

        return new SKPoint(x, y);
    }

    /// <summary>
    /// 指定图像的编码格式
    /// </summary>
    private static SKEncodedImageFormat GetImageFormat(string? fileExt)
    {
        return fileExt?.ToLower() switch
        {
            "jpg" or "jpeg" => SKEncodedImageFormat.Jpeg,
            "png" => SKEncodedImageFormat.Png,
            "gif" => SKEncodedImageFormat.Gif,
            "bmp" => SKEncodedImageFormat.Bmp,
            _ => SKEncodedImageFormat.Png,
        };
    }
    #endregion
}