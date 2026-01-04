using NPOI.SS.Formula.Functions;
using QT.Common.Const;
using QT.Extras.DatabaseAccessor.SqlSugar;
using SqlSugar;

namespace QT.JXC.Entitys.Entity.ERP;

/// <summary>
/// 路线客户（中间表）实体.
/// </summary>
[SugarTable("erp_delivery_customer")]
[Tenant(ClaimConst.TENANTID)]
public class ErpDeliveryCustomerEntity: ICompanyEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public string Id { get; set; }

    /// <summary>
    /// 公司ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Oid")]
    public string Oid { get; set; }

    /// <summary>
    /// 线路ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Did")]
    public string Did { get; set; }

    /// <summary>
    /// 客户ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Cid")]
    public string Cid { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_CreatorTime")]
    public DateTime? CreatorTime { get; set; }

    /// <summary>
    /// 创建用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_CreatorUserId")]
    public string CreatorUserId { get; set; }

    [Navigate(NavigateType.OneToOne, nameof(Cid))]
    public ErpCustomerEntity ErpCustomer { get; set; }

    [Navigate(NavigateType.OneToOne, nameof(Did))]
    public ErpDeliveryrouteEntity ErpDeliveryroute { get; set; }
}