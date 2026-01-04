using QT.Common.Filter;

namespace QT.JZRC.Entitys.Dto.JzrcTransactionRule;

/// <summary>
/// 交易规则列表查询输入
/// </summary>
public class JzrcTransactionRuleListQueryInput : PageInputBase
{
    /// <summary>
    /// 选择导出数据key.
    /// </summary>
    public string selectKey { get; set; }

    /// <summary>
    /// 导出类型.
    /// </summary>
    public int dataType { get; set; }

    /// <summary>
    /// 规则名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 规则标识.
    /// </summary>
    public string code { get; set; }

}