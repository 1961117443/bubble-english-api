using QT.Common.Contracts;
using SqlSugar;

namespace QT.PRM.Entitys;

/// <summary>
/// 收费项目表
/// </summary>
[SugarTable("prm_fee_item")]
public class FeeItemEntity : CLDEntityBase
{
    /// <summary>
    /// 收费项目名称
    /// </summary>
    [SugarColumn(Length = 50, IsNullable = false)]
    public string Name { get; set; }

    /// <summary>
    /// 收费项目编码
    /// </summary>
    [SugarColumn(Length = 20, IsNullable = false)]
    public string Code { get; set; }

    /// <summary>
    /// 计算方式
    /// </summary>
    [SugarColumn(ColumnName = "calc_method")]
    public FeeCalcMethod CalcMethod { get; set; }

    /// <summary>
    /// 单价
    /// </summary>
    [SugarColumn(ColumnName = "unit_price", DecimalDigits = 2)]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 罚款率
    /// </summary>
    [SugarColumn(ColumnName = "penalty_rate", DecimalDigits = 2)]
    public decimal PenaltyRate { get; set; }
}



public enum FeeCalcMethod { 固定 = 1, 按面积 = 2, 按用量 = 3, 阶梯 = 4 }