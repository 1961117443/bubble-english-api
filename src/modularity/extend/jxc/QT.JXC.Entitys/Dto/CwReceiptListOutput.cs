using QT.DependencyInjection;

namespace QT.JXC.Entitys.Dto;

/// <summary>
/// 收款单输入参数.
/// </summary>
public class CwReceiptListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 客户id.
    /// </summary>
    public string cid { get; set; }

    /// <summary>
    /// 收款日期.
    /// </summary>
    public DateTime? receiptDate { get; set; }

    /// <summary>
    /// 收款方式.
    /// </summary>
    public string paymentMethod { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 收款金额.
    /// </summary>
    public decimal amount { get; set; }


    /// <summary>
    /// 客户名称.
    /// </summary>
    public string cidName { get; set; }


    /// <summary>
    /// 公司id.
    /// </summary>
    public string oid { get; set; }

}

[SuppressSniffer]
public class CwReceiptQueryOrderListOutput
{
    public string id { get; set; }
    public string no { get; set; }

    public string cid { get; set; }

    public string cidName { get; set; }

    public DateTime? createTime { get; set; }

    /// <summary>
    /// 应收款
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 已收款
    /// </summary>
    public decimal amount2 { get; set; }

    /// <summary>
    /// 未收款
    /// </summary>
    public decimal amount3 { get; set; }

    /// <summary>
    /// 约定配送时间
    /// </summary>
    public DateTime? posttime { get; set; }

    /// <summary>
    /// 入库单号
    /// </summary>
    public string rkNo { get; set; }

    /// <summary>
    /// 出库单号
    /// </summary>
    public string outNo { get; set; }

    /// <summary>
    /// 实际入库数量：称重数量+特殊入库数量
    /// </summary>
    public decimal readNum { get; set; }

    /// <summary>
    /// 入库单价
    /// </summary>
    public decimal price { get; set; }
}


[SuppressSniffer]
public class CwReceiptQueryOrderDetailListOutput : CwReceiptQueryOrderListOutput
{
    /// <summary>
    /// 商品
    /// </summary>
    public string productName { get; set; }

    /// <summary>
    /// 规格
    /// </summary>
    public string midName { get; set; }

}