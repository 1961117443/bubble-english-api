using QT.Common.Models;
using QT.Application.Entitys.Dto.ExtExpenseDetail;
using Newtonsoft.Json;

namespace QT.Application.Entitys.Dto.ExtExpenseRecord;

/// <summary>
/// 报销单输出参数.
/// </summary>
public class ExtExpenseRecordInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

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
    public List<ExtExpenseDetailInfoOutput> extExpenseDetailList { get; set; }

    /// <summary>
    /// 报销单号.
    /// </summary>
    public string billNo { get; set; }

}