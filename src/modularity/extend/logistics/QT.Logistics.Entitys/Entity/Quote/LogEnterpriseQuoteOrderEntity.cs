using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 报价订单
/// </summary>
[SugarTable("log_enterprise_Quote_Order")]
[Tenant(ClaimConst.TENANTID)]
public class LogEnterpriseQuoteOrderEntity : CLDEntityBase, ILogEnterpriseEntity
{
    /// <summary>
    /// 报价单号.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_No")]
    public string? No { get; set; }

    /// <summary>
    /// 客户id.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_Cid")]
    public string? Cid { get; set; }

    /// <summary>
    /// 报价日期.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_BillDate")]
    public DateTime? BillDate { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_Remark")]
    public string? Remark { get; set; }

    /// <summary>
    /// 查看次数.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_ViewCount")]
    public int? ViewCount { get; set; }

    /// <summary>
    /// 发票类型.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_InvoiceType")]
    public string? InvoiceType { get; set; }

    /// <summary>
    /// 是否包含发票.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_Invoice")]
    public int? Invoice { get; set; }

    /// <summary>
    /// 需方联系人员
    /// </summary>
    [SugarColumn(ColumnName = "ConnectUser")]
    public string ConnectUser { get; set; }

    /// <summary>
    /// 需方联系电话
    /// </summary>
    [SugarColumn(ColumnName = "ConnectPhone")]
    public string ConnectPhone { get; set; }

    /// <summary>
    /// 需方联系地址
    /// </summary>
    [SugarColumn(ColumnName = "ConnectAddress")]
    public string ConnectAddress { get; set; }

    /// <summary>
    /// 供方联系人员
    /// </summary>
    [SugarColumn(ColumnName = "SupplyUser")]
    public string SupplyUser { get; set; }

    /// <summary>
    /// 供方联系电话
    /// </summary>
    [SugarColumn(ColumnName = "SupplyPhone")]
    public string SupplyPhone { get; set; }

    /// <summary>
    /// 供方联系地址
    /// </summary>
    [SugarColumn(ColumnName = "SupplyAddress")]
    public string SupplyAddress { get; set; }

    /// <summary>
    /// 模板id
    /// </summary>
    [SugarColumn(ColumnName = "Tid")]
    public string Tid { get; set; }

    /// <summary>
    /// 模板参数
    /// </summary>
    [SugarColumn(ColumnName = "TemplateJson")]
    public string TemplateJson { get; set; }

    /// <summary>
    /// 商家ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_EId")]
    public string EId { get; set; }
}


/// <summary>
/// 报价订单明细
/// </summary>
[SugarTable("log_enterprise_Quote_record")]
[Tenant(ClaimConst.TENANTID)]
public class LogEnterpriseQuoteOrderRecordEntity : CLDEntityBase
{
    /// <summary>
    /// 报价单id.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_Fid")]
    public string? Fid { get; set; }
    /// <summary>
    /// 商品名称.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_Name")]
    public string? Name { get; set; }

    /// <summary>
    /// 规格.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_Spec")]
    public string? Spec { get; set; }

    /// <summary>
    /// 单位.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_Unit")]
    public string? Unit { get; set; }

    /// <summary>
    /// 单价
    /// </summary>
    [SugarColumn(ColumnName = "F_Price")]
    public decimal Price { get; set; }

    /// <summary>
    /// 金额
    /// </summary>
    [SugarColumn(ColumnName = "F_Amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    [SugarColumn(ColumnName = "F_Num")]
    public decimal Num { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_Remark")]
    public string? Remark { get; set; }
}
