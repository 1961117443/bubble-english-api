using QT.Common.Contracts;
using SqlSugar;

namespace QT.SDMS.Entitys.Entity;

/// <summary>
/// 营销管理-财务记录
/// </summary>
[SugarTable("crm_finance_record")]
public class FinanceRecordEntity : CUDEntityBase
{
    /// <summary>
    /// 项目id
    /// </summary>
    [SugarColumn(ColumnName = "Pid")]
    public string Pid { get; set; }


    /// <summary>
    /// 发生时间
    /// </summary>
    [SugarColumn(ColumnName = "FinanceTime")]
    public DateTime? FinanceTime { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [SugarColumn(ColumnName = "Content")]
    public string Content { get; set; }


    /// <summary>
    /// 附件
    /// </summary>
    [SugarColumn(ColumnName = "Attachment")]
    public string Attachment { get; set; }

    /// <summary>
    /// 金额
    /// </summary>
    [SugarColumn(ColumnName = "Amount")]
    public decimal Amount { get; set; }
}
