using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 区域管理实体.
/// </summary>
[SugarTable("log_administrative_division")]
[Tenant(ClaimConst.TENANTID)]
public class LogAdministrativeDivisionEntity : CUDEntityBase
{
    /// <summary>
    /// 自然主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 区域名称.
    /// </summary>
    [SugarColumn(ColumnName = "Name")]
    public string Name { get; set; }

    /// <summary>
    /// 范围介绍.
    /// </summary>
    [SugarColumn(ColumnName = "Description")]
    public string Description { get; set; }

}