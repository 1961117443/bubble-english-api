using QT.JXC.Entitys.Dto.Erp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JXC.Entitys.Dto.ErpOrderCwdd;

public class ErpOrderCwddUpInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 订单商品表.
    /// </summary>
    public List<ErpOrderdetailCwddUpInput> erpOrderdetailList { get; set; }
}

public class ErpOrderdetailCwddUpInput
{
    public string id { get; set; }

    public decimal printNum { get; set; }

    public decimal printPrice { get; set; }

    public decimal printAmount { get; set; }
}

public class ErpOrderdetailCwddInfoOutput : ErpOrderdetailInfoOutput
{
    public decimal? printNum { get; set; }

    public decimal? printPrice { get; set; }

    public decimal? printAmount { get; set; }
}

public class ErpOrderCwddListOutput: ErpOrderListOutput
{

    public decimal? printAmount { get; set; }
}