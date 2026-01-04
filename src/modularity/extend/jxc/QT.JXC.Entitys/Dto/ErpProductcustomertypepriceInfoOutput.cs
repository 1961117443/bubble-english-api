namespace QT.JXC.Entitys.Dto;

/// <summary>
/// 商品客户类型定价输出参数.
/// </summary>
public class ErpProductcustomertypepriceInfoOutput
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
    /// 客户类型（数据字典）.
    /// </summary>
    public string tid { get; set; }

    /// <summary>
    /// 折扣（=价格/原价）.
    /// </summary>
    public decimal discount { get; set; }

    /// <summary>
    /// 价格（=原价*折扣）.
    /// </summary>
    public decimal price { get; set; }

    public string gidName { get; set; }

    public string productName { get;set; }

    /// <summary>
    /// 计价类型（0：按折扣，1：按价格）
    /// </summary>
    public int pricingType { get; set; }
}