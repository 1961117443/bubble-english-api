using QT.Application.Entitys.Dto.ExtExpenseDetail;
namespace QT.Application.Entitys.Dto.ExtExpenseRecord;

/// <summary>
/// 报销单更新输入.
/// </summary>
public class ExtExpenseRecordUpInput : ExtExpenseRecordCrInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }
    /// <summary>
    /// 报销明细.
    /// </summary>
    public new List<ExtExpenseDetailUpInput> extExpenseDetailList { get; set; }

}