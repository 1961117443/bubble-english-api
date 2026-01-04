using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JXC.Entitys.Dto.Erp;

public class ErpProductCustomerPriceOutput
{
    public string id { get; set; }
    public DateTime? time { get; set; }
    public string cid { get; set; }
    public string gid { get; set; }
    public string cidName { get; set; }
    public decimal price { get; set; }
}
