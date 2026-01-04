using QT.Common.Const;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 配送方式
/// </summary>
[SugarTable("cms_shop_delivery")]
[Tenant(ClaimConst.TENANTID)]
public class ShopDelivery
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    [StringLength(128)]
    public string? Title { get; set; }

    /// <summary>
    /// 备注说明
    /// </summary>
    [Display(Name = "备注说明")]
    [StringLength(512)]
    public string? Remark { get; set; }

    /// <summary>
    /// 首重重量(公斤)
    /// </summary>
    [Display(Name = "首重重量")]
    public int FirstWeight { get; set; } = 0;

    /// <summary>
    /// 首重价格
    /// </summary>
    [Display(Name = "首重价格")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal FirstPrice { get; set; } = 0;

    /// <summary>
    /// 续重重量(公斤)
    /// </summary>
    [Display(Name = "续重重量")]
    public int SecondWeight { get; set; } = 0;

    /// <summary>
    /// 续重价格
    /// </summary>
    [Display(Name = "续重价格")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SecondPrice { get; set; } = 0;

    /// <summary>
    /// 是否支持保价
    /// </summary>
    [Display(Name = "是否支持保价")]
    public byte IsInsure { get; set; } = 0;

    /// <summary>
    /// 保价费率
    /// </summary>
    [Display(Name = "保价费率")]
    public int InsureRate { get; set; } = 0;

    /// <summary>
    /// 最低保价费用
    /// </summary>
    [Display(Name = "最低保价")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal InsurePrice { get; set; } = 0;

    /// <summary>
    /// 0统一设置
    /// 1指定地区
    /// </summary>
    [Display(Name = "费用类型")]
    public byte PriceType { get; set; } = 0;

    /// <summary>
    /// 默认费用(0不启用1启用)
    /// </summary>
    [Display(Name = "默认费用")]
    public byte IsDefault { get; set; } = 0;

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int SortId { get; set; } = 99;

    /// <summary>
    /// 状态0启用1禁用
    /// </summary>
    [Display(Name = "状态")]
    [Range(0, 9)]
    public byte Status { get; set; } = 0;

    /// <summary>
    /// 创建人
    /// </summary>
    [Display(Name = "创建人")]
    [StringLength(30)]
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
    [StringLength(30)]
    public string? UpdateBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Display(Name = "更新时间")]
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 地区价格列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(ShopDeliveryArea.DeliveryId))]
    public List<ShopDeliveryArea> DeliveryAreas { get; set; }
}
