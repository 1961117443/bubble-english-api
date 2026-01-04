using QT.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JZRC.Entitys.Dto.JzrcCompany;

/// <summary>
/// 分配客户经理入参
/// </summary>
[SuppressSniffer]
public class JzrcCompanyAllotManagerInput
{
    /// <summary>
    /// 区域
    /// </summary>
    public string province { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    public int number { get; set; }

    /// <summary>
    /// 客户经理
    /// </summary>
    [Required]
    public string managerId { get; set; }

    /// <summary>
    /// 1：按选中，2：按条件
    /// </summary>
    public int radio { get; set; }

    /// <summary>
    /// 当前选中
    /// </summary>
    public string[] ids { get; set; }
}
