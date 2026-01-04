using QT.Common.Filter;

namespace QT.Logistics.Entitys.Dto.LogEnterprise;

/// <summary>
/// 入驻商家列表查询输入
/// </summary>
public class LogEnterpriseListQueryInput : PageInputBase
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
    /// 电话.
    /// </summary>
    public string phone { get; set; }

    /// <summary>
    /// 负责人.
    /// </summary>
    public string leader { get; set; }

}