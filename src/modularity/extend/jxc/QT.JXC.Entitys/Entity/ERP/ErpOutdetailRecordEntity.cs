using QT.Common.Const;
using SqlSugar;

namespace QT.JXC.Entitys.Entity.ERP;

/// <summary>
/// 商品出库记录明细.
/// </summary>
[SugarTable("erp_outdetailrecord")]
[Tenant(ClaimConst.TENANTID)]
public class ErpOutdetailRecordEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public string Id { get; set; }

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

    /// <summary>
    /// 出库订单ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_OutId")]
    public string OutId { get; set; }

    /// <summary>
    /// 入库订单ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_InId")]
    public string InId { get; set; }

    /// <summary>
    /// 出库数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_Num")]
    public decimal Num { get; set; }

    /// <summary>
    /// 商品规格ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Gid")]
    public string Gid { get; set; }
}
