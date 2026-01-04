using QT.Common.Const;
using QT.Emp.Entitys;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QT.CMS.Entitys;

/// <summary>
/// 广告位
/// </summary>
[SugarTable("cms_app_advert")]
[Tenant(ClaimConst.TENANTID)]
public class AdvertEntity
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SqlSugar.SugarColumn(IsPrimaryKey = true)]
    public int Id { get; set; }

    /// <summary>
    /// 站点ID
    /// </summary>
    [Display(Name = "站点ID")]
    public int SiteId { get; set; }

    /// <summary>
    /// 调用标识
    /// </summary>
    [Display(Name = "调用标识")]
    [StringLength(128)]
    public string? CallIndex { get; set; }

    /// <summary>
    /// 广告位名称
    /// </summary>
    [Display(Name = "广告位名称")]
    [Required]
    [StringLength(128)]
    public string? Title { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int SortId { get; set; } = 99;

    /// <summary>
    /// 创建人
    /// </summary>
    [Display(Name = "创建人")]
    [StringLength(128)]
    public string? AddBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 广告内容列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(AdvertBannerEntity.AdvertId))]
    public List<AdvertBannerEntity> Banner { get; set; }
}
