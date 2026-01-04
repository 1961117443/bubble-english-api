using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace QT.Archive.Dto.Building;

/// <summary>
/// 楼栋创建输入
/// </summary>
[SuppressSniffer]
public class BuildingCrInput
{
    /// <summary>
    /// 上级id
    /// </summary>
    [Required]
    public string pid { get; set; }

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
}
