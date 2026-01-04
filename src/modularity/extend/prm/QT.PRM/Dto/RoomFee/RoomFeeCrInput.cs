using QT.DependencyInjection;
using QT.PRM.Entitys;
using System.ComponentModel.DataAnnotations;

namespace QT.PRM.Dto.RoomFee;

/// <summary>
/// 房间收费创建输入
/// </summary>
[SuppressSniffer]
public class RoomFeeCrInput
{
    /// <summary>
    /// 房间ID
    /// </summary>
    [Required(ErrorMessage = "房间ID不能为空")]
    public string roomId { get; set; }

    /// <summary>
    /// 收费项目ID
    /// </summary>
    [Required(ErrorMessage = "收费项目ID不能为空")]
    public string feeId { get; set; }

    /// <summary>
    /// 开始日期
    /// </summary>
    [Required(ErrorMessage = "开始日期不能为空")]
    public DateTime? startDate { get; set; }

    /// <summary>
    /// 收费周期
    /// </summary>
    [Required(ErrorMessage = "收费周期不能为空")]
    public FeeCycle cycle { get; set; }
}
