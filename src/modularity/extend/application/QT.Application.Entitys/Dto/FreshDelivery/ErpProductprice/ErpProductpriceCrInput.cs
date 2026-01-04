namespace QT.Application.Entitys.Dto.FreshDelivery.ErpProductprice;

/// <summary>
/// 商品定价修改输入参数.
/// </summary>
public class ErpProductpriceCrInput
{
    public string id { get; set; }
    /// <summary>
    /// 规格ID.
    /// </summary>
    public string gid { get; set; }

    /// <summary>
    /// 价格.
    /// </summary>
    public decimal price { get; set; }

    /// <summary>
    /// 定价时间.
    /// </summary>
    public DateTime? time { get; set; }
    /// <summary>
    /// 客户ID.
    /// </summary>
    public string cid { get; set; }

}