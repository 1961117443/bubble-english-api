using System.ComponentModel;
using QT.Common.Filter;
using QT.DependencyInjection;

namespace QT.WorkFlow.Entitys.Enum;

/// <summary>
/// 任务状态枚举.
/// </summary>
//[SuppressSniffer]
public enum FlowTaskStatusEnum
{
    /// <summary>
    /// 等待提交.
    /// </summary>
    [Description("等待提交")]
    [TagStyle("warning")]
    Draft = 0,

    /// <summary>
    /// 等待审核.
    /// </summary>
    [Description("等待审核")]
    [TagStyle("default")]
    Handle = 1,

    /// <summary>
    /// 审核通过. 
    /// </summary>
    [Description("审核通过")]
    [TagStyle("success")]
    Adopt = 2,

    /// <summary>
    /// 审核驳回.
    /// </summary>
    [Description("审核驳回")]
    [TagStyle("danger")]
    Reject = 3,

    /// <summary>
    /// 流程撤回.
    /// </summary>
    [Description("流程撤回")]
    [TagStyle("info")]
    Revoke = 4,

    /// <summary>
    /// 审核终止.
    /// </summary>
    [Description("审核终止")]
    [TagStyle("info")]
    Cancel = 5,
}