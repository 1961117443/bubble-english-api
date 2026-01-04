using QT.Common.Const;
using SqlSugar;

namespace QT.JZRC.Entitys;

/// <summary>
/// 会员余额记录实体.
/// </summary>
[SugarTable("jzrc_member_amount_log")]
[Tenant(ClaimConst.TENANTID)]
public class JzrcMemberAmountLogEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "Id", IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>
    /// 会员id.
    /// </summary>
    [SugarColumn(ColumnName = "UserId")]
    public string UserId { get; set; }

    /// <summary>
    /// 发生金额.
    /// </summary>
    [SugarColumn(ColumnName = "Amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }

    /// <summary>
    /// 发生时间.
    /// </summary>
    [SugarColumn(ColumnName = "AddTime")]
    public DateTime? AddTime { get; set; }

    /// <summary>
    /// 类型.
    /// </summary>
    [SugarColumn(ColumnName = "Category")]
    public int? Category { get; set; }

}