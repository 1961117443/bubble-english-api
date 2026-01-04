using QT.Common.Filter;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseFinancial;

/// <summary>
/// 缴费记录列表查询输入
/// </summary>
public class LogEnterpriseFinancialListQueryInput : PageInputBase
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
    /// 商家id.
    /// </summary>
    public string eId { get; set; }

    /// <summary>
    /// 商铺编号.
    /// </summary>
    public string storeNumber { get; set; }

    /// <summary>
    /// 缴费流水号.
    /// </summary>
    public string no { get; set; }

}