namespace QT.Application.Entitys.Dto.FreshDelivery.ErpSupplierprice;

/// <summary>
/// 供应商定价输入参数.
/// </summary>
public class ErpSupplierpriceListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 规格ID.
    /// </summary>
    public string gid { get; set; }

    /// <summary>
    /// 供应商id.
    /// </summary>
    public string supplierId { get; set; }


    /// <summary>
    /// 供应商.
    /// </summary>
    public string supplier { get; set; }

    /// <summary>
    /// 折扣（=价格/原价）.
    /// </summary>
    public decimal discount { get; set; }

    /// <summary>
    /// 价格（=原价*折扣）.
    /// </summary>
    public decimal price { get; set; }

    public string gidName { get; set; }
    public string productName { get; set; }

    /// <summary>
    /// 计价类型（1：按折扣，2：按价格）
    /// </summary>
    public int pricingType { get; set; }

    /// <summary>
    /// 公司ID.
    /// </summary>
    public string oid { get; set; }

    /// <summary>
    /// 计量单位
    /// </summary>
    public string unit { get; set; }

    /// <summary>
    /// 一级分类
    /// </summary>
    public string rootProducttype { get; set; }
}