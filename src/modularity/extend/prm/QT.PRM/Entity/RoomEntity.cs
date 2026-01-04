using QT.Common.Contracts;
using SqlSugar;

namespace QT.PRM.Entitys;

/// <summary>
/// 房间表
/// </summary>
[SugarTable("prm_room")]
public class RoomEntity : CLDEntityBase
{
    /// <summary>
    /// 楼栋ID
    /// </summary>
    [SugarColumn(ColumnName = "building_id")]
    public string BuildingId { get; set; }

    /// <summary>
    /// 房间号
    /// </summary>
    [SugarColumn(Length = 20, IsNullable = false, ColumnName = "room_number")]
    public string RoomNumber { get; set; }

    /// <summary>
    /// 面积
    /// </summary>
    [SugarColumn(DecimalDigits = 2)]
    public decimal Area { get; set; }

    /// <summary>
    /// 用途类型
    /// </summary>
    [SugarColumn(ColumnName = "usage_type")]
    public RoomUsageType UsageType { get; set; }

    /// <summary>
    /// 房间状态
    /// </summary>
    [SugarColumn]
    public RoomStatus Status { get; set; } = RoomStatus.空置;

    /// <summary>
    /// 附件JSON
    /// </summary>
    [SugarColumn]
    public string AttachmentJson { get; set; }
}


public enum RoomUsageType { 住宅 = 1, 写字楼 = 2, 商铺 = 3, 厂房 = 4 }
public enum RoomStatus { 空置 = 1, 已售 = 2, 已租 = 3 }