using QT.DependencyInjection;
using QT.PRM.Entitys;
using System.ComponentModel.DataAnnotations;

namespace QT.PRM.Dto.Parking;

/// <summary>
/// 车位创建输入
/// </summary>
[SuppressSniffer]
public class ParkingCrInput
{
    /// <summary>
    /// 房间ID
    /// </summary>
    public string roomId { get; set; }

    /// <summary>
    /// 车位编码
    /// </summary>
    [Required]
    [StringLength(20, ErrorMessage = "车位编码不能超过20字符")]
    public string code { get; set; }

    /// <summary>
    /// 车位类型
    /// </summary>
    [Required]
    public ParkingType type { get; set; } // 1固定,2临时

    /// <summary>
    /// 车位状态
    /// </summary>
    [Required]
    public ParkingStatus status { get; set; } // 1空闲,2已租,3已售

    /// <summary>
    /// 附件JSON
    /// </summary>
    [Required]
    public string attachmentJson { get; set; }
}
