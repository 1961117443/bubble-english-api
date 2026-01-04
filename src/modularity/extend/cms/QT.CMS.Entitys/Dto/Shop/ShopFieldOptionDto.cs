using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Shop;

/// <summary>
/// 扩展字段选项
/// </summary>
public class ShopFieldOptionDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 所属字段ID
    /// </summary>
    [Display(Name = "所属字段")]
    public long fieldId { get; set; }

    /// <summary>
    /// 属性名
    /// </summary>
    [Display(Name = "属性名")]
    [Required]
    [StringLength(128)]
    public string? name { get; set; }

    /// <summary>
    /// 控件类型(单选多选下拉框等)
    /// </summary>
    [Display(Name = "控件类型")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string? controlType { get; set; }

    /// <summary>
    /// 选项列表
    /// </summary>
    [Display(Name = "选项列表")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(512)]
    public string? itemOption { get; set; }
}
