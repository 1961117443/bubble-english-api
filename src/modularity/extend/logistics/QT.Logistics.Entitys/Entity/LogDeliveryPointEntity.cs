using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 配送点管理实体.
/// </summary>
[SugarTable("log_delivery_point")]
[Tenant(ClaimConst.TENANTID)]
public class LogDeliveryPointEntity : CUDEntityBase
{
    /// <summary>
    /// 自然主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    [SugarColumn(ColumnName = "Name")]
    public string Name { get; set; }

    /// <summary>
    /// 地址.
    /// </summary>
    [SugarColumn(ColumnName = "Address")]
    public string Address { get; set; }

    /// <summary>
    /// 电话.
    /// </summary>
    [SugarColumn(ColumnName = "Phone")]
    public string Phone { get; set; }

    /// <summary>
    /// 负责人.
    /// </summary>
    [SugarColumn(ColumnName = "Leader")]
    public string Leader { get; set; }

    /// <summary>
    /// 编号.
    /// </summary>
    [SugarColumn(ColumnName = "Code")]
    public string Code { get; set; }

    /// <summary>
    /// 配送点管理员ID.
    /// </summary>
    [SugarColumn(ColumnName = "AdminId")]
    public string AdminId { get; set; }
}