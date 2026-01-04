using QT.Common.Security;

namespace QT.Iot.Application.Dto.CrmUserDelayApply;

public class CrmUserDelayApplyListOutput : CrmUserDelayApplyOutput
{

    /// <summary>
    /// 姓名
    /// </summary>
    public string userIdName { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    public string userIdAccount { get; set; }

    public DateTime? creatorTime { get; set; }
}
