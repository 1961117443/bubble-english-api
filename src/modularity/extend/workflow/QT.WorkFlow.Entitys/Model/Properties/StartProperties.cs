using QT.DependencyInjection;
using QT.WorkFlow.Entitys.Model.Conifg;

namespace QT.WorkFlow.Entitys.Model.Properties;

[SuppressSniffer]
public class StartProperties
{
    /// <summary>
    /// 发起节点标题.
    /// </summary>
    public string? title { get; set; }

    /// <summary>
    /// 指定发起人（为空则是所有人）.
    /// </summary>
    public List<string>? initiator { get; set; } = new List<string>();

    /// <summary>
    /// 指定发起岗位（为空则是所有人）.
    /// </summary>
    public List<string>? initiatePos { get; set; } = new List<string>();

    /// <summary>
    /// 指定发起角色.
    /// </summary>
    public List<string>? initiateRole { get; set; } = new List<string>();

    /// <summary>
    /// 表单权限.
    /// </summary>
    public List<FormOperatesModel>? formOperates { get; set; }

    /// <summary>
    /// 打印id.
    /// </summary>
    public string? printId { get; set; }

    /// <summary>
    /// 是否评论.
    /// </summary>
    public bool isComment { get; set; }

    /// <summary>
    /// 是否批量.
    /// </summary>
    public bool isBatchApproval { get; set; }

    #region 按钮

    /// <summary>
    /// 撤回按钮.
    /// </summary>
    public string? revokeBtnText { get; set; } = "撤 回";

    /// <summary>
    /// 是否撤回.
    /// </summary>
    public bool hasRevokeBtn { get; set; } = true;

    /// <summary>
    /// 提交按钮.
    /// </summary>
    public string? submitBtnText { get; set; } = "提交审核";

    /// <summary>
    /// 是否提交.
    /// </summary>
    public bool hasSubmitBtn { get; set; } = true;

    /// <summary>
    /// 保存按钮.
    /// </summary>
    public string? saveBtnText { get; set; } = "保存草稿";

    /// <summary>
    /// 是否保存.
    /// </summary>
    public bool hasSaveBtn { get; set; } = true;

    /// <summary>
    /// 催办按钮.
    /// </summary>
    public string? pressBtnText { get; set; } = "催 办";

    /// <summary>
    /// 是否催办.
    /// </summary>
    public bool hasPressBtn { get; set; } = true;

    /// <summary>
    /// 打印按钮.
    /// </summary>
    public string? printBtnText { get; set; } = "打 印";

    /// <summary>
    /// 是否打印.
    /// </summary>
    public bool hasPrintBtn { get; set; } = true;
    #endregion

    #region 节点事件

    /// <summary>
    /// 流程发起事件.
    /// </summary>
    public FuncConfig? initFuncConfig { get; set; }

    /// <summary>
    /// 流程结束事件.
    /// </summary>
    public FuncConfig? endFuncConfig { get; set; }

    /// <summary>
    /// 流程撤回事件.
    /// </summary>
    public FuncConfig? flowRecallFuncConfig { get; set; }
    #endregion

    #region 消息

    /// <summary>
    /// 审核.
    /// </summary>
    public MsgConfig? waitMsgConfig { get; set; }

    /// <summary>
    /// 结束.
    /// </summary>
    public MsgConfig? endMsgConfig { get; set; }

    /// <summary>
    /// 同意.
    /// </summary>
    public MsgConfig? approveMsgConfig { get; set; }

    /// <summary>
    /// 拒绝.
    /// </summary>
    public MsgConfig? rejectMsgConfig { get; set; }

    /// <summary>
    /// 抄送.
    /// </summary>
    public MsgConfig? copyMsgConfig { get; set; }
    #endregion
}
