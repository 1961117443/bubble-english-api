using QT.Systems.Entitys.Permission;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 商品评价
/// </summary>
[SugarTable("cms_order_evaluate")]
public class OrderEvaluate
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属订单
    /// </summary>
    [Display(Name = "所属订单")]
    public long OrderId { get; set; }

    /// <summary>
    /// 订单商品
    /// </summary>
    [Display(Name = "订单商品")]
    [ForeignKey("OrderGoods")]
    public long OrderGoodsId { get; set; }

    /// <summary>
    /// 所属用户
    /// </summary>
    [Display(Name = "所属用户")]
    [ForeignKey("User")]
    public string? UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [Display(Name = "用户名")]
    [StringLength(30)]
    public string? UserName { get; set; }

    /// <summary>
    /// 1差评2中评3好评
    /// </summary>
    [Display(Name = "商品评分")]
    public byte GoodsScore { get; set; } = 3;

    /// <summary>
    /// 服务评分(1-5星)
    /// </summary>
    [Display(Name = "服务评分")]
    public byte ServiceScore { get; set; } = 5;

    /// <summary>
    /// 物流评分(1-5星)
    /// </summary>
    [Display(Name = "物流评分")]
    public byte MatterScore { get; set; } = 5;

    /// <summary>
    /// 评价内容
    /// </summary>
    [Display(Name = "评价内容")]
    [StringLength(512)]
    public string? Content { get; set; }

    /// <summary>
    /// 状态(0正常1待审)
    /// </summary>
    [Display(Name = "状态")]
    public byte Status { get; set; } = 0;

    /// <summary>
    /// 评价时间
    /// </summary>
    [Display(Name = "评价时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;


    /// <summary>
    /// 评价图片列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(OrderEvaluateAlbum.EvaluateId))]
    public ICollection<OrderEvaluateAlbum> EvaluateAlbums { get; set; }

    /// <summary>
    /// 订单商品信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(OrderGoodsId))]
    public OrderGoods? OrderGoods { get; set; }

    /// <summary>
    /// 用户信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(UserId))]
    public UserEntity? User { get; set; }
}
