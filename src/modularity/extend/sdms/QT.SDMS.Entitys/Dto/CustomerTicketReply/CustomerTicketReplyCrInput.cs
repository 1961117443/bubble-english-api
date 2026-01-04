using QT.Common.Models;
using QT.SDMS.Entitys.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace QT.SDMS.Entitys.Dto.CustomerTicketReply;

public class CustomerTicketReplyCrInput
{
    public string ticketId { get; set; } 

    public string replyById { get; set; }

    public string replyBy { get; set; }
    public ReplyRole replyRole { get; set; }

    public string content { get; set; }

    public string attachment { get; set; }

    public string imageJson { get; set; }
}