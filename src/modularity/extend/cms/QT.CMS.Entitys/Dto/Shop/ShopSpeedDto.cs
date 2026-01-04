using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys.Dto.Shop;

/// <summary>
/// 限时抢购(显示)
/// </summary>
public class ShopSpeedDto: ShopSpeedEditDto
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
/// 限时抢购(编辑)
/// </summary>
public class ShopSpeedEditDto
{
    /// <summary>
    /// 所属站点
    /// </summary>
    [Display(Name = "所属站点")]
    [Required(ErrorMessage = "{0}不可为空")]
    public int siteId { get; set; }

    /// <summary>
    /// 抢购标题
    /// </summary>
    [Display(Name = "抢购标题")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(512)]
    public string? title { get; set; }

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
    /// 货品ID
    /// </summary>
    [Display(Name = "货品ID")]
    [Required(ErrorMessage = "{0}不可为空")]
    public long productId { get; set; }

    /// <summary>
    /// 抢购价格
    /// </summary>
    [Display(Name = "抢购价格")]
    [Required(ErrorMessage = "{0}不可为空")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal price { get; set; } = 0;

    /// <summary>
    /// 会员组ID(以逗号分隔结尾)
    /// 为空则所有会员组可参与
    /// </summary>
    [Display(Name = "会员组列表")]
    [StringLength(512)]
    public string? groupIds { get; set; }

    /// <summary>
    /// 状态0开启1关闭
    /// </summary>
    [Display(Name = "状态")]
    [Range(0, 9, ErrorMessage = "{0}限制{1}-{2}范围数字")]
    public byte status { get; set; } = 0;

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int sortId { get; set; } = 99;
}
