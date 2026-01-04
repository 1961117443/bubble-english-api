using QT.Common.Const;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 限时抢购
/// </summary>
[SugarTable("cms_shop_speed")]
[Tenant(ClaimConst.TENANTID)]
public class ShopSpeed
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属站点
    /// </summary>
    [Display(Name = "所属站点")]
    public int SiteId { get; set; }

    /// <summary>
    /// 抢购标题
    /// </summary>
    [Display(Name = "抢购标题")]
    [StringLength(512)]
    public string? Title { get; set; }

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
    /// 货品ID
    /// </summary>
    [Display(Name = "货品ID")]
    [ForeignKey("GoodsProduct")]
    public long ProductId { get; set; }

    /// <summary>
    /// 抢购价格
    /// </summary>
    [Display(Name = "抢购价格")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; } = 0;

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


    /// <summary>
    /// 商品货品
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(ProductId))]
    public ShopGoodsProduct? GoodsProduct { get; set; }
}
