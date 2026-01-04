namespace QT.Application.Entitys.Dto.ExtExpenseRecord;

/// <summary>
/// 报销单输入参数.
/// </summary>
public class ExtExpenseRecordListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 流程状态.
    /// </summary>
    public int flowState { get; set; }

    /// <summary>
    /// 流程引擎ID.
    /// </summary>
    public string flowId { get; set; }

    /// <summary>
    /// 报销单号
    /// </summary>
    public string billNo { get; set; }

    /// <summary>
    /// 报销日期
    /// </summary>
    public DateTime? billDate { get; set; }

    /// <summary>
    /// 报销金额
    /// </summary>
    public decimal? amount { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? remark { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    public string? label { get; set; }
}