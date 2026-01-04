
namespace QT.JZRC.Entitys.Dto.JzrcTransactionRule;

/// <summary>
/// 交易规则修改输入参数.
/// </summary>
public class JzrcTransactionRuleCrInput
{
    /// <summary>
    /// 规则名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 规则标识.
    /// </summary>
    public string code { get; set; }

    /// <summary>
    /// 规则说明.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 收取人才佣金(%).
    /// </summary>
    public decimal talentCommission { get; set; }

    /// <summary>
    /// 收取企业佣金(%).
    /// </summary>
    public decimal enterprisesCommission { get; set; }

    /// <summary>
    /// 竞投规则|超越限时(x分钟).
    /// </summary>
    public int? biddingTimeLimit { get; set; }

    /// <summary>
    /// 竞投规则|保证金比例(%).
    /// </summary>
    public decimal biddingDepositRatio { get; set; }

    /// <summary>
    /// 竞投规则|竞投最低加价金额.
    /// </summary>
    public decimal biddingMinimumIncrement { get; set; }

    /// <summary>
    /// 竞投规则|违约平台收取保证金比例(%).
    /// </summary>
    public decimal biddingPlatformDepositRatio { get; set; }

    /// <summary>
    /// 竞投规则|守约方收取违约金额比例(%).
    /// </summary>
    public decimal biddingNonDefaultingDepositRatio { get; set; }

    /// <summary>
    /// 竞聘规则|超越限时(x分钟).
    /// </summary>
    public int? tenderingTimeLimit { get; set; }

    /// <summary>
    /// 竞聘规则|保证金比例(%).
    /// </summary>
    public decimal tenderingDepositRatio { get; set; }

    /// <summary>
    /// 竞聘规则|竞投最低加价金额.
    /// </summary>
    public decimal tenderingMinimumIncrement { get; set; }

    /// <summary>
    /// 竞聘规则|违约平台收取保证金比例(%).
    /// </summary>
    public decimal tenderingPlatformDepositRatio { get; set; }

    /// <summary>
    /// 竞聘规则|守约方收取违约金额比例(%).
    /// </summary>
    public decimal tenderingNonDefaultingDepositRatio { get; set; }

}