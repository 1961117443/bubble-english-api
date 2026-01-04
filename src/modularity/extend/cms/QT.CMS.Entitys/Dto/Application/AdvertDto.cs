using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace QT.CMS.Entitys.Dto.Application;

/// <summary>
/// 广告位(显示)
/// </summary>
public class AdvertDto: AdvertEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public int id { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    [Display(Name = "创建人")]
    [StringLength(128)]
    public string? addBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 广告内容列表
    /// </summary>
    public ICollection<AdvertBannerDto> banner { get; set; } = new List<AdvertBannerDto>();
}

/// <summary>
/// 广告位(编辑)
/// </summary>
public class AdvertEditDto
{
    /// <summary>
    /// 站点ID
    /// </summary>
    [Display(Name = "站点ID")]
    public int siteId { get; set; }

    /// <summary>
    /// 调用标识
    /// </summary>
    [Display(Name = "调用标识")]
    [Required(ErrorMessage = "{0}不可为空")]
    [MaxLength(128, ErrorMessage = "{0}最多{1}位字符")]
    public string? callIndex { get; set; }

    /// <summary>
    /// 广告位名称
    /// </summary>
    [Display(Name = "广告位名称")]
    [Required(ErrorMessage = "{0}不可为空")]
    [MaxLength(128, ErrorMessage = "{0}最多{1}位字符")]
    public string? title { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int sortId { get; set; } = 99;
}
