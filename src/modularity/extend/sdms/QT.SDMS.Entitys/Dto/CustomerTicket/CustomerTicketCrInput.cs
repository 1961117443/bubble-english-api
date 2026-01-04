using QT.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace QT.SDMS.Entitys.Dto.CustomerTicket;

public class CustomerTicketCrInput
{
    public string customerId { get; set; } 

    public string managerId { get; set; }

    public string title { get; set; }
    public string content { get; set; } 

    public string attachment { get; set; }

    public string imageJson { get; set; }

    public int status { get; set; }
}