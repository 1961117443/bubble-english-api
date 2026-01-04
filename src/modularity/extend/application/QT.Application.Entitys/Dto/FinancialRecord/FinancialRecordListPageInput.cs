using QT.Common.Filter;
using System.ComponentModel.DataAnnotations;

namespace QT.Extend.Entitys.Dto.FinancialRecord;

public class FinancialRecordListPageInput: PageInputBase
{
    /// <summary>
    /// 收支分类 收入=1，支出=0.
    /// </summary>
    public int? category { get; set; }

    /// <summary>
    /// 日期范围
    /// </summary>
    public string? billDateRange { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    public string? label { get; set; }
}