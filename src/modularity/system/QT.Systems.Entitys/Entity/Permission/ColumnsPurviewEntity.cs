using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Systems.Entitys.Permission;

/// <summary>
/// 模块列表权限
/// 版 本：V3.3


/// 日 期：2022-03-15.
/// </summary>
[SugarTable("BASE_COLUMNSPURVIEW")]
[Tenant(ClaimConst.TENANTID)]
public class ColumnsPurviewEntity : CLDEntityBase
{
    /// <summary>
    /// 模块ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_MODULEID")]
    public string ModuleId { get; set; }

    /// <summary>
    /// 列表字段数组.
    /// </summary>
    [SugarColumn(ColumnName = "F_FIELDLIST")]
    public string FieldList { get; set; }
}