using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Shop;

/// <summary>
/// 商品规格(列表)
/// </summary>
public class ShopSpecListDto : ShopSpecBaseDto
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
    [StringLength(30)]
    public string? addBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; } = DateTime.Now;
}

/// <summary>
/// 商品规格(显示)
/// </summary>
public class ShopSpecDto : ShopSpecBaseDto
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
    /// 子规格列表
    /// </summary>
    public ICollection<ShopSpecChildrenDto> children { get; set; } = new List<ShopSpecChildrenDto>();
}

/// <summary>
/// 商品规格(编辑)
/// </summary>
public class ShopSpecEditDto : ShopSpecBaseDto
{
    /// <summary>
    /// 规格值列表
    /// </summary>
    public ICollection<ShopSpecChildrenDto> children { get; set; } = new List<ShopSpecChildrenDto>();
}

/// <summary>
/// 商品规格(规格值)
/// </summary>
public class ShopSpecChildrenDto : ShopSpecBaseDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 父规格ID
    /// </summary>
    [Display(Name = "父规格")]
    public long parentId { get; set; } = 0;
}

/// <summary>
/// 商品规格(公共)
/// </summary>
public class ShopSpecBaseDto
{
    /// <summary>
    /// 规格标题
    /// </summary>
    [Display(Name = "标题")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string? title { get; set; }

    /// <summary>
    /// 规格图片
    /// </summary>
    [Display(Name = "规格图片")]
    [StringLength(512)]
    public string? imgUrl { get; set; }

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
}
