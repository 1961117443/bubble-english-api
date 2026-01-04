using QT.JZRC.Entitys.Dto.JzrcTransactionRule;

namespace QT.JZRC.Interfaces;

/// <summary>
/// 业务抽象：交易规则.
/// </summary>
public interface IJzrcTransactionRuleService
{
    Task<JzrcTransactionRuleInfoOutput> GetDefaultRule();
}