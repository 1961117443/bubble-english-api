using QT.Common.Const;
using QT.Systems.Entitys.Permission;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 积分兑换记录
/// </summary>
[SugarTable("cms_shop_convert_history")]
[Tenant(ClaimConst.TENANTID)]
public class ShopConvertHistory
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 兑换活动ID
    /// </summary>
    [Display(Name = "兑换活动")]
    [ForeignKey("ShopConvert")]
    public long ConvertId { get; set; }

    /// <summary>
    /// 所属订单
    /// </summary>
    [Display(Name = "所属订单")]
    [ForeignKey("Order")]
    public long OrderId { get; set; }

    /// <summary>
    /// 所属会员
    /// </summary>
    [Display(Name = "所属会员")]
    [ForeignKey("User")]
    public string? UserId { get; set; }

    /// <summary>
    /// 状态(0进行1成功2失败)
    /// </summary>
    [Display(Name = "状态")]
    public byte Status { get; set; } = 0;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 兑换活动信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(ConvertId))]
    public ShopConvert? ShopConvert { get; set; }

    /// <summary>
    /// 订单信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(OrderId))]
    public Orders? Order { get; set; }

    /// <summary>
    /// 会员信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(UserId))]
    public UserEntity? User { get; set; }
}
