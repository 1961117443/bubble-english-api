namespace QT.JXC.Entitys.Dto.ErpOrderGj;

public class ErpOrderGjListQueryInput: PageInputBase
{
    /// <summary>
    /// 订单编号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 客户ID.
    /// </summary>
    public string cid { get; set; }

    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTime? beginDate { get; set; }


    /// <summary>
    /// 结束日期
    /// </summary>
    public DateTime? endDate { get; set; }

    /// <summary>
    /// 下单时间
    /// </summary>
    public string createTime { get; set; }

    /// <summary>
    /// 约定配送时间
    /// </summary>
    public string posttime { get; set; }

    /// <summary>
    /// 餐别.
    /// </summary>
    public string diningType { get; set; }

    /// <summary>
    /// 状态(1: 未完成，2：已完成)
    /// </summary>
    public string status { get; set; }

    /// <summary>
    /// 商品名称
    /// </summary>
    public string productName { get; set; }

    /// <summary>
    /// 客户类型
    /// </summary>
    public string customerType { get; set; }
}
