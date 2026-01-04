using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpBuyorder;

public class ErpBuyorderRecommendDto
{
    public string productId { get; set; }
    public string mid { get; set; }

    /// <summary>
    /// 需求量，默认为订单量
    /// </summary>
    public decimal needNum { get; set; }

    public decimal orderNum { get; set; }

    /// <summary>
    /// 基本单位的需求数量
    /// </summary>
    public decimal baseNeedNum { get; set; }
    /// <summary>
    /// 基本单位的库存数量
    /// </summary>
    public decimal baseStoreNum { get; set; }
    public decimal storeNum { get; set; }
    public decimal srcStoreNum { get; set; }
    //public decimal planNum { get; set; }
    public string remark { get; set; }

    /// <summary>
    /// 主单位数量比
    /// </summary>
    public decimal ratio { get; set; }

    /// <summary>
    /// 是否使用基本单位（其他单位有库存的话，统一转成基本单位换算）
    /// </summary>
    public bool useBase { get; set; }
}
