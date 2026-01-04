using QT.Common.Filter;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseStore;

/// <summary>
/// 入驻商铺列表查询输入
/// </summary>
public class LogEnterpriseStoreListQueryInput : PageInputBase
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
    /// 商家ID.
    /// </summary>
    public string eId { get; set; }

    /// <summary>
    /// 商铺编号.
    /// </summary>
    public string storeNumber { get; set; }

}