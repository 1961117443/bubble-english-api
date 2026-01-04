using QT.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.SDMS.Entitys.Dto.Order;

public class OrderCrInput
{
    public string no { get; set; }

    public DateTime? orderDate { get; set; }

    public string userId { get; set; }

    public decimal amount { get; set; }

    public List<FileControlsModel> attachment { get; set; }
    public string remark { get; set; }
}
