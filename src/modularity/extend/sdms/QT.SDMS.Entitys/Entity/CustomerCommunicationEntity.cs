using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Systems.Entitys.Crm;

/// <summary>
/// 售电系统-客户沟通记录实体.
/// </summary>
[SugarTable("sdms_customer_communication")]
[Tenant(ClaimConst.TENANTID)]
public class CustomerCommunicationEntity : CUDEntityBase
{
    /// <summary>
    /// 客户id.
    /// </summary>
    [SugarColumn(ColumnName = "CustomerId")]
    public string CustomerId { get; set; }

    /// <summary>
    /// 客户经理id.
    /// </summary>
    [SugarColumn(ColumnName = "ManagerId")]
    public string ManagerId { get; set; }

    /// <summary>
    /// 沟通时间.
    /// </summary>
    [SugarColumn(ColumnName = "CommunicationTime")]
    public DateTime? CommunicationTime { get; set; }

    /// <summary>
    /// 是否接通.
    /// </summary>
    [SugarColumn(ColumnName = "WhetherContent")]
    public int? WhetherContent { get; set; }

    /// <summary>
    /// 沟通内容.
    /// </summary>
    [SugarColumn(ColumnName = "Content")]
    public string Content { get; set; }

    /// <summary>
    /// 附件.
    /// </summary>
    [SugarColumn(ColumnName = "Attachment")]
    public string Attachment { get; set; }

}