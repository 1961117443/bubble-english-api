using QT.Common.Filter;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpProductcheck;

/// <summary>
/// 盘点记录主表列表查询输入
/// </summary>
public class ErpProductcheckMasterListQueryInput : PageInputBase
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
    /// 公司ID.
    /// </summary>
    public string oid { get; set; }

    /// <summary>
    /// 盘点日期.
    /// </summary>
    public DateTime? checkTime { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 商品.
    /// </summary>
    public string productName { get; set; }

    /// <summary>
    /// 盘点日期 范围查询
    /// </summary>
    public string checkTimeRange { get; set; }
}