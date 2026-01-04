using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 商家入库订单表实体.
/// </summary>
[SugarTable("log_enterprise_inorder")]
[Tenant(ClaimConst.TENANTID)]
public class LogEnterpriseInorderEntity : CUDEntityBase, ILogEnterpriseEntity
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
    /// 入库订单号.
    /// </summary>
    [SugarColumn(ColumnName = "F_No")]
    public string No { get; set; }

    /// <summary>
    /// 入库类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_InType")]
    public string InType { get; set; }

    /// <summary>
    /// 入库日期.
    /// </summary>
    [SugarColumn(ColumnName = "F_InTime")]
    public DateTime? InTime { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "F_Remark")]
    public string Remark { get; set; }
}