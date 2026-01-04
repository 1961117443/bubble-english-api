using QT.Common.Filter;

namespace QT.Application.Entitys.Dto.ExtExpenseRecord;

/// <summary>
/// 报销单列表查询输入
/// </summary>
public class ExtExpenseRecordListQueryInput : PageInputBase
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
    /// 报销日期范围
    /// </summary>
    public string billDateRange { get; set; }


    /// <summary>
    /// 标签
    /// </summary>
    public string? label { get; set; }

}