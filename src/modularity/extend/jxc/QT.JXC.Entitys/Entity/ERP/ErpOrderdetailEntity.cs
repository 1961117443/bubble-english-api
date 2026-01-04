using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.JXC.Entitys.Entity.ERP;

/// <summary>
/// 订单商品表实体.
/// </summary>
[SugarTable("erp_orderdetail")]
[Tenant(ClaimConst.TENANTID)]
public class ErpOrderdetailEntity : CUDEntityBase
{
    ///// <summary>
    ///// 主键.
    ///// </summary>
    //[SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    //public string Id { get; set; }

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
    /// 商品规格ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Mid")]
    public string Mid { get; set; }

    /// <summary>
    /// 配送数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_Num1")]
    public decimal Num1 { get; set; }

    /// <summary>
    /// 配送总价.
    /// </summary>
    [SugarColumn(ColumnName = "F_Amount1")]
    public decimal Amount1 { get; set; }

    /// <summary>
    /// 复核数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_Num2")]
    public decimal Num2 { get; set; }

    /// <summary>
    /// 复核总价.
    /// </summary>
    [SugarColumn(ColumnName = "F_Amount2")]
    public decimal Amount2 { get; set; }

    /// <summary>
    /// 复核时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_CheckTime")]
    public DateTime? CheckTime { get; set; }

    /// <summary>
    /// 复核人员.
    /// </summary>
    [SugarColumn(ColumnName = "F_CheckUser")]
    public string CheckUser { get; set; }

    /// <summary>
    /// 订单数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_Num")]
    public decimal Num { get; set; }

    /// <summary>
    /// 订单总价.
    /// </summary>
    [SugarColumn(ColumnName = "F_Amount")]
    public decimal Amount { get; set; }

    ///// <summary>
    ///// 创建时间.
    ///// </summary>
    //[SugarColumn(ColumnName = "F_CreatorTime")]
    //public DateTime? CreatorTime { get; set; }

    ///// <summary>
    ///// 创建用户.
    ///// </summary>
    //[SugarColumn(ColumnName = "F_CreatorUserId")]
    //public string CreatorUserId { get; set; }

    ///// <summary>
    ///// 修改时间.
    ///// </summary>
    //[SugarColumn(ColumnName = "F_LastModifyTime")]
    //public DateTime? LastModifyTime { get; set; }

    ///// <summary>
    ///// 修改用户.
    ///// </summary>
    //[SugarColumn(ColumnName = "F_LastModifyUserId")]
    //public string LastModifyUserId { get; set; }

    /// <summary>
    /// 销售单价.
    /// </summary>
    [SugarColumn(ColumnName = "F_SalePrice")]
    public decimal SalePrice { get; set; }

    /// <summary>
    /// 分拣时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_SorterTime")]
    public DateTime? SorterTime { get; set; }

    /// <summary>
    /// 分拣人员.
    /// </summary>
    [SugarColumn(ColumnName = "F_SorterUserId")]
    public string SorterUserId { get; set; }

    /// <summary>
    /// 分拣备注.
    /// </summary>
    [SugarColumn(ColumnName = "F_SorterDes")]
    public string SorterDes { get; set; }

    /// <summary>
    /// 分拣状态.
    /// </summary>
    [SugarColumn(ColumnName = "F_SorterState")]
    public string SorterState { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "F_Remark")]
    public string Remark { get; set; }

    /// <summary>
    /// 送达状态.
    /// </summary>
    [SugarColumn(ColumnName = "F_ReceiveState")]
    public int ReceiveState { get; set; }

    /// <summary>
    /// 送达时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_ReceiveTime")]
    public DateTime? ReceiveTime { get; set; }
    
    /// <summary>
    /// 退货数量.
    /// </summary>
    [SugarColumn(ColumnName = "RejectNum")]
    public decimal? RejectNum { get; set; }

    /// <summary>
    /// 分拣数量（处理单位不一致的情况，比如下单个，但是分拣按斤，此时fjNum={x}个）.
    /// </summary>
    [SugarColumn(ColumnName = "FjNum")]
    public decimal? FjNum { get; set; }

    /// <summary>
    /// 分拣完成时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_SorterFinishTime")]
    public DateTime? SorterFinishTime { get; set; }


    /// <summary>
    /// 序号.
    /// </summary>
    [SugarColumn(ColumnName = "Order")]
    public int? Order { get; set; }

    
    /// <summary>
    /// 打印别名.
    /// </summary>
    [SugarColumn(ColumnName = "PrintAlais")]
    public string PrintAlais { get; set; }


    /// <summary>
    /// 打单数量.
    /// </summary>
    [SugarColumn(ColumnName = "PrintNum")]
    public decimal? PrintNum { get; set; }

    /// <summary>
    /// 打单单价.
    /// </summary>
    [SugarColumn(ColumnName = "PrintPrice")]
    public decimal? PrintPrice { get; set; }

    /// <summary>
    /// 打单金额.
    /// </summary>
    [SugarColumn(ColumnName = "PrintAmount")]
    public decimal? PrintAmount { get; set; }


    /// <summary>
    /// 质检报告.
    /// </summary>
    [SugarColumn(ColumnName = "QualityReportProof")]
    public string QualityReportProof { get; set; }

    /// <summary>
    /// 采购明细id.
    /// </summary>
    [SugarColumn(ColumnName = "F_Bid")]
    public string Bid { get; set; }

    /// <summary>
    /// 生产日期.
    /// </summary>
    [SugarColumn(ColumnName = "ProductionDate")]
    public DateTime? ProductionDate { get; set; }


    /// <summary>
    /// 保质期.
    /// </summary>
    [SugarColumn(ColumnName = "Retention")]
    public string Retention { get; set; }

    /// <summary>
    /// 打印次数.
    /// </summary>
    [SugarColumn(ColumnName = "PrintCount")]
    public int? PrintCount { get; set; }
}