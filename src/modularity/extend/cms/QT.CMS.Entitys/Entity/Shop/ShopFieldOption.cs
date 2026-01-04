using QT.Common.Const;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 扩展字段值
/// </summary>
[SugarTable("cms_shop_field_option")]
[Tenant(ClaimConst.TENANTID)]
public class ShopFieldOption
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属字段ID
    /// </summary>
    [Display(Name = "所属扩展字段")]
    [ForeignKey("ShopField")]
    public long FieldId { get; set; }

    /// <summary>
    /// 属性名
    /// </summary>
    [Display(Name = "属性名")]
    [Required]
    [StringLength(128)]
    public string? Name { get; set; }

    /// <summary>
    /// 控件类型(单选多选下拉框等)
    /// </summary>
    [Display(Name = "控件类型")]
    [Required]
    [StringLength(128)]
    public string? ControlType { get; set; }

    /// <summary>
    /// 选项列表
    /// </summary>
    [Display(Name = "选项列表")]
    [Required]
    [StringLength(512)]
    public string? ItemOption { get; set; }

    /// <summary>
    /// 扩展属性模型
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(FieldId))]
    public ShopField? ShopField { get; set; }
}
