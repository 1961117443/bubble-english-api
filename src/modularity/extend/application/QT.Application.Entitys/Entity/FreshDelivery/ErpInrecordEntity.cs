using QT.Common.Const;
using QT.Extras.DatabaseAccessor.SqlSugar;
using SqlSugar;

namespace QT.Application.Entitys.FreshDelivery;

/// <summary>
/// 商品入库记录实体.
/// </summary>
[SugarTable("erp_inrecord")]
[Tenant(ClaimConst.TENANTID)]
public class ErpInrecordEntity :ICompanyEntity
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
    /// 入库订单ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_InId")]
    public string InId { get; set; }

    /// <summary>
    /// 商品规格ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Gid")]
    public string Gid { get; set; }

    /// <summary>
    /// 采购订单ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Bid")]
    public string Bid { get; set; }

    /// <summary>
    /// 采购订单数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_OrderNum")]
    public decimal OrderNum { get; set; }

    /// <summary>
    /// 入库数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_InNum")]
    public decimal InNum { get; set; }

    /// <summary>
    /// 入库单价.
    /// </summary>
    [SugarColumn(ColumnName = "F_Price")]
    public decimal Price { get; set; }

    /// <summary>
    /// 入库金额.
    /// </summary>
    [SugarColumn(ColumnName = "F_Amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 入库凭据.
    /// </summary>
    [SugarColumn(ColumnName = "F_InProof")]
    public string InProof { get; set; }

    /// <summary>
    /// 入库用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_InUserId")]
    public string InUserId { get; set; }

    /// <summary>
    /// 入库审核用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_InCheckUserId")]
    public string InCheckUserId { get; set; }

    /// <summary>
    /// 入库时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_InTime")]
    public DateTime? InTime { get; set; }

    /// <summary>
    /// 仓库.
    /// </summary>
    [SugarColumn(ColumnName = "F_StoreRomeId")]
    public string StoreRomeId { get; set; }

    /// <summary>
    /// 仓库库区.
    /// </summary>
    [SugarColumn(ColumnName = "F_StoreRomeAreaId")]
    public string StoreRomeAreaId { get; set; }

    /// <summary>
    /// 剩余数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_Num")]
    public decimal Num { get; set; }

    /// <summary>
    /// 是否特殊入.
    /// </summary>
    [SugarColumn(ColumnName = "F_IsSpecial")]
    public string IsSpecial { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "F_Remark")]
    public string Remark { get; set; }

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
    /// 入库单据号.
    /// </summary>
    [SugarColumn(ColumnName = "F_No")]
    public string No { get; set; }

    /// <summary>
    /// 生产日期.
    /// </summary>
    [SugarColumn(ColumnName = "ProductionDate")]
    public DateTime? ProductionDate { get; set; }

    /// <summary>
    /// 批次号.
    /// </summary>
    [SugarColumn(ColumnName = "BatchNumber")]
    public string BatchNumber { get; set; }


    /// <summary>
    /// 保质期.
    /// </summary>
    [SugarColumn(ColumnName = "Retention")]
    public string Retention { get; set; }

    /// <summary>
    /// 质检报告.
    /// </summary>
    [SugarColumn(ColumnName = "QualityReportProof")]
    public string QualityReportProof { get; set; }
}


/// <summary>
/// 商品入库记录实体.
/// </summary>
[SugarTable("erp_inrecord_ext")]
[Tenant(ClaimConst.TENANTID)]
public class ErpInrecordExtEntity
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
    /// 审批日期.
    /// </summary>
    [SugarColumn(ColumnName = "AuditTime")]
    public DateTime? AuditTime { get; set; }

    /// <summary>
    /// 报销日期.
    /// </summary>
    [SugarColumn(ColumnName = "ExpenseTime")]
    public DateTime? ExpenseTime { get; set; }

}