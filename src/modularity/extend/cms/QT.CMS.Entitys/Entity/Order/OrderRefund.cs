using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 订单退换货
/// </summary>
[SugarTable("cms_order_refund")]
public class OrderRefund
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
    /// 1退款2换货
    /// </summary>
    [Display(Name = "售后类型")]
    public byte Type { get; set; } = 0;

    /// <summary>
    /// 所属用户ID
    /// </summary>
    [Display(Name = "所属用户")]
    public string? UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [Display(Name = "用户名")]
    [StringLength(30)]
    public string? UserName { get; set; }

    /// <summary>
    /// 申请原因
    /// </summary>
    [Display(Name = "申请原因")]
    [StringLength(512)]
    public string? ApplyReason { get; set; }

    /// <summary>
    /// 申请时间
    /// </summary>
    [Display(Name = "申请时间")]
    public DateTime ApplyTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 0已提交
    /// 1待买家发货
    /// 2待卖家发货
    /// 3已完成
    /// 4已拒绝
    /// </summary>
    [Display(Name = "处理状态")]
    public byte HandleStatus { get; set; } = 0;

    /// <summary>
    /// 处理说明
    /// </summary>
    [Display(Name = "处理说明")]
    [StringLength(512)]
    public string? HandleRemark { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "处理时间")]
    public DateTime? HandleTime { get; set; }

    /// <summary>
    /// 1账户余额
    /// 2原路返回
    /// 3线下转账
    /// </summary>
    [Display(Name = "退款方式")]
    public byte RefundMode { get; set; } = 0;

    /// <summary>
    /// 退款金额
    /// </summary>
    [Display(Name = "退款金额")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal RefundAmount { get; set; } = 0M;

    /// <summary>
    /// 会员快递公司
    /// </summary>
    [Display(Name = "会员快递公司")]
    public int? UExpressId { get; set; }

    /// <summary>
    /// 会员快递单号
    /// </summary>
    [Display(Name = "会员快递单号")]
    [StringLength(128)]
    public string? UExpressCode { get; set; }

    /// <summary>
    /// 会员发货时间
    /// </summary>
    [Display(Name = "会员发货时间")]
    public DateTime? UExpressTime { get; set; }

    /// <summary>
    /// 商家快递公司
    /// </summary>
    [Display(Name = "商家快递公司")]
    public int? SExpressId { get; set; }

    /// <summary>
    /// 商家快递单号
    /// </summary>
    [Display(Name = "商家快递单号")]
    [StringLength(128)]
    public string? SExpressCode { get; set; }

    /// <summary>
    /// 商家发货时间
    /// </summary>
    [Display(Name = "商家发货时间")]
    public DateTime? SExpressTime { get; set; }


    /// <summary>
    /// 退换货商品列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(OrderRefundGoods.RefundId), nameof(Id))]
    public List<OrderRefundGoods> RefundGoods { get; set; }

    /// <summary>
    /// 退换货图片列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(OrderRefundAlbum.RefundId), nameof(Id))]
    public List<OrderRefundAlbum> RefundAlbums { get; set; }
}
