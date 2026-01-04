using QT.Common.Contracts;
using SqlSugar;

namespace QT.PRM.Entitys;

/// <summary>
/// 车位表
/// </summary>
[SugarTable("prm_parking")]
public class ParkingEntity : CLDEntityBase
{
    /// <summary>
    /// 房间ID
    /// </summary>
    [SugarColumn(ColumnName = "room_id")]
    public string RoomId { get; set; }

    /// <summary>
    /// 车位编码
    /// </summary>
    [SugarColumn(Length = 20, IsNullable = false, ColumnName = "code")]
    public string Code { get; set; }

    /// <summary>
    /// 车位类型
    /// </summary>
    [SugarColumn(ColumnName = "type")]
    public ParkingType Type { get; set; }

    /// <summary>
    /// 车位状态
    /// </summary>
    [SugarColumn]
    public ParkingStatus Status { get; set; } = ParkingStatus.空置;

    /// <summary>
    /// 附件JSON
    /// </summary>
    [SugarColumn]
    public string AttachmentJson { get; set; }
}


/// <summary>
/// 车位状态枚举
/// </summary>
public enum ParkingStatus { 空置 = 1, 已租 = 2, 已售 = 3 }
/// <summary>
/// 车位类型枚举
/// </summary>
public enum ParkingType { 固定 = 1, 临时 = 2 }
