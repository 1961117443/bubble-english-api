using QT.JZRC.Entitys.Dto.AppService;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JZRC.Entitys.Dto.JzrcCompanyJobTalent;

/// <summary>
/// 企业人才签约入参
/// </summary>
public class JzrcCompanyJobTalentCrInput
{
    /// <summary>
    /// 企业id
    /// </summary>
    [Required]
    public string companyId { get; set; }

    /// <summary>
    /// 人才id
    /// </summary>
    [Required]
    public string talentId { get; set; }

    /// <summary>
    /// 职位id或者岗位id
    /// </summary>
    [Required]
    public string jobId { get; set; }

    /// <summary>
    /// 金额
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 分类（0：人才，1：企业）
    /// </summary>
    public AppLoginUserRole category { get; set; }

    /// <summary>
    /// 状态（0：报名，1：签约）
    /// </summary>
    public int status { get; set; }
}
