using QT.Common;
using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;
using System.ComponentModel;

namespace QT.Iot.Application.Entity;

/// <summary>
/// 物资入库表
/// </summary>
[SugarTable("iot_material_inorder")]
public class MaterialInOrderEntity : CUDEntityBase
{
    /// <summary>
    /// 入库订单号.
    /// </summary>
    [SugarColumn(ColumnName = "No")]
    public string No { get; set; }

    /// <summary>
    /// 入库类型.
    /// </summary>
    [SugarColumn(ColumnName = "InType")]
    public string InType { get; set; }

    /// <summary>
    /// 入库日期.
    /// </summary>
    [SugarColumn(ColumnName = "InTime")]
    public DateTime? InTime { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }
}
