using QT.Common.Contracts;

namespace QT.Asset.Entity;

using SqlSugar;
using System;

/// <summary>
/// 资产调拨任务实体
/// </summary>
[SugarTable("asset_transfers_tasks")]
public class AssetTransferTaskEntity : CLDEntityBase
{
    /// <summary>
    /// 调拨时间
    /// </summary>
    [SugarColumn(ColumnName = "transfer_time", IsNullable = false)]
    public DateTime TransferTime { get; set; }

    /// <summary>
    /// 调拨原因
    /// </summary>
    [SugarColumn(ColumnName = "reason", Length = 255, IsNullable = true)]
    public string Reason { get; set; }
}
