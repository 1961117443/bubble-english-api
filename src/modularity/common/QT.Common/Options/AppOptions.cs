using QT.Common.Enum;
using QT.ConfigurableOptions;

namespace QT.Common.Options;

/// <summary>
/// QT基本配置.
/// </summary>
public sealed class AppOptions : IConfigurableOptions
{
    /// <summary>
    /// 命名空间.
    /// </summary>
    public List<string> CodeAreasName { get; set; }

    /// <summary>
    /// 系统文件路径.
    /// </summary>
    public string SystemPath { get; set; }

    /// <summary>
    /// 微信公众号允许上传文件类型.
    /// </summary>
    public List<string> MPUploadFileType { get; set; }

    /// <summary>
    /// 微信允许上传文件类型.
    /// </summary>
    public List<string> WeChatUploadFileType { get; set; }

    /// <summary>
    /// 允许图片类型.
    /// </summary>
    public List<string> AllowUploadImageType { get; set; }

    /// <summary>
    /// 允许上传文件类型.
    /// </summary>
    public List<string> AllowUploadFileType { get; set; }

    /// <summary>
    /// 文件预览方式.
    /// </summary>
    public PreviewType PreviewType { get; set; } 

    /// <summary>
    /// kkfile发布域名.
    /// </summary>
    public string Domain { get; set; } 

    /// <summary>
    /// 永中 配置.
    /// </summary>
    public YOZO YOZO { get; set; } 

    /// <summary>
    /// 软件的错误报告.
    /// </summary>
    public bool ErrorReport { get; set; }

    /// <summary>
    /// 报告发给谁.
    /// </summary>
    public string ErrorReportTo { get; set; }
}

/// <summary>
/// 永中.
/// </summary>
public class YOZO
{
    /// <summary>
    /// 域名.
    /// </summary>
    public string Domain { get; set; }

    /// <summary>
    /// 域名key.
    /// </summary>
    public string domainKey { get; set; }

    /// <summary>
    /// 上传接口.
    /// </summary>
    public string UploadAPI { get; set; }

    /// <summary>
    /// 预览接口.
    /// </summary>
    public string DownloadAPI { get; set; }

    /// <summary>
    /// 应用Id.
    /// </summary>
    public string AppId { get; set; }

    /// <summary>
    /// 签名.
    /// </summary>
    public string AppKey { get; set; }
}