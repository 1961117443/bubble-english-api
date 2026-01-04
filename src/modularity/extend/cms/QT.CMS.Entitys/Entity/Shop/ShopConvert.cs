using QT.Common.Const;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 积分兑换
/// </summary>
[SugarTable("cms_shop_convert")]
[Tenant(ClaimConst.TENANTID)]
public class ShopConvert
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 站点ID
    /// </summary>
    [Display(Name = "所属站点")]
    public int SiteId { get; set; }

    /// <summary>
    /// 货品ID
    /// </summary>
    [Display(Name = "货品ID")]
    [ForeignKey("GoodsProduct")]
    public long ProductId { get; set; }

    /// <summary>
    /// 活动标题
    /// </summary>
    [Display(Name = "活动标题")]
    [StringLength(128)]
    public string? Title { get; set; }

    /// <summary>
    /// 会员组ID(以逗号分隔结尾)
    /// </summary>
    [Display(Name = "参考会员组")]
    [StringLength(512)]
    public string? GroupIds { get; set; }

    /// <summary>
    /// 兑换积分
    /// </summary>
    [Display(Name = "兑换积分")]
    public int Point { get; set; } = 0;

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

    /// <summary>
    /// 商品货品
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(ProductId))]
    public ShopGoodsProduct? GoodsProduct { get; set; }
}
