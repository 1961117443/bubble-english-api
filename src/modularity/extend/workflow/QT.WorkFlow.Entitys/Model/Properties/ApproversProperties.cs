using QT.DependencyInjection;
using QT.WorkFlow.Entitys.Model.Conifg;

namespace QT.WorkFlow.Entitys.Model.Properties;

[SuppressSniffer]
public class ApproversProperties
{
    /// <summary>
    /// 标题.
    /// </summary>
    public string? title { get; set; }

    /// <summary>
    /// 审批类型（类型参考FlowTaskOperatorEnum类）.
    /// </summary>
    public int assigneeType { get; set; }

    /// <summary>
    /// 进度.
    /// </summary>
    public string? progress { get; set; }

    /// <summary>
    /// 驳回节点.
    /// </summary>
    public string? rejectStep { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    public string? description { get; set; }

    /// <summary>
    /// 自定义抄送人.
    /// </summary>
    public bool isCustomCopy { get; set; }

    /// <summary>
    /// 发起人主管级别.
    /// </summary>
    public int managerLevel { get; set; } = 1;

    /// <summary>
    /// 会签比例.
    /// </summary>
    public int? countersignRatio { get; set; } = 100;

    /// <summary>
    /// 审批类型（0：或签 1：会签） .
    /// </summary>
    public int? counterSign { get; set; } = 0;

    /// <summary>
    /// 表单字段.
    /// </summary>
    public string? formField { get; set; }

    /// <summary>
    /// 指定复审审批节点.
    /// </summary>
    public string? nodeId { get; set; }

    /// <summary>
    /// 服务 请求路径.
    /// </summary>
    public string? getUserUrl { get; set; }

    /// <summary>
    /// 是否有签名.
    /// </summary>
    public bool hasSign { get; set; }

    /// <summary>
    /// 是否可以加签.
    /// </summary>
    public bool hasFreeApprover { get; set; }

    /// <summary>
    /// 打印id.
    /// </summary>
    public string? printId { get; set; }

    /// <summary>
    /// 表单字段审核方式的类型(1-用户 2-部门).
    /// </summary>
    public int formFieldType { get; set; }

    /// <summary>
    /// 超时设置.
    /// </summary>
    public TimeOutConfig? timeoutConfig { get; set; }

    /// <summary>
    /// 表单权限数据.
    /// </summary>
    public List<FormOperatesModel>? formOperates { get; set; }

    /// <summary>
    /// 定时器到时时间.
    /// </summary>
    public List<TimerProperties> timerList { get; set; } = new List<TimerProperties>();

    #region 人员

    /// <summary>
    /// 指定审批人.
    /// </summary>
    public List<string> approvers { get; set; } = new List<string>();

    /// <summary>
    /// 指定审批岗位.
    /// </summary>
    public List<string> approverPos { get; set; } = new List<string>();

    /// <summary>
    /// 指定抄送岗位.
    /// </summary>
    public List<string> circulatePosition { get; set; } = new List<string>();

    /// <summary>
    /// 指定抄送人.
    /// </summary>
    public List<string> circulateUser { get; set; } = new List<string>();

    /// <summary>
    /// 指定审批角色.
    /// </summary>
    public List<string> approverRole { get; set; } = new List<string>();

    /// <summary>
    /// 抄送角色.
    /// </summary>
    public List<string> circulateRole { get; set; } = new List<string>();
    #endregion

    #region 消息

    /// <summary>
    /// 审核通过.
    /// </summary>
    public MsgConfig? approveMsgConfig { get; set; }

    /// <summary>
    /// 审核驳回.
    /// </summary>
    public MsgConfig? rejectMsgConfig { get; set; }

    /// <summary>
    /// 审核催办.
    /// </summary>
    public MsgConfig? copyMsgConfig { get; set; }
    #endregion

    #region 节点事件

    /// <summary>
    /// 审核通过事件.
    /// </summary>
    public FuncConfig? approveFuncConfig { get; set; }

    /// <summary>
    /// 审核驳回事件.
    /// </summary>
    public FuncConfig? rejectFuncConfig { get; set; }

    /// <summary>
    /// 审核撤回事件.
    /// </summary>
    public FuncConfig? recallFuncConfig { get; set; }

    #endregion

    #region 按钮

    /// <summary>
    /// 是否保存.
    /// </summary>
    public bool hasSaveBtn { get; set; }

    /// <summary>
    /// 保存按钮.
    /// </summary>
    public string? saveBtnText { get; set; } = "保存草稿";

    /// <summary>
    /// 是否打印.
    /// </summary>
    public bool hasPrintBtn { get; set; } = false;

    /// <summary>
    /// 打印.
    /// </summary>
    public string? printBtnText { get; set; } = "打 印";

    /// <summary>
    /// 是否通过.
    /// </summary>
    public bool hasAuditBtn { get; set; } = true;

    /// <summary>
    /// 通过按钮.
    /// </summary>
    public string? auditBtnText { get; set; } = "通 过";

    /// <summary>
    /// 是否拒绝.
    /// </summary>
    public bool hasRejectBtn { get; set; } = true;

    /// <summary>
    /// 拒绝按钮.
    /// </summary>
    public string? rejectBtnText { get; set; } = "拒 绝";

    /// <summary>
    /// 是否撤回.
    /// </summary>
    public bool hasRevokeBtn { get; set; } = true;

    /// <summary>
    /// 撤回按钮.
    /// </summary>
    public string? revokeBtnText { get; set; } = "撤 回";

    /// <summary>
    /// 是否转办.
    /// </summary>
    public bool hasTransferBtn { get; set; } = true;

    /// <summary>
    /// 转办按钮.
    /// </summary>
    public string? transferBtnText { get; set; } = "转 办";
    #endregion
}
