using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 退换货图片
/// </summary>
[SugarTable("cms_order_refund_album")]
public class OrderRefundAlbum
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属退换ID
    /// </summary>
    [Display(Name = "所属退换")]
    [ForeignKey("OrderRefund")]
    public long RefundId { get; set; }

    /// <summary>
    /// 缩略图
    /// </summary>
    [Display(Name = "缩略图")]
    [StringLength(512)]
    public string? ThumbPath { get; set; }

    /// <summary>
    /// 原图
    /// </summary>
    [Display(Name = "原图")]
    [StringLength(512)]
    public string? OriginalPath { get; set; }

    /// <summary>
    /// 图片描述
    /// </summary>
    [Display(Name = "图片描述")]
    [StringLength(512)]
    public string? Remark { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;


    /// <summary>
    /// 订单退换货信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(RefundId))]
    public OrderRefund? OrderRefund { get; set; }
}
