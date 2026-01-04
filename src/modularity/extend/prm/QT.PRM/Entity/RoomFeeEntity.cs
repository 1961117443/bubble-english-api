using QT.Common.Contracts;
using SqlSugar;

namespace QT.PRM.Entitys;

/// <summary>
/// 房间收费绑定表
/// </summary>
[SugarTable("prm_room_fee")]
public class RoomFeeEntity : CLDEntityBase
{
    /// <summary>
    /// 房间ID
    /// </summary>
    [SugarColumn(ColumnName = "room_id")]
    public string RoomId { get; set; }

    /// <summary>
    /// 收费项目ID
    /// </summary>
    [SugarColumn(ColumnName = "fee_item_id")]
    public string FeeId { get; set; }

    /// <summary>
    /// 开始日期
    /// </summary>
    [SugarColumn(ColumnName = "start_date")]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 收费周期
    /// </summary>
    [SugarColumn]
    public FeeCycle Cycle { get; set; } = FeeCycle.月;
}



/// <summary>
/// 收费周期枚举
/// </summary>
public enum FeeCycle { 月 = 1, 季 = 2, 年 = 3 }
