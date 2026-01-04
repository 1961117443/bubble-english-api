using QT.Common.Models;
using QT.Logistics.Entitys.Dto.LogEnterpriseAttachment;
using QT.Logistics.Entitys.Dto.LogEnterpriseAuditrecord;

namespace QT.Logistics.Entitys.Dto.LogEnterprise;

/// <summary>
/// 入驻商家修改输入参数.
/// </summary>
public class LogEnterpriseCrInput
{
    /// <summary>
    /// 名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 电话.
    /// </summary>
    public string phone { get; set; }

    /// <summary>
    /// 附件.
    /// </summary>
    public List<LogEnterpriseAttachmentInfoOutput> logEnterpriseAttachmentList { get; set; }

    /// <summary>
    /// 负责人.
    /// </summary>
    public string leader { get; set; }

    /// <summary>
    /// 管理员id.
    /// </summary>
    public string adminId { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    public bool status { get; set; }

    /// <summary>
    /// 入驻商家审批记录.
    /// </summary>
    public List<LogEnterpriseAuditrecordCrInput> logEnterpriseAuditrecordList { get; set; }

    /// <summary>
    /// 简图.
    /// </summary>
    public List<FileControlsModel> imageUrl { get; set; }

}