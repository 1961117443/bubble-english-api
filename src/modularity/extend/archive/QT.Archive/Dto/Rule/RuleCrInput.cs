using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace QT.Archive.Dto.Rule;

/// <summary>
/// 楼栋创建输入
/// </summary>
[SuppressSniffer]
public class RuleCrInput
{
    /// <summary>
    /// 编号
    /// </summary>
    [Required]
    public string code { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [Required]
    public string name { get; set; }

    /// <summary>
    /// 配置信息
    /// </summary>
    public string propertyJson { get; set; }
}
