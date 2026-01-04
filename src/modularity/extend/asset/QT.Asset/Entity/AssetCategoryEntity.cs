using QT.Common.Contracts;

namespace QT.Asset.Entity;

using SqlSugar;

/// <summary>
/// 资产分类实体
/// </summary>
[SugarTable("asset_categories")]
public class AssetCategoryEntity : CLDEntityBase
{
    /// <summary>
    /// 分类名称
    /// </summary>
    [SugarColumn(ColumnName = "name", Length = 100, IsNullable = false)]
    public string Name { get; set; }

    /// <summary>
    /// 父分类ID
    /// </summary>
    [SugarColumn(ColumnName = "pid", Length = 50, IsNullable = true)]
    public string ParentId { get; set; }
}
