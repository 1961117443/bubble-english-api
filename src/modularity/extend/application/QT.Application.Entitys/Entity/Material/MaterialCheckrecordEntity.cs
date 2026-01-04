using QT.Common;
using QT.Common.Contracts;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Iot.Application.Entity;

/// <summary>
/// 物资类别
/// </summary>
[SugarTable("iot_material_checkrecord")]
public class MaterialCheckrecordEntity : CUDSlaveEntityBase
{
    /// <summary>
    /// 物品ID.
    /// </summary>
    [SugarColumn(ColumnName = "Mid")]
    public string Mid { get; set; }

    /// <summary>
    /// 系统数量.
    /// </summary>
    [SugarColumn(ColumnName = "SystemNum")]
    public decimal SystemNum { get; set; }

    /// <summary>
    /// 实际数量.
    /// </summary>
    [SugarColumn(ColumnName = "RealNum")]
    public decimal RealNum { get; set; }

    /// <summary>
    /// 拆损数量.
    /// </summary>
    [SugarColumn(ColumnName = "LoseNum")]
    public decimal LoseNum { get; set; }

    /// <summary>
    /// 仓库ID.
    /// </summary>
    [SugarColumn(ColumnName = "StoreRomeId")]
    public string StoreRomeId { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }
}
