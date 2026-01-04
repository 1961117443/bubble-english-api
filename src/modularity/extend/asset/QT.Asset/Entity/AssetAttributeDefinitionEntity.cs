using QT.Common.Contracts;

namespace QT.Asset.Entity;

using SqlSugar;

/// <summary>
/// 资产扩展字段定义实体
/// </summary>
[SugarTable("asset_attribute_definitions")]
public class AssetAttributeDefinitionEntity : CLDEntityBase
{
    /// <summary>
    /// 分类ID
    /// </summary>
    [SugarColumn(ColumnName = "cid", Length = 50, IsNullable = false)]
    public string CategoryId { get; set; }

    /// <summary>
    /// 字段名（英文标识）
    /// </summary>
    [SugarColumn(ColumnName = "name", Length = 100, IsNullable = false)]
    public string Name { get; set; }

    /// <summary>
    /// 字段标题（显示名称）
    /// </summary>
    [SugarColumn(ColumnName = "title", Length = 100, IsNullable = false)]
    public string Title { get; set; }

    /// <summary>
    /// 控件类型
    /// </summary>
    [SugarColumn(ColumnName = "control_type", Length = 128, IsNullable = false)]
    public string ControlType { get; set; }

    /// <summary>
    /// 选项配置（JSON格式）
    /// </summary>
    [SugarColumn(ColumnName = "item_option", IsNullable = true)]
    public string ItemOption { get; set; }

    /// <summary>
    /// 默认值
    /// </summary>
    [SugarColumn(ColumnName = "default_value", Length = 512, IsNullable = true)]
    public string DefaultValue { get; set; }

    /// <summary>
    /// 是否密码字段
    /// </summary>
    [SugarColumn(ColumnName = "is_password", IsNullable = false)]
    public bool IsPassword { get; set; }

    /// <summary>
    /// 是否必填
    /// </summary>
    [SugarColumn(ColumnName = "is_required", IsNullable = false)]
    public bool IsRequired { get; set; }

    /// <summary>
    /// 编辑器类型
    /// </summary>
    [SugarColumn(ColumnName = "editor_type", IsNullable = false)]
    public int EditorType { get; set; }

    /// <summary>
    /// 验证提示信息
    /// </summary>
    [SugarColumn(ColumnName = "valid_tip_msg", Length = 255, IsNullable = true)]
    public string ValidTipMsg { get; set; }

    /// <summary>
    /// 验证错误信息
    /// </summary>
    [SugarColumn(ColumnName = "valid_error_msg", Length = 255, IsNullable = true)]
    public string ValidErrorMsg { get; set; }

    /// <summary>
    /// 验证正则表达式
    /// </summary>
    [SugarColumn(ColumnName = "valid_pattern", Length = 255, IsNullable = true)]
    public string ValidPattern { get; set; }

    /// <summary>
    /// 排序ID
    /// </summary>
    [SugarColumn(ColumnName = "sort_id", IsNullable = true)]
    public int? SortId { get; set; }
}