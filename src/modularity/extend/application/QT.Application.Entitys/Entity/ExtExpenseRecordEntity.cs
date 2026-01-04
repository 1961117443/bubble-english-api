using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Application.Entitys;

/// <summary>
/// 报销单实体.
/// </summary>
[SugarTable("ext_expense_record")]
[Tenant(ClaimConst.TENANTID)]
public class ExtExpenseRecordEntity : CLDEntityBase, IFlowEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 报销日期.
    /// </summary>
    [SugarColumn(ColumnName = "F_BillDate")]
    public DateTime? BillDate { get; set; }

    /// <summary>
    /// 金额.
    /// </summary>
    [SugarColumn(ColumnName = "F_Amount")]
    public decimal Amount { get; set; }


    /// <summary>
    /// 报销单号.
    /// </summary>
    [SugarColumn(ColumnName = "F_BillNo")]
    public string BillNo { get; set; }

    /// <summary>
    /// 标签.
    /// </summary>
    [SugarColumn(ColumnName = "F_Label")]
    public string Label { get; set; }

    /// <summary>
    /// 报销说明.
    /// </summary>
    [SugarColumn(ColumnName = "F_Remark")]
    public string Remark { get; set; }

    /// <summary>
    /// 图片.
    /// </summary>
    [SugarColumn(ColumnName = "F_ImageJson")]
    public string ImageJson { get; set; }

    /// <summary>
    /// 流程主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_FlowId")]
    public string FlowId { get; set; }

    /// <summary>
    /// 流程标题.
    /// </summary>
    [SugarColumn(ColumnName = "F_FlowTitle")]
    public string FlowTitle { get; set; }

    /// <summary>
    /// 流程等级.
    /// </summary>
    [SugarColumn(ColumnName = "F_FlowUrgent")]
    public int? FlowUrgent { get; set; }

}