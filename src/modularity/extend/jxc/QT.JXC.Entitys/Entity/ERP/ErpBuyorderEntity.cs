using QT.Common.Const;
using QT.Extras.DatabaseAccessor.SqlSugar;
using SqlSugar;

namespace QT.JXC.Entitys.Entity.ERP;

/// <summary>
/// 采购任务订单实体.
/// </summary>
[SugarTable("erp_buyorder")]
[Tenant(ClaimConst.TENANTID)]
public class ErpBuyorderEntity:ICompanyEntity
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
    /// 订单编号.
    /// </summary>
    [SugarColumn(ColumnName = "F_No")]
    public string No { get; set; }

    /// <summary>
    /// 任务发起者.
    /// </summary>
    [SugarColumn(ColumnName = "F_TaskFromUser")]
    public string TaskFromUser { get; set; }

    /// <summary>
    /// 指派采购人员.
    /// </summary>
    [SugarColumn(ColumnName = "F_TaskToUserId")]
    public string TaskToUserId { get; set; }

    /// <summary>
    /// 计划采购时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_TaskBuyTime")]
    public DateTime? TaskBuyTime { get; set; }

    /// <summary>
    /// 接受任务时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_AcceptTime")]
    public DateTime? AcceptTime { get; set; }

    /// <summary>
    /// 完毕状态.
    /// </summary>
    [SugarColumn(ColumnName = "F_FinishState")]
    public string FinishState { get; set; }

    /// <summary>
    /// 完毕时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_FinishTime")]
    public DateTime? FinishTime { get; set; }

    /// <summary>
    /// 凭据附件.
    /// </summary>
    [SugarColumn(ColumnName = "F_Proof")]
    public string Proof { get; set; }

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
    /// 任务备注.
    /// </summary>
    [SugarColumn(ColumnName = "F_TaskRemark")]
    public string TaskRemark { get; set; }

    /// <summary>
    /// 采购人员.
    /// </summary>
    [SugarColumn(ColumnName = "F_TaskUserId")]
    public string TaskUserId { get; set; }

    /// <summary>
    /// 采购备注.
    /// </summary>
    [SugarColumn(ColumnName = "F_BuyRemark")]
    public string BuyRemark { get; set; }

    /// <summary>
    /// 订单状态(0:未完成,1：已采购，2：已入库).
    /// </summary>
    [SugarColumn(ColumnName = "F_State")]
    public string State { get; set; }

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
    /// 配送日期.
    /// </summary>
    [SugarColumn(ColumnName = "F_Posttime")]
    public string Posttime { get; set; }

    /// <summary>
    /// 质检报告.
    /// </summary>
    [SugarColumn(ColumnName = "QualityReportProof")]
    public string QualityReportProof { get; set; }

    /// <summary>
    /// 自检报告.
    /// </summary>
    [SugarColumn(ColumnName = "SelfReportProof")]
    public string SelfReportProof { get; set; }

    /// <summary>
    /// 入库单号.
    /// </summary>
    [SugarColumn(ColumnName = "RkNo")]
    public string RkNo { get; set; }
}


/// <summary>
/// 采购任务订单关联采购员.
/// </summary>
[SugarTable("erp_buyorderuser")]
[Tenant(ClaimConst.TENANTID)]
public class ErpBuyorderUserEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public string Id { get; set; }

    /// <summary>
    /// 采购订单ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Bid")]
    public string Bid { get; set; }

    /// <summary>
    /// 采购用户ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Uid")]
    public string Uid { get; set; }
}