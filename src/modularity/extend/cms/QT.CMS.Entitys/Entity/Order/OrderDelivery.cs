using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 订单发货
/// </summary>
[SugarTable("cms_order_delivery")]
public class OrderDelivery
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属订单ID
    /// </summary>
    [Display(Name = "所属订单")]
    public long OrderId { get; set; }

    /// <summary>
    /// 订单号
    /// </summary>
    [Display(Name = "订单号")]
    [Required]
    [StringLength(128)]
    public string? OrderNo { get; set; }

    /// <summary>
    /// 收货人
    /// </summary>
    [Display(Name = "收货人")]
    [StringLength(30)]
    public string? AcceptName { get; set; }

    /// <summary>
    /// 固定电话
    /// </summary>
    [Display(Name = "固定电话")]
    [StringLength(30)]
    public string? TelPhone { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    [Display(Name = "手机号码")]
    [StringLength(30)]
    public string? Mobile { get; set; }

    /// <summary>
    /// 省份名称
    /// </summary>
    [Display(Name = "所属省份")]
    [StringLength(30)]
    public string? Province { get; set; }

    /// <summary>
    /// 城市名称
    /// </summary>
    [Display(Name = "所属城市")]
    [StringLength(30)]
    public string? City { get; set; }

    /// <summary>
    /// 所属地区
    /// </summary>
    [Display(Name = "所属地区")]
    [StringLength(30)]
    public string? Area { get; set; }

    /// <summary>
    /// 详细地址
    /// </summary>
    [Display(Name = "详细地址")]
    [StringLength(512)]
    public string? Address { get; set; }

    /// <summary>
    /// 运费
    /// </summary>
    [Display(Name = "运费")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Freight { get; set; } = 0M;

    /// <summary>
    /// 快递公司ID
    /// </summary>
    [Display(Name = "快递公司")]
    [ForeignKey("ShopExpress")]
    public int ExpressId { get; set; }

    /// <summary>
    /// 快递单号
    /// </summary>
    [Display(Name = "快递单号")]
    [StringLength(128)]
    public string? ExpressCode { get; set; }

    /// <summary>
    /// 备注说明
    /// </summary>
    [Display(Name = "备注说明")]
    [StringLength(512)]
    public string? Remark { get; set; }

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
    /// 订单商品列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(OrderDeliveryGoods.DeliveryId), nameof(Id))]
    public List<OrderDeliveryGoods> DeliveryGoods { get; set; }

    /// <summary>
    /// 快递公司信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(ExpressId))]
    public ShopExpress? ShopExpress { get; set; }
}
