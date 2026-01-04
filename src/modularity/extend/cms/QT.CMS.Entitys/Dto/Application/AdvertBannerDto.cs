using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace QT.CMS.Entitys.Dto.Application;

/// <summary>
/// 广告内容(显示)
/// </summary>
public class AdvertBannerDto: AdvertBannerEditDto
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
    /// 广告位名称
    /// </summary>
    public string? advertTitle { get; set; }
}

/// <summary>
/// 广告内容(编辑)
/// </summary>
public class AdvertBannerEditDto
{
    /// <summary>
    /// 所属广告位
    /// </summary>
    [Display(Name = "所属广告位")]
    public int advertId { get; set; }

    /// <summary>
    /// 广告名称
    /// </summary>
    [Display(Name = "广告名称")]
    [Required(ErrorMessage = "{0}不可为空")]
    [MaxLength(128, ErrorMessage = "{0}最多{1}位字符")]
    public string? title { get; set; }

    /// <summary>
    /// 广告内容
    /// </summary>
    [Display(Name = "广告内容")]
    public string? content { get; set; }

    /// <summary>
    /// 上传文件
    /// </summary>
    [Display(Name = "上传文件")]
    [StringLength(512)]
    public string? filePath { get; set; }

    /// <summary>
    /// 链接地址
    /// </summary>
    [Display(Name = "链接地址")]
    [StringLength(512)]
    public string? linkUrl { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    [Display(Name = "开始时间")]
    [Required(ErrorMessage = "{0}不可为空")]
    public DateTime startTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [Display(Name = "结束时间")]
    [Required(ErrorMessage = "{0}不可为空")]
    public DateTime endTime { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int sortId { get; set; } = 99;

    /// <summary>
    /// 状态0关闭1开启
    /// </summary>
    [Display(Name = "状态")]
    public byte status { get; set; } = 0;
}
