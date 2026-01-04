using QT.Common.Contracts;

namespace QT.Asset.Entity;

using SqlSugar;

/// <summary>
/// 资产扩展字段值实体
/// </summary>
[SugarTable("asset_attribute_values")]
public class AssetAttributeValueEntity : CLDEntityBase
{
    /// <summary>
    /// 资产ID
    /// </summary>
    [SugarColumn(ColumnName = "asset_id", Length = 50, IsNullable = false)]
    public string AssetId { get; set; }

    /// <summary>
    /// 属性定义ID
    /// </summary>
    [SugarColumn(ColumnName = "field_id", Length = 50, IsNullable = false)]
    public string FieldId { get; set; }

    /// <summary>
    /// 字段
    /// </summary>
    [SugarColumn(ColumnName = "field_name", IsNullable = true)]
    public string FieldName { get; set; }


    /// <summary>
    /// 字段值
    /// </summary>
    [SugarColumn(ColumnName = "field_value", IsNullable = true)]
    public string FieldValue { get; set; }
}
