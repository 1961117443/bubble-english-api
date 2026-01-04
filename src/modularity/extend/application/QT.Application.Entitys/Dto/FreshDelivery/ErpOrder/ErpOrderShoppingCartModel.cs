using QT.Application.Entitys.Dto.FreshDelivery.ErpProductmodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOrder;

public class ErpOrderShoppingCartModel
{
    /// <summary>
    /// 客户id
    /// </summary>
    public string cid { get; set; }

    /// <summary>
    /// 规格id
    /// </summary>
    public string gid { get; set; }

    ///// <summary>
    ///// 单价
    ///// </summary>
    //public decimal price { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    public decimal num { get; set; }

    /// <summary>
    /// 详细信息
    /// </summary>
    public ErpProductSelectorOutput info { get; set; }

    /// <summary>
    /// 是否选中
    /// </summary>
    public bool @checked { get; set; }

    /// <summary>
    /// 标识id（客户id+规格id）
    /// </summary>
    public string id => $"{cid}_{gid}";

    /// <summary>
    /// 备注
    /// </summary>
    public string remark { get; set; }
}

public class ErpOrderShoppingCartByCustomerModel
{
    public string cid { get; set; }

    public string cidName { get; set; }

    public List<ErpOrderShoppingCartModel> list { get; set; } = new List<ErpOrderShoppingCartModel>();

    public bool @checked { get; set; }
}
