using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Systems.Entitys.Permission;

/// <summary>
/// 分组信息基类.
/// </summary>
[SugarTable("BASE_GROUP")]
[Tenant(ClaimConst.TENANTID)]
public class GroupEntity : CLDEntityBase
{
    /// <summary>
    /// 获取或设置 分组名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULLNAME", ColumnDescription = "分组名称")]
    public string FullName { get; set; }

    /// <summary>
    /// 获取或设置 分组编号.
    /// </summary>
    [SugarColumn(ColumnName = "F_ENCODE", ColumnDescription = "分组编号")]
    public string EnCode { get; set; }

    /// <summary>
    /// 获取或设置 分组类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_TYPE", ColumnDescription = "分组类型")]
    public string Type { get; set; }

    /// <summary>
    /// 获取或设置 排序.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE", ColumnDescription = "排序")]
    public long? SortCode { get; set; }

    /// <summary>
    /// 获取或设置 说明.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION", ColumnDescription = "说明")]
    public string Description { get; set; }
}