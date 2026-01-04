using QT.Common.Filter;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOutorder;

/// <summary>
/// 出库订单表列表查询输入
/// </summary>
public class ErpOutorderListQueryInput : PageInputBase
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
    /// 出库订单号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 出库类型.
    /// </summary>
    public string inType { get; set; }

    /// <summary>
    /// 开始时间.
    /// </summary>
    public DateTime? beginDate { get; set; }

    /// <summary>
    /// 结束时间.
    /// </summary>
    public DateTime? endDate { get; set; }

    /// <summary>
    /// 当前导出数据
    /// </summary>
    public string items { get; set; }

    /// <summary>
    /// 创建时间范围
    /// </summary>
    public string createTimeRange { get; set; }


    /// <summary>
    /// 商品名称.
    /// </summary>
    public string productName { get; set; }

    /// <summary>
    /// 入库单号
    /// </summary>
    public string rkNo { get; set; }
}

public class ErpOutorderXsListQueryInput: ErpOutorderListQueryInput
{
    /// <summary>
    /// 销售订单号.
    /// </summary>
    public string xsNo { get; set; }
}