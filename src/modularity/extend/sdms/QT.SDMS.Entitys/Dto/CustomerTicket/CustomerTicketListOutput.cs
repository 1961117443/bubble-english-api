using QT.Common.Security;
namespace QT.SDMS.Entitys.Dto.CustomerTicket;

public class CustomerTicketListOutput : CustomerTicketOutput
{
    //public bool hasChildren {  get; set; }
    //public List<object>? children { get; set; }
    //public int num { get; set; }
    //public bool isLeaf { get; set; }

    /// <summary>
    /// 问题编号
    /// </summary>
    public string no { get; set; }


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


    ///// <summary>
    ///// 上级业务员 业务主管
    ///// </summary>
    //public string? parentUserName { get; set; }

    ///// <summary>
    ///// 上上级业务员 部门经理
    ///// </summary>
    //public string? grandfatherUserName { get; set; }


}
