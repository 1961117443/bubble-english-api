using QT.Common.Models;

namespace QT.Logistics.Entitys.Dto.LogOrderAttachment;

/// <summary>
/// 物流订单输出参数.
/// </summary>
public class LogOrderAttachmentInfoOutput : FileControlsModel
{
    /// <summary>
    /// 主键
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
    public string size { get; set; }
}