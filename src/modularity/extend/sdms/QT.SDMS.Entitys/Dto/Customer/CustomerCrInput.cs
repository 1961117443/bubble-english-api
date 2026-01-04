using QT.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace QT.SDMS.Entitys.Dto.Customer;

public class CustomerCrInput
{
    public string name { get; set; } 

    public string contactName { get; set; }

    public string contactPhone { get; set; }
    public string email { get; set; }
    public string address { get; set; }
    public string managerId { get; set; }
    public string meterPoint { get; set; }

    public string attachment { get; set; }

    public string remark { get; set; }

    public int status { get; set; }
    public int enabledMark { get; set; }
    public string idCardImgJson { get; set; }
    public string householdImgJson { get; set; }
}
