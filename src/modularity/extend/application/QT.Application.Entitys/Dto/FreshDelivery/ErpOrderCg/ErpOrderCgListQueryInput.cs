using QT.Common.Filter;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOrderCg;

/// <summary>
/// 订单信息列表查询输入
/// </summary>
public class ErpOrderCgListQueryInput : PageInputBase
{
    /// <summary>
    /// 选择导出数据key.
    /// </summary>
    public string selectKey { get; set; }

    /// <summary>
    /// 导出类型.
    /// </summary>
    public int dataType { get; set; }

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
    public DateTime? createTime { get; set; }

    /// <summary>
    /// 约定配送时间
    /// </summary>
    public DateTime? posttime { get; set; }

    /// <summary>
    /// 下单时间范围
    /// </summary>
    public string createTimeRange { get; set; }

    /// <summary>
    /// 约定配送时间范围
    /// </summary>
    public string posttimeRange { get; set; }


    public string items { get; set; }

    public string activedTab { get; set; }
    /// <summary>
    /// 商品名称
    /// </summary>
    public string productName { get; set; }

    /// <summary>
    /// 客户类型
    /// </summary>
    public string tid { get; set; }
    
    /// <summary>
    /// 餐别
    /// </summary>
    public string diningType { get; set; }
}