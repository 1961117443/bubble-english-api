using System.ComponentModel;

namespace QT.JXC.Entitys.Dto;

/// <summary>
/// 付款单明细输出参数.
/// </summary>
public class CwPaymentDetailInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 转入id.
    /// </summary>
    public string inId { get; set; }

    /// <summary>
    /// 转入单号.
    /// </summary>
    public string inNo { get; set; }

    /// <summary>
    /// 应收款.
    /// </summary>
    public decimal inAmount { get; set; }


    /// <summary>
    /// 转入类型.
    /// </summary>
    public string inType { get; set; }

    /// <summary>
    /// 付款金额.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }


    /// <summary>
    /// 已付款金额.
    /// </summary>
    public decimal amount2 { get; set; }
}


public class CwPaymentExportOutput
{

    #region 主表
    /// <summary>
    /// 订单编号.
    /// </summary>
    [Description("订单编号")]
    public string no { get; set; }

    /// <summary>
    /// 供应商id.
    /// </summary>
    public string sid { get; set; }

    /// <summary>
    /// 付款日期.
    /// </summary>
    [Description("付款日期")]
    public string paymentDate { get; set; }

    /// <summary>
    /// 付款方式.
    /// </summary>
    [Description("付款方式")]
    public string paymentMethod { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [Description("主备注")]
    public string remark { get; set; }

    ///// <summary>
    ///// 付款金额.
    ///// </summary>
    //public decimal amount { get; set; }


    /// <summary>
    /// 所属公司.
    /// </summary>
    [Description("公司")]
    public string oidName { get; set; }

    /// <summary>
    /// 供应商.
    /// </summary>
    [Description("供应商")]
    public string sidName { get; set; }

    #endregion


    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 转入id.
    /// </summary>
    public string inId { get; set; }

    /// <summary>
    /// 转入单号.
    /// </summary>
    [Description("转入单号")]
    public string inNo { get; set; }

    /// <summary>
    /// 应收款.
    /// </summary>
    [Description("应收款")]
    public decimal inAmount { get; set; }


    /// <summary>
    /// 转入类型.
    /// </summary>
    public string inType { get; set; }

    /// <summary>
    /// 已付款金额.
    /// </summary>
    [Description("已付款金额")]
    public decimal amount2 { get; set; }

    /// <summary>
    /// 付款金额.
    /// </summary>
    [Description("付款金额")]
    public decimal amount { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [Description("备注")]
    public string itremark { get; set; }


}