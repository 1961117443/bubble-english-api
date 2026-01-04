using QT.Common.Contracts;
using SqlSugar;

namespace QT.PRM.Entitys;

/// <summary>
/// 应收记录表
/// </summary>
[SugarTable("prm_receivable")]
public class ReceivableEntity : CLDEntityBase
{
    /// <summary>
    /// 房间费用ID
    /// </summary>
    [SugarColumn(ColumnName = "room_fee_id")]
    public string RoomFeeId { get; set; }

    /// <summary>
    /// 应收金额
    /// </summary>
    [SugarColumn(ColumnName = "amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 到期日期
    /// </summary>
    [SugarColumn(ColumnName = "due_date")]
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// 应收状态
    /// </summary>
    [SugarColumn(ColumnName = "status")]
    public ReceivableStatus Status { get; set; } = ReceivableStatus.未缴;
}


/// <summary>
/// 应收状态枚举
/// </summary>
public enum ReceivableStatus { 未缴 = 1, 部分缴纳 = 2, 已缴 = 3 }
