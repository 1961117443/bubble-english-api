
namespace QT.Logistics.Entitys.Dto.LogEnterpriseAuditrecord;

/// <summary>
/// 入驻商家审批记录输出参数.
/// </summary>
public class LogEnterpriseAuditrecordInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 审批时间.
    /// </summary>
    public DateTime? auditTime { get; set; }

    /// <summary>
    /// 审批用户.
    /// </summary>
    public string auditUserId { get; set; }

    /// <summary>
    /// 审批意见.
    /// </summary>
    public string remark { get; set; }

}