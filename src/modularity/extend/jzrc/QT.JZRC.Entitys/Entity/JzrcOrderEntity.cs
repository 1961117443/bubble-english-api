using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.JZRC.Entitys;

/// <summary>
/// 建筑人才平台订单管理实体.
/// </summary>
[SugarTable("jzrc_order")]
[Tenant(ClaimConst.TENANTID)]
public class JzrcOrderEntity :CUDEntityBase
{
    /// <summary>
    /// 订单编号.
    /// </summary>
    [SugarColumn(ColumnName = "OrderNo")]
    public string OrderNo { get; set; }

    /// <summary>
    /// 人才id.
    /// </summary>
    [SugarColumn(ColumnName = "TalentId")]
    public string TalentId { get; set; }

    /// <summary>
    /// 企业id.
    /// </summary>
    [SugarColumn(ColumnName = "CompanyId")]
    public string CompanyId { get; set; }

    /// <summary>
    /// 客户经理id.
    /// </summary>
    [SugarColumn(ColumnName = "ManagerId")]
    public string ManagerId { get; set; }

    /// <summary>
    /// 订单金额.
    /// </summary>
    [SugarColumn(ColumnName = "Amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 人才应分.
    /// </summary>
    [SugarColumn(ColumnName = "TalentShare")]
    public decimal TalentShare { get; set; }

    /// <summary>
    /// 企业应分.
    /// </summary>
    [SugarColumn(ColumnName = "CompanyShare")]
    public decimal CompanyShare { get; set; }

    /// <summary>
    /// 平台应分.
    /// </summary>
    [SugarColumn(ColumnName = "PlatformShare")]
    public decimal PlatformShare { get; set; }

    /// <summary>
    /// 订单处理状态.
    /// </summary>
    [SugarColumn(ColumnName = "ProcessingStatus")]
    public int? ProcessingStatus { get; set; }

    /// <summary>
    /// 订单处理时间.
    /// </summary>
    [SugarColumn(ColumnName = "ProcessingTime")]
    public DateTime? ProcessingTime { get; set; }

    /// <summary>
    /// 订单结算状态.
    /// </summary>
    [SugarColumn(ColumnName = "SettlementStatus")]
    public int? SettlementStatus { get; set; }

    /// <summary>
    /// 订单结算时间.
    /// </summary>
    [SugarColumn(ColumnName = "SettlementTime")]
    public DateTime? SettlementTime { get; set; }

}