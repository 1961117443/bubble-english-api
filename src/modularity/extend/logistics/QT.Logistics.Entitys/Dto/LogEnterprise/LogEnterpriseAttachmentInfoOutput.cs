
using QT.Common.Models;
using System.Security.AccessControl;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseAttachment;

/// <summary>
/// 入驻商家附件输出参数.
/// </summary>
public class LogEnterpriseAttachmentInfoOutput : FileControlsModel
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 上传时间
    /// </summary>
    public DateTime? uploadTime { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
    public string size { get; set; }
}