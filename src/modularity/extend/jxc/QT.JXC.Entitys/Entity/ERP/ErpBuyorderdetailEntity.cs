using QT.Common.Const;
using SqlSugar;

namespace QT.JXC.Entitys.Entity.ERP;

/// <summary>
/// 采购订单明细实体.
/// </summary>
[SugarTable("erp_buyorderdetail")]
[Tenant(ClaimConst.TENANTID)]
public class ErpBuyorderdetailEntity
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
    /// 订单ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Fid")]
    public string Fid { get; set; }

    /// <summary>
    /// 规格ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Gid")]
    public string Gid { get; set; }

    /// <summary>
    /// 预计采购数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_PlanNum")]
    public decimal PlanNum { get; set; }

    /// <summary>
    /// 采购状态.
    /// </summary>
    [SugarColumn(ColumnName = "F_BuyState")]
    public string BuyState { get; set; }

    /// <summary>
    /// 采购数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_Num")]
    public decimal? Num { get; set; }

    /// <summary>
    /// 供货商.
    /// </summary>
    [SugarColumn(ColumnName = "F_Supplier")]
    public string Supplier { get; set; }

    /// <summary>
    /// 采购时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_BuyTime")]
    public DateTime? BuyTime { get; set; }

    /// <summary>
    /// 凭据附件.
    /// </summary>
    [SugarColumn(ColumnName = "F_Proof")]
    public string Proof { get; set; }

    /// <summary>
    /// 分量统计.
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
    /// 采购单价.
    /// </summary>
    [SugarColumn(ColumnName = "F_Price")]
    public decimal Price { get; set; }

    /// <summary>
    /// 采购金额.
    /// </summary>
    [SugarColumn(ColumnName = "F_Amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 付款属性.
    /// </summary>
    [SugarColumn(ColumnName = "F_Payment")]
    public string Payment { get; set; }


    /// <summary>
    /// 采购渠道.
    /// </summary>
    [SugarColumn(ColumnName = "F_Channel")]
    public string Channel { get; set; }


    /// <summary>
    /// 入库数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_InNum")]
    public decimal? InNum { get; set; }


    /// <summary>
    /// 审核时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_AuditTime")]
    public DateTime? AuditTime { get; set; }

    /// <summary>
    /// 审核用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_AuditUserId")]
    public string AuditUserId { get; set; }

    /// <summary>
    /// 库存数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_StoreNum")]
    public decimal StoreNum { get; set; }

    
    /// <summary>
    /// 明细备注.
    /// </summary>
    [SugarColumn(ColumnName = "F_ItRemark")]
    public string ItRemark { get; set; }



    /// <summary>
    /// 仓库.
    /// </summary>
    [SugarColumn(ColumnName = "F_StoreRomeId")]
    public string StoreRomeId { get; set; }



    /// <summary>
    /// 库区.
    /// </summary>
    [SugarColumn(ColumnName = "F_StoreRomeAreaId")]
    public string StoreRomeAreaId { get; set; }

    /// <summary>
    /// 是否仓库加工.
    /// </summary>
    [SugarColumn(ColumnName = "F_WhetherProcess")]
    public int? WhetherProcess { get; set; }


    /// <summary>
    /// 采购提交人.
    /// </summary>
    [SugarColumn(ColumnName = "F_BuyUserId")]
    public string BuyUserId { get; set; }


    /// <summary>
    /// 特殊入库数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_TsNum")]
    public decimal TsNum { get; set; }

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

    /// <summary>
    /// 客户类型.
    /// </summary>
    [SugarColumn(ColumnName = "CustomerType")]
    public string CustomerType { get; set; }

    /// <summary>
    /// 销货清单图片.
    /// </summary>
    [SugarColumn(ColumnName = "SalesImageJson")]
    public string SalesImageJson { get; set; }

    /// <summary>
    /// 是否赠品
    /// </summary>
    [SugarColumn(ColumnName = "IsFree")]
    public int? IsFree { get; set; }


    /// <summary>
    /// 关联的订单明细id
    /// </summary>
    [SugarColumn(ColumnName = "OrderDetailIds")]
    public string OrderDetailIds { get; set; }
}