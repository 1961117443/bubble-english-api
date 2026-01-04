using QT.DependencyInjection;
using QT.PRM.Entitys;
using System.ComponentModel.DataAnnotations;

namespace QT.PRM.Dto.Room;

/// <summary>
/// 房间创建输入
/// </summary>
[SuppressSniffer]
public class RoomCrInput
{
    /// <summary>
    /// 楼栋ID
    /// </summary>
    [Required]
    public string buildingId { get; set; }

    /// <summary>
    /// 房间号
    /// </summary>
    [Required]
    [StringLength(20, ErrorMessage = "房间号不能超过20字符")]
    public string roomNumber { get; set; }

    /// <summary>
    /// 面积
    /// </summary>
    [Range(0, 10000, ErrorMessage = "面积范围无效")]
    public decimal? area { get; set; }

    /// <summary>
    /// 用途类型
    /// </summary>
    [Required]
    public int usageType { get; set; } // 住宅/写字楼/商铺/厂房


    /// <summary>
    /// 附件图片
    /// </summary>
    public string attachmentJson { get; set; }
}
