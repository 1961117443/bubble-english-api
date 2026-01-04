using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Shop;

/// <summary>
/// 积分兑换(显示)
/// </summary>
public class ShopConvertDto : ShopConvertEditDto
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

    /// <summary>
    /// 更新人
    /// </summary>
    [Display(Name = "更新人")]
    [StringLength(30)]
    public string? updateBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Display(Name = "更新时间")]
    public DateTime? updateTime { get; set; }

    /// <summary>
    /// 商品货品
    /// </summary>
    public ShopGoodsProductListDto? goodsProduct { get; set; }
}

/// <summary>
/// 积分兑换(编辑)
/// </summary>
public class ShopConvertEditDto
{
    /// <summary>
    /// 站点ID
    /// </summary>
    [Display(Name = "所属站点")]
    public int siteId { get; set; }

    /// <summary>
    /// 货品ID
    /// </summary>
    [Display(Name = "货品ID")]
    [Required(ErrorMessage = "{0}不可为空")]
    public long productId { get; set; }

    /// <summary>
    /// 活动标题
    /// </summary>
    [Display(Name = "活动标题")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string? title { get; set; }

    /// <summary>
    /// 会员组ID(以逗号分隔结尾)
    /// </summary>
    [Display(Name = "参考会员组")]
    [StringLength(512)]
    public string? groupIds { get; set; }

    /// <summary>
    /// 兑换积分
    /// </summary>
    [Display(Name = "兑换积分")]
    [Required(ErrorMessage = "{0}不可为空")]
    public int point { get; set; } = 0;

    /// <summary>
    /// 状态0开启1关闭
    /// </summary>
    [Display(Name = "状态")]
    [Range(0, 9)]
    public byte status { get; set; } = 0;

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int sortId { get; set; } = 99;
}
