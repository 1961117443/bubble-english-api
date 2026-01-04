using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Shop;

/// <summary>
/// 快递公司(显示)
/// </summary>
public class ShopExpressDto : ShopExpressEditDto
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
}

/// <summary>
/// 快递公司(编辑)
/// </summary>
public class ShopExpressEditDto
{
    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string? title { get; set; }

    /// <summary>
    /// 物流编号
    /// </summary>
    [Display(Name = "物流编号")]
    [StringLength(128)]
    public string? expressCode { get; set; }

    /// <summary>
    /// 公司网址
    /// </summary>
    [Display(Name = "公司网址")]
    [StringLength(255)]
    public string? webSite { get; set; }

    /// <summary>
    /// 备注说明
    /// </summary>
    [Display(Name = "备注说明")]
    [StringLength(512)]
    public string? remark { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int sortId { get; set; } = 99;

    /// <summary>
    /// 状态0启用1禁用
    /// </summary>
    [Display(Name = "状态")]
    [Range(0, 9)]
    public byte status { get; set; } = 0;
}
