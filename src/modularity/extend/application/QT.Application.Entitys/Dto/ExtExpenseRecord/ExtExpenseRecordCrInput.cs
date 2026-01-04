using QT.Common.Models;
using QT.Application.Entitys.Dto.ExtExpenseDetail;
using Newtonsoft.Json;

namespace QT.Application.Entitys.Dto.ExtExpenseRecord;

/// <summary>
/// 报销单修改输入参数.
/// </summary>
public class ExtExpenseRecordCrInput
{
    /// <summary>
    /// 报销日期.
    /// </summary>
    public DateTime? billDate { get; set; }

    /// <summary>
    /// 金额.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 标签.
    /// </summary>
    public object label { get; set; }

    /// <summary>
    /// 报销说明.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 图片.
    /// </summary>
    public List<FileControlsModel> imageJson { get; set; }

    /// <summary>
    /// 流程引擎ID.
    /// </summary>
    public string flowId { get; set; }

    /// <summary>
    /// 报销明细.
    /// </summary>
    public List<ExtExpenseDetailCrInput> extExpenseDetailList { get; set; }

    /// <summary>
    /// 流程状态.
    /// </summary>
    public int flowState { get; set; }

    /// <summary>
    /// 候选人.
    /// </summary>
    public Dictionary<string, List<string>> candidateList { get; set; }
}