using QT.Common.Contracts;
using SqlSugar;

namespace QT.PRM.Entitys;

/// <summary>
/// 住户表
/// </summary>
[SugarTable("prm_resident")]
public class ResidentEntity : CLDEntityBase
{
    /// <summary>
    /// 住户姓名
    /// </summary>
    [SugarColumn(Length = 50, IsNullable = false)]
    public string Name { get; set; }

    /// <summary>
    /// 身份证号
    /// </summary>
    [SugarColumn(Length = 20, ColumnName = "id_card")]
    public string IdCard { get; set; }

    /// <summary>
    /// 联系电话
    /// </summary>
    [SugarColumn(Length = 20)]
    public string Phone { get; set; }

    /// <summary>
    /// 住户类型
    /// </summary>
    [SugarColumn(ColumnName = "resident_type")]
    public ResidentType ResidentType { get; set; }

    /// <summary>
    /// 入住日期
    /// </summary>
    [SugarColumn(ColumnName = "check_in_date")]
    public DateTime? CheckInDate { get; set; }

    /// <summary>
    /// 退房日期
    /// </summary>
    [SugarColumn(ColumnName = "check_out_date")]
    public DateTime? CheckOutDate { get; set; }

    /// <summary>
    /// 房间ID
    /// </summary>
    [SugarColumn(ColumnName = "room_id")]
    public string RoomId { get; set; }

    /// <summary>
    /// 紧急联系人信息
    /// </summary>
    [SugarColumn(IsJson = true, ColumnName = "emergency_contact")]
    public List<EmergencyContactInfo> EmergencyContact { get; set; }
}


/// <summary>
/// 紧急联系人信息
/// </summary>
public class EmergencyContactInfo
{
    /// <summary>
    /// 紧急联系人姓名
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 紧急联系人电话
    /// </summary>
    public string Phone { get; set; }
}



public enum ResidentType { 业主 = 1, 租户 = 2, 临时 = 3 }