using QT.Common.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOrderFj;

/// <summary>
/// 订单分拣，提交检查输出对象
/// </summary>
public class ErpOrderFjWaitListOutput
{
    /// <summary>
    /// 订单明细id
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 商品名称
    /// </summary>
    public string productName { get; set; }
    
    /// <summary>
    /// 规格名称
    /// </summary>
    public string midName { get; set; }
    
    /// <summary>
    /// 规格单位
    /// </summary>
    public string unit { get; set; }
    
    /// <summary>
    /// 客户单位
    /// </summary>
    public string customerUnit { get; set; }

    /// <summary>
    /// 订单数量
    /// </summary>
    public decimal num { get; set; }
    
    /// <summary>
    /// 实际分拣（当前实际分拣）
    /// </summary>
    public decimal num1 { get; set; }
    
    /// <summary>
    /// 分拣数量（按客户单位）
    /// </summary>
    public decimal fjNum { get; set; }

    /// <summary>
    /// 分拣状态
    /// </summary>
    public string sorterState { get; set; }

    /// <summary>
    /// 子单实际分拣（当前实际分拣）
    /// </summary>
    public decimal cnum1 { get; set; }

    /// <summary>
    /// 子单分拣数量（按客户单位）
    /// </summary>
    public decimal cfjNum { get; set; }

    /// <summary>
    /// 待分拣数量（会根据单位判断）
    /// </summary>
    public decimal wnum
    {
        get
        {
            if (this.sorterFinishTime.HasValue)
            {
                return 0;
            }

            if (string.IsNullOrEmpty(customerUnit) || customerUnit == unit)
            {
                return num - num1 - cnum1;
            }

            if (customerUnit != unit)
            {
                return num - fjNum - cfjNum;
            }

            return 0;
        }
    }

    /// <summary>
    /// 已分拣数量（会根据单位判断）
    /// </summary>
    public decimal ynum
    {
        get
        {
            if (string.IsNullOrEmpty(customerUnit) || customerUnit == unit)
            {
                return cnum1;
            }

            if (customerUnit != unit)
            {
                return cfjNum;
            }

            return 0;
        }
    }

    /// <summary>
    /// 订单单位（有客户单位取客户单位，否则取规则单位）
    /// </summary>
    public string ounit
    {
        get
        {
            return customerUnit ?? unit;
        }
    }

    /// <summary>
    /// 分拣完成时间.
    /// </summary>
    public DateTime? sorterFinishTime { get; set; }
}

public class ErpOrderFjSubmitCheckOutput
{
    public List<ErpOrderFjWaitListOutput> list { get; set; }

    public bool done { get; set; }
}