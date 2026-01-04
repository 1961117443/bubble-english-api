using QT.CMS.Entitys.Dto.Shop;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys.Dto.Order;

/// <summary>
/// 订单发货(编辑)
/// </summary>
public class OrderDeliveryDto : OrderDeliveryEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public string id { get; set; }

    /// <summary>
    /// 订单号
    /// </summary>
    [Display(Name = "订单号")]
    [StringLength(128)]
    public string? orderNo { get; set; }

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
    /// 快递公司信息
    /// </summary>
    public ShopExpressDto? shopExpress { get; set; }
}

/// <summary>
/// 订单发货(编辑)
/// </summary>
public class OrderDeliveryEditDto
{
    /// <summary>
    /// 所属订单ID
    /// </summary>
    [Display(Name = "所属订单")]
    public long orderId { get; set; }

    /// <summary>
    /// 收货人
    /// </summary>
    [Display(Name = "收货人")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(30)]
    public string? acceptName { get; set; }

    /// <summary>
    /// 固定电话
    /// </summary>
    [Display(Name = "固定电话")]
    [StringLength(30)]
    public string? telPhone { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    [Display(Name = "手机号码")]
    [Required(ErrorMessage = "{0}不可为空")]
    [RegularExpression(@"^(13|14|15|16|18|19|17)\d{9}$")]
    [StringLength(30)]
    public string? mobile { get; set; }

    /// <summary>
    /// 所属省份
    /// </summary>
    [Display(Name = "所属省份")]
    public string? province { get; set; }

    /// <summary>
    /// 所属城市
    /// </summary>
    [Display(Name = "所属城市")]
    public string? city { get; set; }

    /// <summary>
    /// 所属地区
    /// </summary>
    [Display(Name = "所属地区")]
    public string? area { get; set; }

    /// <summary>
    /// 详细地址
    /// </summary>
    [Display(Name = "详细地址")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(512)]
    public string? address { get; set; }

    /// <summary>
    /// 运费
    /// </summary>
    [Display(Name = "运费")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal freight { get; set; } = 0M;

    /// <summary>
    /// 快递公司
    /// </summary>
    [Display(Name = "快递公司")]
    public int expressId { get; set; }

    /// <summary>
    /// 快递单号
    /// </summary>
    [Display(Name = "快递单号")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string? expressCode { get; set; }

    /// <summary>
    /// 备注说明
    /// </summary>
    [Display(Name = "备注说明")]
    [StringLength(512)]
    public string? remark { get; set; }

    /// <summary>
    /// 订单商品列表
    /// </summary>
    public ICollection<OrderDeliveryGoodsDto> deliveryGoods { get; set; } = new List<OrderDeliveryGoodsDto>();
}
