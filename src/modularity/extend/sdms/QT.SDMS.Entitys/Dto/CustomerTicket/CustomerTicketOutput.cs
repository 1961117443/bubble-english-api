
namespace QT.SDMS.Entitys.Dto.CustomerTicket;

public class CustomerTicketOutput: CustomerTicketUpInput
{
    public DateTime? creatorTime { get; set; }

    public string customerName { get; set; }


    public string managerName { get; set; }

    public string no { get; set; }
}