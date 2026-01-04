using QT.Common.Const;
using SqlSugar;

namespace QT.Application.Entitys.FreshDelivery;
/// <summary>
/// 商品出库记录实体.
/// </summary>
[SugarTable("erp_outrecord")]
[Tenant(ClaimConst.TENANTID)]
public class ErpOutrecordEntity
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
    /// 出库订单ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_OutId")]
    public string OutId { get; set; }

    /// <summary>
    /// 商品规格ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Gid")]
    public string Gid { get; set; }

    /// <summary>
    /// 销售订单ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_OrderId")]
    public string OrderId { get; set; }

    /// <summary>
    /// 数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_Num")]
    public decimal Num { get; set; }

    /// <summary>
    /// 单价.
    /// </summary>
    [SugarColumn(ColumnName = "F_Price")]
    public decimal Price { get; set; }

    /// <summary>
    /// 总价.
    /// </summary>
    [SugarColumn(ColumnName = "F_Amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 出库时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_OutTime")]
    public DateTime? OutTime { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "F_Remark")]
    public string Remark { get; set; }

    /// <summary>
    /// 仓库ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_StoreRomeId")]
    public string StoreRomeId { get; set; }

    /// <summary>
    /// 库区ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_StoreRomeAreaId")]
    public string StoreRomeAreaId { get; set; }

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
    /// 修改时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_LastModifyTime")]
    public DateTime? LastModifyTime { get; set; }

    /// <summary>
    /// 修改用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_LastModifyUserId")]
    public string LastModifyUserId { get; set; }


    /// <summary>
    /// 成本金额.
    /// </summary>
    [SugarColumn(ColumnName = "F_CostAmount")]
    public decimal CostAmount { get; set; }
}