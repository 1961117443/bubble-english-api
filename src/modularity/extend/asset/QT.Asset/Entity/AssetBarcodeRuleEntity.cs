using QT.Common.Contracts;

namespace QT.Asset.Entity;

using SqlSugar;
using System.ComponentModel;

/// <summary>
/// 条码生成规则实体
/// </summary>
[SugarTable("asset_barcode_rules")]
public class AssetBarcodeRuleEntity : CLDEntityBase
{
    /// <summary>
    /// 规则名称
    /// </summary>
    [SugarColumn(ColumnName = "rule_name", Length = 100, IsNullable = false)]
    public string RuleName { get; set; }

    /// <summary>
    /// 条码类型：Asset/Warehouse
    /// </summary>
    [SugarColumn(ColumnName = "barcode_type", Length = 20, IsNullable = false)]
    public string BarcodeType { get; set; }

    /// <summary>
    /// 编码前缀
    /// </summary>
    [SugarColumn(ColumnName = "prefix", Length = 20, IsNullable = true)]
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// 补零位数
    /// </summary>
    [SugarColumn(ColumnName = "zero_padding", IsNullable = true)]
    public int ZeroPadding { get; set; } = 5;

    /// <summary>
    /// 二维码样式设置
    /// </summary>
    [SugarColumn(ColumnName = "style", IsNullable = true)]
    public string Style { get; set; }

    /// <summary>
    /// 当前最大流水号
    /// </summary>
    [SugarColumn(ColumnName = "last_serial_number", IsNullable = true)]
    public int LastSerialNumber { get; set; } = 0;


    /// <summary>
    /// 日期格式
    /// </summary>
    [SugarColumn(ColumnName = "date_format", IsNullable = true)]
    public string DateFormat { get; set; }
}


public enum AssetBarcodeType
{
    /// <summary>
    /// 资产条码
    /// </summary>
   [Description("资产")] Asset,
    /// <summary>
    /// 仓库条码
    /// </summary>
    [Description("仓库")] Warehouse

}