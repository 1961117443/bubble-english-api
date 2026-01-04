using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace QT.Extend.Entitys.Dto.Label;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class LabelCrInput
{
    /// <summary>
    /// 标签分类.
    /// </summary>
    [Required]
    public string? category { get; set; }


    /// <summary>
    /// 标签描述.
    /// </summary>
    [Required]
    public string? fullName { get; set; }
}
