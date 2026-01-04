using System.ComponentModel.DataAnnotations;
namespace QT.CMS.Entitys.Dto.Order;

/// <summary>
/// 订单跟踪
/// </summary>
public class OrderLogDto
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
    [Required]
    [StringLength(128)]
    public string? orderNo { get; set; }

    /// <summary>
    /// 动作类型
    /// </summary>
    [Display(Name = "动作类型")]
    [StringLength(128)]
    public string? actionType { get; set; }

    /// <summary>
    /// 备注说明
    /// </summary>
    [Display(Name = "备注说明")]
    [StringLength(512)]
    public string? remark { get; set; }

    /// <summary>
    /// 操作人
    /// </summary>
    [Display(Name = "操作人")]
    [StringLength(128)]
    public string? addBy { get; set; }

    /// <summary>
    /// 操作时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; } = DateTime.Now;
}
