using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace QT.CMS.Entitys.Dto.Config;

/// <summary>
/// 系统设置
/// </summary>
public class SysConfigDto
{
    #region 基本设置======================================
    /// <summary>
    /// 网站名称
    /// </summary>
    [Display(Name = "网站名称")]
    [Required(ErrorMessage = "{0}不可为空")]
    public string? webName { get; set; }

    /// <summary>
    /// 主站域名
    /// </summary>
    [Display(Name = "主站域名")]
    [Required(ErrorMessage = "{0}不可为空")]
    public string webUrl { get; set; } = "http://demo.95033.cn";

    /// <summary>
    /// 系统版本号
    /// </summary>
    [Display(Name = "系统版本号")]
    [Required(ErrorMessage = "{0}不可为空")]
    public string webVersion { get; set; } = "VCore 1.0";

    /// <summary>
    /// 公司名称
    /// </summary>
    public string? webCompany { get; set; }

    /// <summary>
    /// 通讯地址
    /// </summary>
    public string? webAddress { get; set; }

    /// <summary>
    /// 联系电话
    /// </summary>
    public string? webTel { get; set; }

    /// <summary>
    /// 传真号码
    /// </summary>
    public string? webFax { get; set; }

    /// <summary>
    /// 管理员邮箱
    /// </summary>
    public string? webMail { get; set; }

    /// <summary>
    /// 网站备案号
    /// </summary>
    public string? webCrod { get; set; }

    /// <summary>
    /// 后台管理日志
    /// </summary>
    public byte logStatus { get; set; } = 0;

    /// <summary>
    /// 是否关闭网站
    /// </summary>
    public byte webStatus { get; set; } = 0;

    /// <summary>
    /// 关闭原因描述
    /// </summary>
    public string? webCloseReason { get; set; }
    #endregion

    #region 短信平台设置==================================
    /// <summary>
    /// 短信API地址
    /// </summary>
    public string smsApiUrl { get; set; } = "http://smsapi.95033.cn/httpapi/";

    /// <summary>
    /// 短信平台登录账户名
    /// </summary>
    public string smsUserName { get; set; } = "test";

    /// <summary>
    /// 短信平台登录密码
    /// </summary>
    public string smsPassword { get; set; } = "123456";
    #endregion

    #region 邮件发送设置==================================
    /// <summary>
    /// STMP服务器
    /// </summary>
    public string? emailSmtp { get; set; }

    /// <summary>
    /// 是否启用SSL加密连接
    /// </summary>
    public byte emailSSL { get; set; }

    /// <summary>
    /// SMTP端口
    /// </summary>
    public int? emailPort { get; set; }

    /// <summary>
    /// 发件人地址
    /// </summary>
    public string? emailFrom { get; set; }

    /// <summary>
    /// 邮箱账号
    /// </summary>
    public string? emailUserName { get; set; }

    /// <summary>
    /// 邮箱密码
    /// </summary>
    public string? emailPassword { get; set; }

    /// <summary>
    /// 发件人昵称
    /// </summary>
    public string? emailNickname { get; set; }
    #endregion

    #region 文件上传设置==================================
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
    public string fileExtension { get; set; } = "gif,jpg,jpeg,png,bmp,rar,zip,doc,xls,txt,pdf";

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
    #endregion

    #region 云存储设置====================================
    /// <summary>
    /// 文件服务器(本地localhost、七牛云qiniu、阿里云aliyun)
    /// </summary>
    public string fileServer { get; set; } = "localhost";

    /// <summary>
    /// 七牛云Bucket名称
    /// </summary>
    public string? kodoBucket { get; set; }

    /// <summary>
    /// 七牛云自定义空间域名
    /// </summary>
    public string? kodoDomain { get; set; }
    /// <summary>
    /// 七牛云AccessKey密钥
    /// </summary>
    public string? kodoAccessKey { get; set; }

    /// <summary>
    /// 七牛云SecretKey密钥
    /// </summary>
    public string? kodoSecretKey { get; set; }

    /// <summary>
    /// 阿里云存储空间名称
    /// </summary>
    public string? ossBucket { get; set; }

    /// <summary>
    /// 阿里云区域外网
    /// </summary>
    public string? ossEndpoint { get; set; }

    /// <summary>
    /// 阿里云自定义空间域名
    /// </summary>
    public string? ossDomain { get; set; }

    /// <summary>
    /// 阿里云AccessKeyId
    /// </summary>
    public string? ossAccessKey { get; set; }

    /// <summary>
    /// 阿里云AccessKeySecret
    /// </summary>
    public string? ossSecretKey { get; set; }
    #endregion
}

/// <summary>
/// 系统设置(公共)
/// </summary>
public class SysConfigClientDto
{
    /// <summary>
    /// 网站名称
    /// </summary>
    [Display(Name = "网站名称")]
    [Required(ErrorMessage = "{0}不可为空")]
    public string? webName { get; set; }

    /// <summary>
    /// 主站域名
    /// </summary>
    [Display(Name = "主站域名")]
    [Required(ErrorMessage = "{0}不可为空")]
    public string webUrl { get; set; } = "http://demo.95033.cn";

    /// <summary>
    /// 系统版本号
    /// </summary>
    [Display(Name = "系统版本号")]
    [Required(ErrorMessage = "{0}不可为空")]
    public string webVersion { get; set; } = "VCore 1.0";

    /// <summary>
    /// 公司名称
    /// </summary>
    public string? webCompany { get; set; }

    /// <summary>
    /// 通讯地址
    /// </summary>
    public string? webAddress { get; set; }

    /// <summary>
    /// 联系电话
    /// </summary>
    public string? webTel { get; set; }

    /// <summary>
    /// 传真号码
    /// </summary>
    public string? webFax { get; set; }

    /// <summary>
    /// 管理员邮箱
    /// </summary>
    public string? webMail { get; set; }

    /// <summary>
    /// 网站备案号
    /// </summary>
    public string? webCrod { get; set; }

    /// <summary>
    /// 后台管理日志
    /// </summary>
    public byte logStatus { get; set; } = 0;

    /// <summary>
    /// 是否关闭网站
    /// </summary>
    public byte webStatus { get; set; } = 0;

    /// <summary>
    /// 关闭原因描述
    /// </summary>
    public string? webCloseReason { get; set; }
}
