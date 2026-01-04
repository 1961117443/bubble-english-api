using QT.Common.Filter;

namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 入库订单表列表查询输入
/// </summary>
public class ErpInorderListQueryInput : PageInputBase
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
    /// 入库订单号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 入库类型.
    /// </summary>
    public string inType { get; set; }

    /// <summary>
    /// 采购订单编号.
    /// </summary>
    public string cgNo { get; set; }


    /// <summary>
    /// 特殊入库状态
    /// </summary>
    public string specialState { get; set; }

    /// <summary>
    /// 商品名称.
    /// </summary>
    public string productName { get; set; }

    /// <summary>
    /// 创建时间 范围查询
    /// </summary>
    public string creatorTimeRange { get; set; }

    /// <summary>
    /// 销售订单编号.
    /// </summary>
    public string orderNo { get; set; }

    /// <summary>
    /// 过滤有入库数的记录
    /// </summary>
    public bool? hasNum { get; set; }

    /// <summary>
    /// 选中id集合，多个逗号相连
    /// </summary>
    public string items { get; set; }

    /// <summary>
    /// 约定配送时间 范围查询
    /// </summary>
    public string postTimeRange { get; set; }
}
