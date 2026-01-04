
namespace QT.JZRC.Entitys.Dto.JzrcMemberAmountLog;

/// <summary>
/// 会员余额记录修改输入参数.
/// </summary>
public class JzrcMemberAmountLogCrInput
{
    /// <summary>
    /// 发生金额.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 发生时间.
    /// </summary>
    public DateTime? addTime { get; set; }

    /// <summary>
    /// 类型.
    /// </summary>
    public string category { get; set; }

}