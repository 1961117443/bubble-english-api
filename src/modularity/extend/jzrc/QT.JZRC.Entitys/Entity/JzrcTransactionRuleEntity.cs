using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.JZRC.Entitys;

/// <summary>
/// 交易规则实体.
/// </summary>
[SugarTable("jzrc_transaction_rule")]
[Tenant(ClaimConst.TENANTID)]
public class JzrcTransactionRuleEntity: CUDEntityBase
{
    /// <summary>
    /// 规则名称.
    /// </summary>
    [SugarColumn(ColumnName = "Name")]
    public string Name { get; set; }

    /// <summary>
    /// 规则标识.
    /// </summary>
    [SugarColumn(ColumnName = "Code")]
    public string Code { get; set; }

    /// <summary>
    /// 规则说明.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }

    /// <summary>
    /// 收取人才佣金(%).
    /// </summary>
    [SugarColumn(ColumnName = "TalentCommission")]
    public decimal TalentCommission { get; set; }

    /// <summary>
    /// 收取企业佣金(%).
    /// </summary>
    [SugarColumn(ColumnName = "EnterprisesCommission")]
    public decimal EnterprisesCommission { get; set; }

    /// <summary>
    /// 竞投规则|超越限时(x分钟).
    /// </summary>
    [SugarColumn(ColumnName = "BiddingTimeLimit")]
    public int? BiddingTimeLimit { get; set; }

    /// <summary>
    /// 竞投规则|保证金比例(%).
    /// </summary>
    [SugarColumn(ColumnName = "BiddingDepositRatio")]
    public decimal BiddingDepositRatio { get; set; }

    /// <summary>
    /// 竞投规则|竞投最低加价金额.
    /// </summary>
    [SugarColumn(ColumnName = "BiddingMinimumIncrement")]
    public decimal BiddingMinimumIncrement { get; set; }

    /// <summary>
    /// 竞投规则|违约平台收取保证金比例(%).
    /// </summary>
    [SugarColumn(ColumnName = "BiddingPlatformDepositRatio")]
    public decimal BiddingPlatformDepositRatio { get; set; }

    /// <summary>
    /// 竞投规则|守约方收取违约金额比例(%).
    /// </summary>
    [SugarColumn(ColumnName = "BiddingNonDefaultingDepositRatio")]
    public decimal BiddingNonDefaultingDepositRatio { get; set; }

    /// <summary>
    /// 竞聘规则|超越限时(x分钟).
    /// </summary>
    [SugarColumn(ColumnName = "TenderingTimeLimit")]
    public int? TenderingTimeLimit { get; set; }

    /// <summary>
    /// 竞聘规则|保证金比例(%).
    /// </summary>
    [SugarColumn(ColumnName = "TenderingDepositRatio")]
    public decimal TenderingDepositRatio { get; set; }

    /// <summary>
    /// 竞聘规则|竞投最低加价金额.
    /// </summary>
    [SugarColumn(ColumnName = "TenderingMinimumIncrement")]
    public decimal TenderingMinimumIncrement { get; set; }

    /// <summary>
    /// 竞聘规则|违约平台收取保证金比例(%).
    /// </summary>
    [SugarColumn(ColumnName = "TenderingPlatformDepositRatio")]
    public decimal TenderingPlatformDepositRatio { get; set; }

    /// <summary>
    /// 竞聘规则|守约方收取违约金额比例(%).
    /// </summary>
    [SugarColumn(ColumnName = "TenderingNonDefaultingDepositRatio")]
    public decimal TenderingNonDefaultingDepositRatio { get; set; }

}