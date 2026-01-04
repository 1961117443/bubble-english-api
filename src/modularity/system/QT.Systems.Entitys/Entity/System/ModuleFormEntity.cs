using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Systems.Entitys.System;

/// <summary>
/// 表单权限



/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_MODULEFORM")]
[Tenant(ClaimConst.TENANTID)]
public class ModuleFormEntity : CLDEntityBase
{
    /// <summary>
    /// 功能上级.
    /// </summary>
    [SugarColumn(ColumnName = "F_PARENTID")]
    public string ParentId { get; set; }

    /// <summary>
    /// 功能名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULLNAME")]
    public string FullName { get; set; }

    /// <summary>
    /// 功能编码.
    /// </summary>
    [SugarColumn(ColumnName = "F_ENCODE")]
    public string EnCode { get; set; }

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

    /// <summary>
    /// 功能主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_MODULEID")]
    public string ModuleId { get; set; }

    /// <summary>
    /// 规则(0:主表，1：副表).
    /// </summary>
    [SugarColumn(ColumnName = "F_FIELDRULE")]
    public int? FieldRule { get; set; }

    /// <summary>
    /// 绑定表格Id.
    /// </summary>
    [SugarColumn(ColumnName = "F_BINDTABLE")]
    public string BindTable { get; set; }
}