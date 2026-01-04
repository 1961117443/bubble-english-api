using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Base;

/// <summary>
/// 支付方式(显示)
/// </summary>
public class PaymentsDto : PaymentsEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public int id { get; set; }
}

/// <summary>
/// 支付方式(编辑)
/// </summary>
public class PaymentsEditDto
{
    /// <summary>
    /// 支付名称
    /// </summary>
    [Display(Name = "支付名称")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string? title { get; set; }

    /// <summary>
    /// 显示图片
    /// </summary>
    [Display(Name = "显示图片")]
    [StringLength(512)]
    public string? imgUrl { get; set; }

    /// <summary>
    /// 备注说明
    /// </summary>
    [Display(Name = "备注说明")]
    [StringLength(512)]
    public string? remark { get; set; }

    /// <summary>
    /// 收款类型0线下1余额2微信3支付宝
    /// </summary>
    [Display(Name = "收款类型")]
    [Required(ErrorMessage = "{0}不可为空")]
    [Range(0, 9, ErrorMessage = "{0}只允许{1}-{2}整数")]
    public byte type { get; set; } = 0;

    /// <summary>
    /// 支付接口页面
    /// </summary>
    [Display(Name = "支付接口页面")]
    [StringLength(512)]
    public string? payUrl { get; set; }

    /// <summary>
    /// 支付通知页面
    /// </summary>
    [Display(Name = "支付通知页面")]
    [StringLength(512)]
    public string? notifyUrl { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int sortId { get; set; } = 99;

    /// <summary>
    /// 状态0启用1关闭
    /// </summary>
    [Display(Name = "状态")]
    public byte status { get; set; } = 0;
}
