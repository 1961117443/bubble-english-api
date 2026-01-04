using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Shop;

/// <summary>
/// 扩展属性模型(显示)
/// </summary>
public class ShopFieldDto : ShopFieldEditDto
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
}

/// <summary>
/// 扩展属性模型(编辑)
/// </summary>
public class ShopFieldEditDto
{
    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string? title { get; set; }

    /// <summary>
    /// 扩展属性选项列表
    /// </summary>
    public ICollection<ShopFieldOptionDto> options { get; set; } = new List<ShopFieldOptionDto>();
}
