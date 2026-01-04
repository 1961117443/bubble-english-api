using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Shop;

/// <summary>
/// 商品标签(显示)
/// </summary>
public class ShopLabelDto : ShopLabelEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

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
    /// 更新人
    /// </summary>
    [Display(Name = "更新人")]
    [StringLength(128)]
    public string? updateBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Display(Name = "更新时间")]
    public DateTime? updateTime { get; set; }
}

/// <summary>
/// 商品标签(编辑)
/// </summary>
public class ShopLabelEditDto
{
    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string? title { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int sortId { get; set; } = 99;

    /// <summary>
    /// 状态0正常1禁用
    /// </summary>
    [Display(Name = "状态")]
    [Range(0, 9)]
    public byte status { get; set; } = 0;
}
