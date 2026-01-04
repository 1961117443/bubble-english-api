namespace QT.JZRC.Entitys.Dto.JzrcTransactionRule;

/// <summary>
/// 交易规则输入参数.
/// </summary>
public class JzrcTransactionRuleListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

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

}