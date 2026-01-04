using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys.Dto.Shop;

/// <summary>
/// 优惠券(显示)
/// </summary>
public class ShopCouponDto : ShopCouponEditDto
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
/// 优惠券(编辑)
/// </summary>
public class ShopCouponEditDto
{
    /// <summary>
    /// 所属站点
    /// </summary>
    [Display(Name = "所属站点")]
    public int siteId { get; set; }

    /// <summary>
    /// 优惠券名称
    /// </summary>
    [Display(Name = "优惠券名称")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string? title { get; set; }

    /// <summary>
    /// 适用范围0全场1分类2商品
    /// </summary>
    [Display(Name = "适用范围")]
    public byte useType { get; set; } = 0;

    /// <summary>
    /// 优惠券金额
    /// </summary>
    [Display(Name = "优惠券金额")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal amount { get; set; } = 0M;

    /// <summary>
    /// 使用门槛0不限制
    /// </summary>
    [Display(Name = "使用门槛")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal minAmount { get; set; } = 0M;

    /// <summary>
    /// 兑换积分
    /// </summary>
    [Display(Name = "兑换积分")]
    public int point { get; set; } = 0;

    /// <summary>
    /// 发行数量
    /// </summary>
    [Display(Name = "发行数量")]
    public int publishCount { get; set; }

    /// <summary>
    /// 已使用数量
    /// </summary>
    [Display(Name = "已使用数量")]
    public int useCount { get; set; } = 0;

    /// <summary>
    /// 已领取数量
    /// </summary>
    [Display(Name = "已领取数量")]
    public int receiveCount { get; set; } = 0;

    /// <summary>
    /// 领取时间
    /// </summary>
    [Display(Name = "领取时间")]
    [Required(ErrorMessage = "{0}不可为空")]
    public DateTime enableTime { get; set; }

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
    /// 商品关联列表
    /// </summary>
    public List<ShopCouponGoodsRelationDto> goodsRelations { get; set; } = new List<ShopCouponGoodsRelationDto>();

    /// <summary>
    /// 分类关联列表
    /// </summary>
    public List<ShopCouponCategoryRelationDto> categoryRelations { get; set; } = new List<ShopCouponCategoryRelationDto>();
}
