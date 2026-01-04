using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Extend.Entitys;

/// <summary>
/// 分组信息基类.
/// </summary>
[SugarTable("EXT_TEAM")]
[Tenant(ClaimConst.TENANTID)]
public class TeamEntity : CLDEntityBase
{
    /// <summary>
    /// 获取或设置 分组名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULLNAME", ColumnDescription = "团队名称")]
    public string FullName { get; set; }

    /// <summary>
    /// 获取或设置 分组编号.
    /// </summary>
    [SugarColumn(ColumnName = "F_ENCODE", ColumnDescription = "团队编号")]
    public string EnCode { get; set; }

    /// <summary>
    /// 获取或设置 分组类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_TYPE", ColumnDescription = "团队类型")]
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


    /// <summary>
    /// 负责人.
    /// </summary>
    [SugarColumn(ColumnName = "F_MANAGERIDS")]
    public string? ManagerIds { get; set; }

    /// <summary>
    /// 参与人.
    /// </summary>
    [SugarColumn(ColumnName = "F_MEMBERIDS")]
    public string? MemberIds { get; set; }
}