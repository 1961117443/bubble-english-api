using NPOI.Util;
using System;

namespace QT.SDMS.Entitys.Dto.CustomerInvoice;

public class CustomerInvoiceCrInput
{

    public string customerId { get; set; }

    /// <summary>
    /// 发票号
    /// </summary>
    public string invoiceNo { get; set; }

    /// <summary>
    /// 开票金额
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 普票/专票
    /// </summary>
    public int? invoiceType { get; set; }

    /// <summary>
    /// 来源（充值/账单等）
    /// </summary>
    public int? sourceType { get; set; }

    /// <summary>
    /// 来源表主键（充值Id 或 账单Id）
    /// </summary>
    public string sourceId { get; set; }
    /// <summary>
    /// 状态（申请中、已开、已作废）
    /// </summary>
    public int? status { get; set; }



    /// <summary>
    /// 电子发票文件地址
    /// </summary>
    public string pdfUrl { get; set; }

    /// <summary>
    /// 开票时间
    /// </summary>
    public DateTime? issueTime { get; set; }


    /// <summary>
    /// 备注
    /// </summary>
    public string remark { get; set; }
}