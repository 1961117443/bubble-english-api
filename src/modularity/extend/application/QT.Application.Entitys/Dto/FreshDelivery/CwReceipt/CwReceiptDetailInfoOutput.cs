
namespace QT.Application.Entitys.Dto.FreshDelivery.CwReceiptDetail;

/// <summary>
/// 收款单明细输出参数.
/// </summary>
public class CwReceiptDetailInfoOutput
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
    /// 收款金额.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 商品
    /// </summary>
    public string productName { get; set; }

    /// <summary>
    /// 规格
    /// </summary>
    public string midName { get; set; }

}