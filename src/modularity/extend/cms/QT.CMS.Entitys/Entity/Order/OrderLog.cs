using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QT.CMS.Entitys;

/// <summary>
/// 订单跟踪
/// </summary>
[SugarTable("cms_order_log")]
public class OrderLog
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属订单ID
    /// </summary>
    [Display(Name = "所属订单")]
    [ForeignKey("Order")]
    public long OrderId { get; set; }

    /// <summary>
    /// 订单号
    /// </summary>
    [Display(Name = "订单号")]
    [Required]
    [StringLength(128)]
    public string? OrderNo { get; set; }

    /// <summary>
    /// 动作类型
    /// </summary>
    [Display(Name = "动作类型")]
    [StringLength(128)]
    public string? ActionType { get; set; }

    /// <summary>
    /// 备注说明
    /// </summary>
    [Display(Name = "备注说明")]
    [StringLength(512)]
    public string? Remark { get; set; }

    /// <summary>
    /// 操作人
    /// </summary>
    [Display(Name = "操作人")]
    [StringLength(128)]
    public string? AddBy { get; set; }

    /// <summary>
    /// 操作时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;


    /// <summary>
    /// 订单信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(OrderId))]
    public Orders? Order { get; set; }
}
