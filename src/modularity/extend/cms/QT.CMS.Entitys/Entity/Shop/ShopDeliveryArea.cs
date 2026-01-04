using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QT.CMS.Entitys;

/// <summary>
/// 配送方式价格
/// </summary>
[SugarTable("cms_shop_delivery_area")]
[Tenant(ClaimConst.TENANTID)]
public class ShopDeliveryArea
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 配送方式ID
    /// </summary>
    [Display(Name = "所属配送方式")]
    [ForeignKey("Delivery")]
    public int DeliveryId { get; set; }

    /// <summary>
    /// 所属省份
    /// </summary>
    [Display(Name = "所属省份")]
    [StringLength(30)]
    public string? Province { get; set; }

    /// <summary>
    /// 首重价格
    /// </summary>
    [Display(Name = "首重价格")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal FirstPrice { get; set; } = 0;

    /// <summary>
    /// 续重价格
    /// </summary>
    [Display(Name = "续重价格")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SecondPrice { get; set; } = 0;

    /// <summary>
    /// 配送方式
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(DeliveryId))]
    public ShopDelivery? Delivery { get; set; }
}
