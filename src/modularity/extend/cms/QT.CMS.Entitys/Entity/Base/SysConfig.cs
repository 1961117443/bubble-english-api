using QT.Common.Const;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QT.CMS.Entitys;

/// <summary>
/// 系统配置
/// </summary>
[SugarTable("cms_sysconfig")]
[Tenant(ClaimConst.TENANTID)]
public class SysConfig
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey =true,IsIdentity =true)]
    public int Id { get; set; }

    /// <summary>
    /// 配置类型
    /// </summary>
    [Display(Name = "配置类型")]
    [Required]
    [StringLength(128)]
    public string? Type { get; set; }

    /// <summary>
    /// Json数据
    /// </summary>
    [Display(Name = "Json数据")]
    public string? JsonData { get; set; }
}
