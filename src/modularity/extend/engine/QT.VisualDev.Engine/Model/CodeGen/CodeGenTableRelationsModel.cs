using QT.DependencyInjection;

namespace QT.VisualDev.Engine.Model.CodeGen;

/// <summary>
/// 代码生成表关系模型.
/// </summary>
[SuppressSniffer]
public class CodeGenTableRelationsModel
{
    /// <summary>
    /// 表名.
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// 原始表名称.
    /// </summary>
    public string OriginalTableName { get; set; }

    /// <summary>
    /// 控件绑定模型.
    /// </summary>
    public string ControlModel { get; set; }

    /// <summary>
    /// 表名(首字母小写).
    /// </summary>
    public string LowerTableName => string.IsNullOrWhiteSpace(TableName) ? null : TableName.Substring(0, 1).ToLower() + TableName[1..];

    /// <summary>
    /// 主键.
    /// </summary>
    public string PrimaryKey { get; set; }

    /// <summary>
    /// 表描述.
    /// </summary>
    public string TableComment { get; set; }

    /// <summary>
    /// 外键字段.
    /// </summary>
    public string TableField { get; set; }

    /// <summary>
    /// 关联主键.
    /// </summary>
    public string RelationField { get; set; }

    /// <summary>
    /// 关联主键.
    /// </summary>
    public string LowerRelationField => string.IsNullOrWhiteSpace(RelationField) ? null : RelationField.Substring(0, 1).ToLower() + RelationField[1..];

    /// <summary>
    /// 子表控件配置.
    /// </summary>
    public List<TableColumnConfigModel> ChilderColumnConfigList { get; set; }

    /// <summary>
    /// 编号.
    /// </summary>
    public int TableNo { get; set; }
}