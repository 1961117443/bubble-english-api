using System;
using System.Collections.Generic;
using System.Text;

namespace QT.CMS.Entitys.Dto.Config;

/// <summary>
/// 上传设置
/// </summary>
public class UploadConfigDto
{
    /// <summary>
    /// 附件上传目录
    /// </summary>
    public string filePath { get; set; } = "upload";

    /// <summary>
    /// 附件保存方式
    /// </summary>
    public int fileSave { get; set; } = 2;

    /// <summary>
    /// 编辑器远程图片上传
    /// </summary>
    public int fileRemote { get; set; } = 0;

    /// <summary>
    /// 附件上传类型
    /// </summary>
    public string fileExtension { get; set; } = "gif,jpg,jpeg,png,bmp,rar,zip,doc,xls,txt";

    /// <summary>
    /// 视频上传类型
    /// </summary>
    public string videoExtension { get; set; } = "flv,mp3,mp4,avi";

    /// <summary>
    /// 文件上传大小
    /// </summary>
    public int attachSize { get; set; } = 51200;

    /// <summary>
    /// 视频上传大小
    /// </summary>
    public int videoSize { get; set; } = 102400;

    /// <summary>
    /// 图片上传大小
    /// </summary>
    public int imgSize { get; set; } = 10240;

    /// <summary>
    /// 图片最大高度(像素)
    /// </summary>
    public int imgMaxHeight { get; set; } = 1600;

    /// <summary>
    /// 图片最大宽度(像素)
    /// </summary>
    public int imgMaxWidth { get; set; } = 1600;

    /// <summary>
    /// 生成缩略图高度(像素)
    /// </summary>
    public int thumbnailHeight { get; set; } = 300;

    /// <summary>
    /// 生成缩略图宽度(像素)
    /// </summary>
    public int thumbnailWidth { get; set; } = 300;

    /// <summary>
    /// 缩略图生成方式
    /// </summary>
    public string thumbnailMode { get; set; } = "Cut";

    /// <summary>
    /// 图片水印类型
    /// </summary>
    public int watermarkType { get; set; } = 2;

    /// <summary>
    /// 图片水印位置
    /// </summary>
    public int watermarkPosition { get; set; } = 9;

    /// <summary>
    /// 图片生成质量
    /// </summary>
    public int watermarkImgQuality { get; set; } = 80;

    /// <summary>
    /// 图片水印文件
    /// </summary>
    public string watermarkPic { get; set; } = "watermark.png";

    /// <summary>
    /// 水印透明度
    /// </summary>
    public int watermarkTransparency { get; set; } = 5;

    /// <summary>
    /// 水印文字
    /// </summary>
    public string watermarkText { get; set; } = "乾通中软";

    /// <summary>
    /// 文字字体
    /// </summary>
    public string watermarkFont { get; set; } = "Tahoma";

    /// <summary>
    /// 文字大小(像素)
    /// </summary>
    public int watermarkFontSize { get; set; } = 12;
}
