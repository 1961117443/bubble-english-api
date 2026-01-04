using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 入驻商铺实体.
/// </summary>
[SugarTable("log_enterprise_store")]
[Tenant(ClaimConst.TENANTID)]
public class LogEnterpriseStoreEntity : CUDEntityBase
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 商家ID.
    /// </summary>
    [SugarColumn(ColumnName = "EId")]
    public string EId { get; set; }

    /// <summary>
    /// 商铺编号.
    /// </summary>
    [SugarColumn(ColumnName = "StoreNumber")]
    public string StoreNumber { get; set; }

    /// <summary>
    /// 商铺位置.
    /// </summary>
    [SugarColumn(ColumnName = "StoreLocation")]
    public string StoreLocation { get; set; }

    /// <summary>
    /// 商铺面积.
    /// </summary>
    [SugarColumn(ColumnName = "StoreArea")]
    public string StoreArea { get; set; }

    /// <summary>
    /// 商铺租金.
    /// </summary>
    [SugarColumn(ColumnName = "StoreRent")]
    public string StoreRent { get; set; }

    /// <summary>
    /// 起租时间.
    /// </summary>
    [SugarColumn(ColumnName = "LeaseStartTime")]
    public DateTime? LeaseStartTime { get; set; }

    /// <summary>
    /// 合同期限.
    /// </summary>
    [SugarColumn(ColumnName = "ContractDuration")]
    public DateTime? ContractDuration { get; set; }

}