using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Order;

/// <summary>
/// 商品评价图片
/// </summary>
public class OrderEvaluateAlbumDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 所属评价ID
    /// </summary>
    [Display(Name = "所属评价")]
    public long evaluateId { get; set; }

    /// <summary>
    /// 缩略图
    /// </summary>
    [Display(Name = "缩略图")]
    [StringLength(512)]
    public string? thumbPath { get; set; }

    /// <summary>
    /// 原图
    /// </summary>
    [Display(Name = "原图")]
    [StringLength(512)]
    public string? originalPath { get; set; }

    /// <summary>
    /// 图片描述
    /// </summary>
    [Display(Name = "图片描述")]
    [StringLength(512)]
    public string? remark { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; } = DateTime.Now;
}
