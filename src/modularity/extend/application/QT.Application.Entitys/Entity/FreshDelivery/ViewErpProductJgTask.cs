using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Application.Entitys.FreshDelivery;

[SqlSugar.SugarTable("v_erp_product_jgtask")]
public class ViewErpProductJgTask
{
    public string GidName { get; set; }
    public string ProductName { get; set; }
    public string SupplierName { get; set; }
    public string Id { get; set; }


    public string Gid { get; set; }

    public decimal PlanNum { get; set; }
    public string Supplier { get; set; }
    public string ItRemark { get; set; }


    public decimal JgNum { get; set; }

    public decimal WaitJgNum { get; set; }
    public string Unit { get; set; }
    public string Oid { get; set; }
    public DateTime CreatorTime { get; set; }
    public decimal SalePrice { get; set; }
    public string Rid { get; set; }
}
