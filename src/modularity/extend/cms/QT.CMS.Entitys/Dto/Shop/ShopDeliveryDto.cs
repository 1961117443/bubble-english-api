using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Shop;

/// <summary>
/// 配送方式(统计)
/// </summary>
public class ShopDeliveryTotalDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public int id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    public string? title { get; set; }

    /// <summary>
    /// 配送费用
    /// </summary>
    [Display(Name = "配送费用")]
    public decimal freight { get; set; } = 0;
}

/// <summary>
/// 配送方式(显示)
/// </summary>
public class ShopDeliveryDto : ShopDeliveryEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public int id { get; set; }

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
    /// 更新人
    /// </summary>
    [Display(Name = "更新人")]
    [StringLength(128)]
    public string? updateBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Display(Name = "更新时间")]
    public DateTime? updateTime { get; set; }
}

// <summary>
/// 配送方式(编辑)
/// </summary>
public class ShopDeliveryEditDto
{
    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string? title { get; set; }

    /// <summary>
    /// 备注说明
    /// </summary>
    [Display(Name = "备注说明")]
    [StringLength(512)]
    public string? remark { get; set; }

    /// <summary>
    /// 0先款后货
    /// 1货到付款
    /// </summary>
    [Display(Name = "配送类型")]
    [Range(0, 1, ErrorMessage = "{0}应在{1}-{2}之间")]
    public byte type { get; set; } = 0;

    /// <summary>
    /// 首重重量(公斤)
    /// </summary>
    [Display(Name = "首重重量")]
    public int firstWeight { get; set; } = 0;

    /// <summary>
    /// 首重价格
    /// </summary>
    [Display(Name = "首重价格")]
    public decimal firstPrice { get; set; } = 0;

    /// <summary>
    /// 续重重量(公斤)
    /// </summary>
    [Display(Name = "续重重量")]
    public int secondWeight { get; set; } = 0;

    /// <summary>
    /// 续重价格
    /// </summary>
    [Display(Name = "续重价格")]
    public decimal secondPrice { get; set; } = 0;

    /// <summary>
    /// 是否支持保价
    /// </summary>
    [Display(Name = "是否支持保价")]
    [Range(0, 1, ErrorMessage = "{0}应在{1}-{2}之间")]
    public byte isInsure { get; set; } = 0;

    /// <summary>
    /// 保价费率(千分之几)
    /// </summary>
    [Display(Name = "保价费率")]
    [Range(0, 1000, ErrorMessage = "{0}应在{1}-{2}之间")]
    public int insureRate { get; set; } = 0;

    /// <summary>
    /// 最低保价
    /// </summary>
    [Display(Name = "最低保价")]
    public decimal insurePrice { get; set; } = 0;

    /// <summary>
    /// 0统一设置
    /// 1指定地区
    /// </summary>
    [Display(Name = "费用类型")]
    [Range(0, 1, ErrorMessage = "{0}应在{1}-{2}之间")]
    public byte priceType { get; set; } = 0;

    /// <summary>
    /// 默认费用(0不启用1启用)
    /// </summary>
    [Display(Name = "默认费用")]
    [Range(0, 1, ErrorMessage = "{0}应在{1}-{2}之间")]
    public byte isDefault { get; set; } = 0;

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int sortId { get; set; } = 99;

    /// <summary>
    /// 状态0启用1禁用
    /// </summary>
    [Display(Name = "状态")]
    [Range(0, 1, ErrorMessage = "{0}应在{1}-{2}之间")]
    public byte status { get; set; } = 0;

    /// <summary>
    /// 地区价格列表
    /// </summary>
    public ICollection<ShopDeliveryAreaDto> deliveryAreas { get; set; } = new List<ShopDeliveryAreaDto>();
}
