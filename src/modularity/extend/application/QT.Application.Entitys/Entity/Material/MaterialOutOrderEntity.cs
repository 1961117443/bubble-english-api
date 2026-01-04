using QT.Common;
using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;
using System.ComponentModel;

namespace QT.Iot.Application.Entity;

/// <summary>
/// 物资出库表
/// </summary>
[SugarTable("iot_material_outorder")]
public class MaterialOutOrderEntity : CUDEntityBase
{
    /// <summary>
    /// 出库订单号.
    /// </summary>
    [SugarColumn(ColumnName = "No")]
    public string No { get; set; }

    /// <summary>
    /// 出库类型.
    /// </summary>
    [SugarColumn(ColumnName = "OutType")]
    public string OutType { get; set; }

    /// <summary>
    /// 出库日期.
    /// </summary>
    [SugarColumn(ColumnName = "OutTime")]
    public DateTime? OutTime { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }
}
