using QT.Common.Contracts;
using SqlSugar;
using System.ComponentModel;

namespace QT.SDMS.Entitys.Entity;

/// <summary>
/// 客户发票记录表
/// </summary>
[SugarTable("sdms_customer_invoice")]
public class CustomerInvoiceEntity : CUDEntityBase
{
    /// <summary>
    /// 客户ID
    /// </summary>
    [SugarColumn(ColumnName = "CustomerId")]
    public string CustomerId { get; set; }

    /// <summary>
    /// 发票单号
    /// </summary>
    [SugarColumn(ColumnName = "InvoiceNo")] public string InvoiceNo { get; set; }

    /// <summary>
    /// 发票金额
    /// </summary>
    [SugarColumn(ColumnName = "Amount")] public decimal Amount { get; set; } 

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "Remark")] public string Remark { get; set; }

    /// <summary>
    /// 普票/专票
    /// </summary>
    [SugarColumn(ColumnName = "InvoiceType")] public SdmsInvoiceType InvoiceType { get; set; }

    /// <summary>
    /// 来源（充值/账单等）
    /// </summary>
    [SugarColumn(ColumnName = "SourceType")] public SdmsInvoiceSourceType SourceType { get; set; }

    /// <summary>
    /// 来源表主键（充值Id 或 账单Id）
    /// </summary>
    [SugarColumn(ColumnName = "SourceId")] public string SourceId { get; set; }

    /// <summary>
    /// 状态（申请中、已开、已作废）
    /// </summary>
    [SugarColumn(ColumnName = "Status")] public SdmsInvoiceStatus Status { get; set; }

    /// <summary>
    /// 电子发票文件地址
    /// </summary>
    [SugarColumn(ColumnName = "PdfUrl")] public string PdfUrl { get; set; }

    /// <summary>
    /// 开票时间
    /// </summary>
    [SugarColumn(ColumnName = "IssueTime")] public DateTime? IssueTime { get; set; }
}

public enum SdmsInvoiceStatus
{
    [Description("申请中")] Pending = 1,   // 申请中
    [Description("已开票")] Issued = 2,    // 已开票
    [Description("已作废")] Canceled = 3   // 已作废
}


public enum SdmsInvoiceType
{
    [Description("普通发票")] Normal = 1, // 普通发票
    [Description("专用发票")] Special = 2 // 专用发票
}


public enum SdmsInvoiceSourceType
{
    [Description("电费账单")] Bill = 1,     // 电费账单
    [Description("充值记录")] Recharge = 2  // 充值记录
}
