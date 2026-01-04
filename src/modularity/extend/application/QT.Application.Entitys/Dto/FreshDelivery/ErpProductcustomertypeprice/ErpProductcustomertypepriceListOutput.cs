
namespace QT.Application.Entitys.Dto.FreshDelivery.ErpProductcustomertypeprice;

/// <summary>
/// 商品客户类型定价输入参数.
/// </summary>
public class ErpProductcustomertypepriceListOutput
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
    /// <summary>
    /// 是否有质检报告
    /// </summary>
    public int hasQualityReport { get; set; }
}