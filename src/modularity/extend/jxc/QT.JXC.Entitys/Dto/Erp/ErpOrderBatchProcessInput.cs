using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 批量提交订单入参
/// </summary>
public class ErpOrderBatchProcessInput
{
    /// <summary>
    /// 订单id集合.
    /// </summary>
    public List<string> items { get; set; }
}
