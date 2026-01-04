using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys.Dto.Order;

/// <summary>
/// 订单退换货(显示)
/// </summary>
public class OrderRefundDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public string id { get; set; }

    /// <summary>
    /// 所属订单ID
    /// </summary>
    [Display(Name = "所属订单")]
    public long orderId { get; set; }

    /// <summary>
    /// 订单号
    /// </summary>
    [Display(Name = "订单号")]
    [StringLength(128)]
    public string? orderNo { get; set; }

    /// <summary>
    /// 1退款2换货
    /// </summary>
    [Display(Name = "售后类型")]
    public byte type { get; set; } = 0;

    /// <summary>
    /// 所属用户ID
    /// </summary>
    [Display(Name = "所属用户")]
    public string userId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [Display(Name = "用户名")]
    [StringLength(30)]
    public string? userName { get; set; }

    /// <summary>
    /// 申请原因
    /// </summary>
    [Display(Name = "申请原因")]
    [StringLength(512)]
    public string? applyReason { get; set; }

    /// <summary>
    /// 申请时间
    /// </summary>
    [Display(Name = "申请时间")]
    public DateTime applyTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 0已提交
    /// 1待买家发货
    /// 2待卖家发货
    /// 3已完成
    /// 4已拒绝
    /// </summary>
    [Display(Name = "处理状态")]
    public byte handleStatus { get; set; } = 0;

    /// <summary>
    /// 处理说明
    /// </summary>
    [Display(Name = "处理说明")]
    [StringLength(512)]
    public string? handleRemark { get; set; }

    /// <summary>
    /// 处理时间
    /// </summary>
    [Display(Name = "处理时间")]
    public DateTime? handleTime { get; set; }

    /// <summary>
    /// 1账户余额
    /// 2原路返回
    /// 3线下转账
    /// </summary>
    [Display(Name = "退款方式")]
    public byte refundMode { get; set; } = 0;

    /// <summary>
    /// 退款金额
    /// </summary>
    [Display(Name = "退款金额")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal refundAmount { get; set; } = 0M;

    /// <summary>
    /// 会员快递公司
    /// </summary>
    [Display(Name = "会员快递公司")]
    public int? uExpressId { get; set; }

    /// <summary>
    /// 会员快递单号
    /// </summary>
    [Display(Name = "会员快递单号")]
    [StringLength(128)]
    public string? uExpressCode { get; set; }

    /// <summary>
    /// 会员发货时间
    /// </summary>
    [Display(Name = "会员发货时间")]
    public DateTime? uExpressTime { get; set; }

    /// <summary>
    /// 商家快递公司
    /// </summary>
    [Display(Name = "商家快递公司")]
    public int? sExpressId { get; set; }

    /// <summary>
    /// 商家快递单号
    /// </summary>
    [Display(Name = "商家快递单号")]
    [StringLength(128)]
    public string? sExpressCode { get; set; }

    /// <summary>
    /// 商家发货时间
    /// </summary>
    [Display(Name = "商家发货时间")]
    public DateTime? sExpressTime { get; set; }

    /// <summary>
    /// 退换货商品列表
    /// </summary>
    public ICollection<OrderRefundGoodsDto> refundGoods { get; set; } = new List<OrderRefundGoodsDto>();

    /// <summary>
    /// 退换货图片列表
    /// </summary>
    public ICollection<OrderRefundAlbumDto> refundAlbums { get; set; } = new List<OrderRefundAlbumDto>();
}

/// <summary>
/// 申请退换货
/// </summary>
public class OrderRefundApplyDto
{
    /// <summary>
    /// 所属订单
    /// </summary>
    [Display(Name = "所属订单")]
    public long orderId { get; set; }

    /// <summary>
    /// 所属用户ID
    /// </summary>
    [Display(Name = "所属用户")]
    public string? userId { get; set; }

    /// <summary>
    /// 1退款2换货
    /// </summary>
    [Display(Name = "售后类型")]
    public byte type { get; set; } = 0;

    /// <summary>
    /// 申请原因
    /// </summary>
    [Display(Name = "申请原因")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(512)]
    public string? applyReason { get; set; }

    /// <summary>
    /// 退换货商品列表
    /// </summary>
    public ICollection<OrderRefundGoodsEditDto> refundGoods { get; set; } = new List<OrderRefundGoodsEditDto>();

    /// <summary>
    /// 退换货图片列表
    /// </summary>
    public ICollection<OrderRefundAlbumDto> refundAlbums { get; set; } = new List<OrderRefundAlbumDto>();
}

/// <summary>
/// 买家发货
/// </summary>
public class OrderRefundBuyDto
{
    /// <summary>
    /// 会员快递公司
    /// </summary>
    [Display(Name = "会员快递公司")]
    public int uExpressId { get; set; }

    /// <summary>
    /// 会员快递单号
    /// </summary>
    [Display(Name = "会员快递单号")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string? uExpressCode { get; set; }

    /// <summary>
    /// 会员发货时间
    /// </summary>
    [Display(Name = "会员发货时间")]
    public DateTime uExpressTime { get; set; } = DateTime.Now;
}

/// <summary>
/// 卖家发货
/// </summary>
public class OrderRefundSellerDto
{
    /// <summary>
    /// 商家快递公司
    /// </summary>
    [Display(Name = "商家快递公司")]
    public int sExpressId { get; set; }

    /// <summary>
    /// 商家快递单号
    /// </summary>
    [Display(Name = "商家快递单号")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string? sExpressCode { get; set; }

    /// <summary>
    /// 商家发货时间
    /// </summary>
    [Display(Name = "商家发货时间")]
    public DateTime sExpressTime { get; set; } = DateTime.Now;
}

/// <summary>
/// 卖家处理
/// </summary>
public class OrderRefundHandleDto
{
    /// <summary>
    /// 0已提交
    /// 1待买家发货
    /// 2待卖家发货
    /// 3已完成
    /// 4已拒绝
    /// </summary>
    [Display(Name = "处理状态")]
    public byte handleStatus { get; set; } = 0;

    /// <summary>
    /// 处理说明
    /// </summary>
    [Display(Name = "处理说明")]
    [StringLength(512)]
    public string? handleRemark { get; set; }

    /// <summary>
    /// 处理时间
    /// </summary>
    [Display(Name = "处理时间")]
    public DateTime? handleTime { get; set; }

    /// <summary>
    /// 1账户余额
    /// 2原路返回
    /// 3线下转账
    /// </summary>
    [Display(Name = "退款方式")]
    public byte? refundMode { get; set; } = 0;

    /// <summary>
    /// 退款金额
    /// </summary>
    [Display(Name = "退款金额")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal? refundAmount { get; set; } = 0M;
}
