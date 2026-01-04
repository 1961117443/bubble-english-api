using QT.Common.Security;
namespace QT.SDMS.Entitys.Dto.CustomerTicketReply;

public class CustomerTicketReplyListOutput : CustomerTicketReplyOutput
{
    /// <summary>
    /// 咨询时间
    /// </summary>
    public DateTime? ticketTime { get; set; }


    /// <summary>
    /// 业务员
    /// </summary>
    public string managerIdName { get; set; }

    /// <summary>
    /// 客户名称
    /// </summary>
    public string customerIdName { get; set; }

    public ManagersInfo managerInfo { get; set; }
}


/// <summary>
/// 参加人员信息.
/// </summary>
public class ManagersInfo
{
    /// <summary>
    /// 账号+名字.
    /// </summary>
    public string? account { get; set; }

    /// <summary>
    /// 用户头像.
    /// </summary>
    public string? headIcon { get; set; }

    /// <summary>
    /// 名字第一个字
    /// </summary>
    public string firstName { get; set; }
}
