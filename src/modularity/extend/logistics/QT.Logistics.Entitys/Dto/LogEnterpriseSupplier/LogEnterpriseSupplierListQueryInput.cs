using QT.Common.Filter;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseSupplier;

/// <summary>
/// 供应商列表查询输入
/// </summary>
public class LogEnterpriseSupplierListQueryInput : PageInputBase
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
    /// 名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 名称首字母.
    /// </summary>
    public string firstChar { get; set; }

}