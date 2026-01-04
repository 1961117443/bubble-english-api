
namespace QT.Application.Entitys.Dto.ExtExpenseDetail;

/// <summary>
/// 报销明细输出参数.
/// </summary>
public class ExtExpenseDetailInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 报销说明.
    /// </summary>
    public string title { get; set; }

    /// <summary>
    /// 金额.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 转入id.
    /// </summary>
    public string inid { get; set; }

}