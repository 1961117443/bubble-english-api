using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 物流园信息实体.
/// </summary>
[SugarTable("log_parkinformation")]
[Tenant(ClaimConst.TENANTID)]
public class LogParkinformationEntity : CUDEntityBase
{
    /// <summary>
    /// 自然主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 物流园名称.
    /// </summary>
    [SugarColumn(ColumnName = "Name")]
    public string Name { get; set; }

    /// <summary>
    /// 物流园地址.
    /// </summary>
    [SugarColumn(ColumnName = "Address")]
    public string Address { get; set; }

    /// <summary>
    /// 物流园简介.
    /// </summary>
    [SugarColumn(ColumnName = "Description")]
    public string Description { get; set; }

    /// <summary>
    /// 物流园电话.
    /// </summary>
    [SugarColumn(ColumnName = "Phone")]
    public string Phone { get; set; }
}