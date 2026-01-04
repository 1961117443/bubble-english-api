using QT.Common.Filter;
using System.ComponentModel.DataAnnotations;

namespace QT.Extend.Entitys.Dto.Label;

public class LabelListPageInput: PageInputBase
{
    /// <summary>
    /// 标签分类.
    /// </summary>
    public string? category { get; set; }
}