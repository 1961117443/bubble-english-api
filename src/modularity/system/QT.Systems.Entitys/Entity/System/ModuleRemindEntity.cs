using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Systems.Entitys.System;

/// <summary>
/// 打印模板配置



/// 日 期：2021-06-01.
/// </summary>
[SugarTable("base_moduleremind")]
[Tenant(ClaimConst.TENANTID)]
public class ModuleRemindEntity : CLDEntityBase
{
    /// <summary>
    /// 名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULLNAME")]
    public string FullName { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string Description { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }


    /// <summary>
    /// sql模板.
    /// </summary>
    [SugarColumn(ColumnName = "F_SQLTEMPLATE")]
    public string SqlTemplate { get; set; }

    /// <summary>
    /// 模块id.
    /// </summary>
    [SugarColumn(ColumnName = "F_MODULEID")]
    public string ModuleId { get; set; }
}