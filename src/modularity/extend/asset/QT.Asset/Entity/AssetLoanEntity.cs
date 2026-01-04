using QT.Common.Contracts;

namespace QT.Asset.Entity;

using SqlSugar;
using System;

/// <summary>
/// 资产领用记录实体
/// </summary>
[SugarTable("asset_loans")]
public class AssetLoanEntity : CLDEntityBase
{
    /// <summary>
    /// 资产ID
    /// </summary>
    [SugarColumn(ColumnName = "asset_id", Length = 50, IsNullable = false)]
    public string AssetId { get; set; }

    /// <summary>
    /// 使用人ID
    /// </summary>
    [SugarColumn(ColumnName = "user_id", Length = 50, IsNullable = false)]
    public string UserId { get; set; }

    /// <summary>
    /// 领用时间
    /// </summary>
    [SugarColumn(ColumnName = "loan_time", IsNullable = false)]
    public DateTime LoanTime { get; set; }

    /// <summary>
    /// 归还时间
    /// </summary>
    [SugarColumn(ColumnName = "return_time", IsNullable = true)]
    public DateTime? ReturnTime { get; set; }

    /// <summary>
    /// 状态：在用/归还
    /// </summary>
    [SugarColumn(ColumnName = "status", Length = 20, IsNullable = true)]
    public string Status { get; set; } = "在用";
}
