using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys.Dto.Shop;

/// <summary>
/// 促销活动(显示)
/// </summary>
public class ShopPromotionDto : ShopPromotionEditDto
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
    /// 优惠券
    /// </summary>
    public ShopCouponDto? shopCoupon { get; set; }

    /// <summary>
    /// 商品货品
    /// </summary>
    public ShopGoodsProductListDto? goodsProduct { get; set; }
}

/// <summary>
/// 促销活动(编辑)
/// </summary>
public class ShopPromotionEditDto
{
    /// <summary>
    /// 所属站点ID
    /// </summary>
    [Display(Name = "所属站点")]
    public int siteId { get; set; }

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
    /// 消费额度
    /// </summary>
    [Display(Name = "消费额度")]
    [Required(ErrorMessage = "{0}不可为空")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal condition { get; set; }

    /// <summary>
    /// 1减金额
    /// 2奖励折扣
    /// 3赠送积分
    /// 4赠送优惠券
    /// 5赠送赠品
    /// 6免运费
    /// </summary>
    [Display(Name = "奖励类型")]
    [Required(ErrorMessage = "{0}不可为空")]
    [Range(1, 6, ErrorMessage = "{0}限制{1}-{2}范围数字")]
    public byte awardType { get; set; }

    /// <summary>
    /// 奖励值
    /// </summary>
    [Display(Name = "奖励值")]
    [Required(ErrorMessage = "{0}不可为空")]
    public long awardValue { get; set; }

    /// <summary>
    /// 活动标题
    /// </summary>
    [Display(Name = "活动标题")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string? title { get; set; }

    /// <summary>
    /// 活动简介
    /// </summary>
    [Display(Name = "活动简介")]
    [StringLength(512)]
    public string? remark { get; set; }

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
    [Range(0, 9)]
    public byte status { get; set; } = 0;

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int sortId { get; set; } = 99;
}
