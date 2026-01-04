using QT.CMS.Entitys.Dto.Member;
using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Order;

/// <summary>
/// 商品评价(显示)
/// </summary>
public class OrderEvaluateDto : OrderEvaluateEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public string id { get; set; }

    /// <summary>
    /// 评价时间
    /// </summary>
    [Display(Name = "评价时间")]
    public DateTime addTime { get; set; } = DateTime.Now;

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
    /// 状态(0正常1待审)
    /// </summary>
    [Display(Name = "状态")]
    public byte status { get; set; } = 0;

    /// <summary>
    /// 所属订单ID
    /// </summary>
    [Display(Name = "所属订单")]
    public long orderId { get; set; }

    /// <summary>
    /// 订单商品信息
    /// </summary>
    public OrderGoodsDto? orderGoods { get; set; }

    /// <summary>
    /// 会员信息
    /// </summary>
    public MembersDto? member { get; set; }
}

/// <summary>
/// 商品评价(编辑)
/// </summary>
public class OrderEvaluateEditDto
{
    /// <summary>
    /// 订单商品
    /// </summary>
    [Display(Name = "订单商品")]
    public long orderGoodsId { get; set; }

    /// <summary>
    /// 1差评2中评3好评
    /// </summary>
    [Display(Name = "商品评分")]
    public byte goodsScore { get; set; } = 3;

    /// <summary>
    /// 服务评分(1-5星)
    /// </summary>
    [Display(Name = "服务评分")]
    public byte serviceScore { get; set; } = 5;

    /// <summary>
    /// 物流评分(1-5星)
    /// </summary>
    [Display(Name = "物流评分")]
    public byte matterScore { get; set; } = 5;

    /// <summary>
    /// 评价内容
    /// </summary>
    [Display(Name = "评价内容")]
    [StringLength(512)]
    public string? content { get; set; }

    /// <summary>
    /// 评价图片列表
    /// </summary>
    public ICollection<OrderEvaluateAlbumDto> evaluateAlbums { get; set; } = new List<OrderEvaluateAlbumDto>();
}
