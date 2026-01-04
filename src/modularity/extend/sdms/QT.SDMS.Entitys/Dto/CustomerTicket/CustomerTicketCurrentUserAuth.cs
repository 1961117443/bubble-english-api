namespace QT.SDMS.Entitys.Dto.CustomerTicket;

public class CustomerTicketCurrentUserAuth
{
    /// <summary>
    /// 回复权限
    /// </summary>
    public bool replayAuth { get; set; }

    /// <summary>
    /// 关单权限
    /// </summary>
    public bool endAuth { get; set; }

    /// <summary>
    /// 指派权限
    /// </summary>
    public bool assignAuth { get; set; }
}