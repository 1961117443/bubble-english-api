using QT.Common.Filter;

namespace QT.JXC.Entitys.Dto.Erp.BuyOrder;

/// <summary>
/// 采购任务订单列表查询输入
/// </summary>
public class ErpBuyorderListQueryInput : PageInputBase
{
    /// <summary>
    /// 选择导出数据key.
    /// </summary>
    public string selectKey { get; set; }

    /// <summary>
    /// 导出类型.
    /// 0:导出本页，1:导出全部
    /// </summary>
    public int dataType { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 计划采购时间.
    /// </summary>
    public string taskBuyTime { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public string state { get; set; }

    /// <summary>
    /// 指派采购员id
    /// </summary>
    public string taskToUserId { get; set; }

    /// <summary>
    /// 公司id
    /// </summary>
    public string oid { get; set; }

    /// <summary>
    /// 采购渠道
    /// </summary>
    public string channel { get; set; }

    public string productName { get; set; }

    /// <summary>
    /// 付款属性
    /// </summary>
    public string payment { get; set; }

    /// <summary>
    /// 供应商
    /// </summary>
    public string supplierName { get; set; }

    /// <summary>
    /// 选中id集合，多个逗号相连
    /// </summary>
    public string items { get; set; }

    /// <summary>
    /// 入库单号
    /// </summary>
    public string rkNo { get; set; }

    /// <summary>
    /// 供应商id
    /// </summary>
    public string supplierId { get; set; }

    /// <summary>
    /// 审核人
    /// </summary>
    public string auditUserName { get; set; }

    /// <summary>
    /// 采购员
    /// </summary>
    public string taskToUserName { get; set; }
    
}