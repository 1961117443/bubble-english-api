using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 商家商品出库表实体.
/// </summary>
[SugarTable("log_enterprise_outorder")]
[Tenant(ClaimConst.TENANTID)]
public class LogEnterpriseOutorderEntity : CUDEntityBase, ILogEnterpriseEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 商家ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_EId")]
    public string EId { get; set; }

    /// <summary>
    /// 出库订单号.
    /// </summary>
    [SugarColumn(ColumnName = "F_No")]
    public string No { get; set; }

    /// <summary>
    /// 出库类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_OutType")]
    public string OutType { get; set; }

    /// <summary>
    /// 出库日期.
    /// </summary>
    [SugarColumn(ColumnName = "F_OutTime")]
    public DateTime? OutTime { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "F_Remark")]
    public string Remark { get; set; }

}