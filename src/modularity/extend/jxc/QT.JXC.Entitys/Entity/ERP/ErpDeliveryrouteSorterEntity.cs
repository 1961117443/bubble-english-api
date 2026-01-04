using QT.Common.Const;
using QT.Extras.DatabaseAccessor.SqlSugar;
using SqlSugar;

namespace QT.JXC.Entitys.Entity.ERP;

/// <summary>
/// 线路分拣员（中间表）实体.
/// </summary>
[SugarTable("erp_deliveryroute_sorter")]
[Tenant(ClaimConst.TENANTID)]
public class ErpDeliveryrouteSorterEntity: ICompanyEntity
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
    /// 分拣员ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Sid")]
    public string Sid { get; set; }

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

    [Navigate(NavigateType.OneToOne, nameof(Sid))]
    public ErpSorterEntity ErpSorter { get; set; }

    [Navigate(NavigateType.OneToOne, nameof(Did))]
    public ErpDeliveryrouteEntity ErpDeliveryroute { get; set; }

}