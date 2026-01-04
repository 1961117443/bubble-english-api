using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 订单分账配置表实体.
/// </summary>
[SugarTable("log_order_financial_configuration")]
[Tenant(ClaimConst.TENANTID)]
public class LogOrderFinancialConfigurationEntity: CUDEntityBase
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public string Id { get; set; }

    /// <summary>
    /// 配置名称.
    /// </summary>
    [SugarColumn(ColumnName = "Name")]
    public string Name { get; set; }

    /// <summary>
    /// 平台占比.
    /// </summary>
    [SugarColumn(ColumnName = "PlatformProportion")]
    public decimal PlatformProportion { get; set; }

    /// <summary>
    /// 收件点占比.
    /// </summary>
    [SugarColumn(ColumnName = "PointProportion")]
    public decimal PointProportion { get; set; }

    /// <summary>
    /// 启用状态.
    /// </summary>
    [SugarColumn(ColumnName = "Status")]
    public int? Status { get; set; }

    /// <summary>
    /// 配置说明.
    /// </summary>
    [SugarColumn(ColumnName = "Description")]
    public string Description { get; set; }


    /// <summary>
    /// 到达点占比.
    /// </summary>
    [SugarColumn(ColumnName = "ReachPointProportion")]
    public decimal ReachPointProportion { get; set; }
}