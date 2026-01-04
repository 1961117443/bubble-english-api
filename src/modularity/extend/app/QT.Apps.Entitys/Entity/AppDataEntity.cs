using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Apps.Entitys;

/// <summary>
/// App常用数据
/// </summary>
[SugarTable("BASE_APPDATA")]
[Tenant(ClaimConst.TENANTID)]
public class AppDataEntity : CDEntityBase
{
    /// <summary>
    /// 对象类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_OBJECTTYPE")]
    public string ObjectType { get; set; }

    /// <summary>
    /// 对象主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_OBJECTID")]
    public string ObjectId { get; set; }

    /// <summary>
    /// 对象json.
    /// </summary>
    [SugarColumn(ColumnName = "F_OBJECTDATA")]
    public string ObjectData { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string Description { get; set; }
}