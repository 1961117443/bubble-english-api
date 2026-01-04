using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Shop;

/// <summary>
/// 积分兑换记录(显示)
/// </summary>
public class ShopConvertHistoryDto : ShopConvertHistoryEditDto
{
    [Display(Name = "自增ID")]
    public long id { get; set; }

    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 用户名
    /// </summary>
    public string? userName { get; set; }

    /// <summary>
    /// 活动标题
    /// </summary>
    public string? convertTitle { get; set; }

    /// <summary>
    /// 订单号
    /// </summary>
    public string? orderNo { get; set; }
}

/// <summary>
/// 积分兑换记录(编辑)
/// </summary>
public class ShopConvertHistoryEditDto
{
    [Display(Name = "兑换活动")]
    public long convertId { get; set; }

    [Display(Name = "所属订单")]
    public long orderId { get; set; }

    [Display(Name = "所属会员")]
    public int userId { get; set; }

    /// <summary>
    /// 状态(0进行1成功2失败)
    /// </summary>
    [Display(Name = "状态")]
    public byte status { get; set; } = 0;
}
