using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 促销活动
/// </summary>
[SugarTable("cms_shop_promotion")]
//[Tenant(ClaimConst.TENANTID)]
public class ShopPromotion
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属站点ID
    /// </summary>
    [Display(Name = "所属站点")]
    public int SiteId { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    [Display(Name = "开始时间")]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [Display(Name = "结束时间")]
    public DateTime EndTime { get; set; }

    /// <summary>
    /// 消费额度
    /// </summary>
    [Display(Name = "消费额度")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Condition { get; set; }

    /// <summary>
    /// 1减金额
    /// 2奖励折扣
    /// 3赠送积分
    /// 4赠送优惠券
    /// 5赠送赠品
    /// 6免运费
    /// </summary>
    [Display(Name = "奖励类型")]
    public byte AwardType { get; set; }

    /// <summary>
    /// 奖励值
    /// </summary>
    [Display(Name = "奖励值")]
    public long AwardValue { get; set; }

    /// <summary>
    /// 活动标题
    /// </summary>
    [Display(Name = "活动标题")]
    [StringLength(128)]
    public string? Title { get; set; }

    /// <summary>
    /// 活动介绍
    /// </summary>
    [Display(Name = "活动介绍")]
    [StringLength(512)]
    public string? Remark { get; set; }

    /// <summary>
    /// 会员组ID(以逗号分隔结尾)
    /// 为空则所有会员组可参与
    /// </summary>
    [Display(Name = "会员组列表")]
    [StringLength(512)]
    public string? GroupIds { get; set; }

    /// <summary>
    /// 状态0开启1关闭
    /// </summary>
    [Display(Name = "状态")]
    [Range(0, 9)]
    public byte Status { get; set; } = 0;

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int SortId { get; set; } = 99;

    /// <summary>
    /// 创建人
    /// </summary>
    [Display(Name = "创建人")]
    [StringLength(128)]
    public string? AddBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新人
    /// </summary>
    [Display(Name = "更新人")]
    [StringLength(128)]
    public string? UpdateBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Display(Name = "更新时间")]
    public DateTime? UpdateTime { get; set; }
}
