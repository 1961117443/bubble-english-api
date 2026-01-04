using QT.Common.Contracts;


using SqlSugar;
using System;

namespace QT.Asset.Entity;
/// <summary>
/// 资产组成关系实体
/// </summary>
[SugarTable("asset_component_relations")]
public class AssetComponentRelationEntity : CLDEntityBase
{
    /// <summary>
    /// 父资产ID
    /// </summary>
    [SugarColumn(ColumnName = "parent_asset_id", Length = 50, IsNullable = false)]
    public string ParentAssetId { get; set; }

    /// <summary>
    /// 子资产ID
    /// </summary>
    [SugarColumn(ColumnName = "child_asset_id", Length = 50, IsNullable = false)]
    public string ChildAssetId { get; set; }

    /// <summary>
    /// 绑定时间
    /// </summary>
    [SugarColumn(ColumnName = "bind_time", IsNullable = true)]
    public DateTime? BindTime { get; set; }

    /// <summary>
    /// 解绑时间
    /// </summary>
    [SugarColumn(ColumnName = "unbind_time", IsNullable = true)]
    public DateTime? UnbindTime { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "remark", Length = 255, IsNullable = true)]
    public string Remark { get; set; }
}