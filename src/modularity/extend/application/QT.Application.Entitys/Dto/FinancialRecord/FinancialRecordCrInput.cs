using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace QT.Extend.Entitys.Dto.FinancialRecord;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class FinancialRecordCrInput
{
    /// <summary>
    /// 分类 收入=1，支出=0.
    /// </summary>
    [Required]
    public int category { get; set; }


    /// <summary>
    /// 费用说明.
    /// </summary>
    public string? remark { get; set; }

    /// <summary>
    /// 标签.
    /// </summary>
    public string? label { get; set; }

    /// <summary>
    /// 金额.
    /// </summary>
    public decimal amount { get; set; }


    /// <summary>
    /// 图片附件.
    /// </summary>
    public string? imageJson { get; set; }


    /// <summary>
    /// 日期.
    /// </summary>
    public DateTime? billDate { get; set; }


}
