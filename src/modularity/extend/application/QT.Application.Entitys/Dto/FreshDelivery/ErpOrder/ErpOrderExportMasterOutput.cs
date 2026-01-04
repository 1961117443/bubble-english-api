using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOrder;

/// <summary>
/// 订单导出，主表内容
/// </summary>
public class ErpOrderExportMasterOutput
{
    public string id { get; set; }  
    public string oidName { get; set; }

    public string orderNo { get; set; }

    public string orderDate { get; set; }
    public string cidName { get; set; }
    public string cidAdmin { get; set; }
    public string cidAdminTel { get; set; }
    public string cidAddress { get; set; }
    public string totalCount { get; set; }
    public string totalAmount { get; set; }
    public DateTime? CreateTime { get; set; }
    public string CreateUidName { get; set; }
    public string DiningType { get; set; }
    public DateTime? Posttime { get; set; }

    public string deliveryManIdName { get; set; }
}

public class ErpOrderExportDetailOutput
{
    public string fid { get; set; }  
    public string productCode { get; set; }
    public string productName { get; set; }
    public string productSpec { get; set; }
    public string productUnit { get; set; }
    public string productType { get; set; }
    public string price { get; set; }
    public string num { get; set; }
    public string amount { get; set; }
    public string remark { get; set; }
    public string num1 { get; set; }
}
