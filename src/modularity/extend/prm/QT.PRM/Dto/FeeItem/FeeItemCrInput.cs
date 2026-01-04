using QT.DependencyInjection;
using QT.PRM.Entitys;
using System.ComponentModel.DataAnnotations;

namespace QT.PRM.Dto.FeeItem;

/// <summary>
/// 收费项目创建输入
/// </summary>
[SuppressSniffer]
public class FeeItemCrInput
{
    /// <summary>
    /// 项目名称
    /// </summary>
    [Required]
    [StringLength(50, ErrorMessage = "项目名称不能超过50字符")]
    public string name { get; set; }

    /// <summary>
    /// 项目编码
    /// </summary>
    [Required]
    [StringLength(20, ErrorMessage = "项目编码不能超过20字符")]
    public string code { get; set; }

    /// <summary>
    /// 计算方式
    /// </summary>
    [Required]
    public FeeCalcMethod calcMethod { get; set; }

    /// <summary>
    /// 单价
    /// </summary>
    [Required]
    [Range(0, 100000, ErrorMessage = "单价范围无效")]
    public decimal unitPrice { get; set; }
}
