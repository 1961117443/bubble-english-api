using QT.Common.Filter;

namespace QT.JXC.Entitys.Dto;

/// <summary>
/// 供货商信息列表查询输入
/// </summary>
public class ErpSupplierListQueryInput : PageInputBase
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
    /// 负责人
    /// </summary>
    public string admin { get; set; }

    /// <summary>
    /// 负责人电话
    /// </summary>
    public string adminTel { get; set; }

}