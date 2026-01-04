using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Systems.Entitys.Permission;

/// <summary>
/// 机构管理
/// 版 本：V3.0.0


/// 日 期：2017.09.20.
/// </summary>
[SugarTable("BASE_ORGANIZE")]
[Tenant(ClaimConst.TENANTID)]
public class OrganizeEntity : CLDEntityBase
{
    /// <summary>
    /// 机构上级.
    /// </summary>
    [SugarColumn(ColumnName = "F_PARENTID")]
    public string ParentId { get; set; }

    /// <summary>
    /// 父级组织.
    /// </summary>
    [SugarColumn(ColumnName = "F_ORGANIZEIDTREE")]
    public string OrganizeIdTree { get; set; }

    /// <summary>
    /// 机构分类【company-公司、department-部门】.
    /// </summary>
    [SugarColumn(ColumnName = "F_CATEGORY")]
    public string Category { get; set; }

    /// <summary>
    /// 机构编码.
    /// </summary>
    [SugarColumn(ColumnName = "F_ENCODE")]
    public string EnCode { get; set; }

    /// <summary>
    /// 机构名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULLNAME")]
    public string FullName { get; set; }

    /// <summary>
    /// 机构主管.
    /// </summary>
    [SugarColumn(ColumnName = "F_MANAGERID")]
    public string ManagerId { get; set; }

    /// <summary>
    /// 扩展属性.
    /// </summary>
    [SugarColumn(ColumnName = "F_PROPERTYJSON")]
    public string PropertyJson { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }
}