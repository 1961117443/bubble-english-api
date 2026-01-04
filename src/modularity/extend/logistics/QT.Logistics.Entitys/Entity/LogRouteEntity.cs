using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 物流线路实体.
/// </summary>
[SugarTable("log_route")]
[Tenant(ClaimConst.TENANTID)]
public class LogRouteEntity : CUDEntityBase
{
    /// <summary>
    /// 自然主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 线路名称.
    /// </summary>
    [SugarColumn(ColumnName = "Name")]
    public string Name { get; set; }

    /// <summary>
    /// 线路编号.
    /// </summary>
    [SugarColumn(ColumnName = "Code")]
    public string Code { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "Description")]
    public string Description { get; set; }
}