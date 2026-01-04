namespace QT.JXC.Entitys.Dto.Erp.ErpSupplierprice;

/// <summary>
/// 供应商定价修改输入参数.
/// </summary>
public class ErpSupplierpriceCrInput
{
    /// <summary>
    /// 规格ID.
    /// </summary>
    public string gid { get; set; }

    /// <summary>
    /// 供应商id.
    /// </summary>
    public string supplierId { get; set; }

    /// <summary>
    /// 折扣（=价格/原价）.
    /// </summary>
    public decimal discount { get; set; }

    /// <summary>
    /// 价格（=原价*折扣）.
    /// </summary>
    public decimal price { get; set; }

    /// <summary>
    /// 计价类型（1：按折扣，2：按价格）
    /// </summary>
    public int pricingType { get; set; }

    /// <summary>
    /// 公司id.
    /// </summary>
    public string oid { get; set; }

}