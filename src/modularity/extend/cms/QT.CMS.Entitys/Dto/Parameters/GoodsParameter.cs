namespace QT.CMS.Entitys.Dto.Parameter;

/// <summary>
/// 商品查询参数
/// </summary>
public class GoodsParameter : BaseParameter
{
    /// <summary>
    /// 所属类别
    /// </summary>
    public long CategoryId { get; set; }
    /// <summary>
    /// 所属品牌
    /// </summary>
    public long BrandId { get; set; }
    /// <summary>
    /// 商品标签
    /// </summary>
    public long LabelId { get; set; }
    /// <summary>
    /// 最低价格
    /// </summary>
    public decimal MinPrice { get; set; }
    /// <summary>
    /// 最高价格
    /// </summary>
    public decimal MaxPrice { get; set; }
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }
    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }
}
