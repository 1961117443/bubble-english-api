

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpProductcustomertypeprice;

/// <summary>
/// 商品客户类型定价修改输入参数.
/// </summary>
public class ErpProductcustomertypepriceCrInput
{
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

    /// <summary>
    /// 计价类型（1：按折扣，2：按价格）
    /// </summary>
    public int pricingType { get; set; }

    /// <summary>
    /// 公司id.
    /// </summary>
    public string oid { get; set; }

}