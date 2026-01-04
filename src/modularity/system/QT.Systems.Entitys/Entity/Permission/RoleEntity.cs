using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Systems.Entitys.Permission;

/// <summary>
/// 角色信息基类.
/// </summary>
[SugarTable("BASE_ROLE")]
[Tenant(ClaimConst.TENANTID)]
public class RoleEntity : CLDEntityBase
{
    /// <summary>
    /// 获取或设置 角色名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULLNAME", ColumnDescription = "角色名称")]
    public string FullName { get; set; }

    /// <summary>
    /// 获取或设置 角色编号.
    /// </summary>
    [SugarColumn(ColumnName = "F_ENCODE", ColumnDescription = "角色编号")]
    public string EnCode { get; set; }

    /// <summary>
    /// 获取或设置 角色类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_TYPE", ColumnDescription = "角色类型")]
    public string Type { get; set; }

    /// <summary>
    /// 获取或设置 扩展属性.
    /// </summary>
    [SugarColumn(ColumnName = "F_PROPERTYJSON", ColumnDescription = "扩展属性")]
    public string PropertyJson { get; set; }

    /// <summary>
    /// 获取或设置 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION", ColumnDescription = "描述")]
    public string Description { get; set; }

    /// <summary>
    /// 获取或设置 排序.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE", ColumnDescription = "排序")]
    public long? SortCode { get; set; }

    /// <summary>
    /// 获取或设置 全局标识 1:全局 0 组织.
    /// </summary>
    [SugarColumn(ColumnName = "F_GLOBAL_MARK", ColumnDescription = "全局标识")]
    public int GlobalMark { get; set; }
}