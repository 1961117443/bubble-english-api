using QT.Common.Contracts;
using SqlSugar;

namespace QT.Iot.Application.Entity;

/// <summary>
/// 物资入库明细实体.
/// </summary>
[SugarTable("iot_material_inrecord")]
public class MaterialInrecordEntity : CUDSlaveEntityBase
{
    /// <summary>
    /// 物资ID.
    /// </summary>
    [SugarColumn(ColumnName = "Mid")]
    public string Mid { get; set; }

    /// <summary>
    /// 入库数量.
    /// </summary>
    [SugarColumn(ColumnName = "InNum")]
    public decimal InNum { get; set; }

    /// <summary>
    /// 入库单价.
    /// </summary>
    [SugarColumn(ColumnName = "Price")]
    public decimal Price { get; set; }

    /// <summary>
    /// 入库金额.
    /// </summary>
    [SugarColumn(ColumnName = "Amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 仓库.
    /// </summary>
    [SugarColumn(ColumnName = "StoreRomeId")]
    public string StoreRomeId { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }
}