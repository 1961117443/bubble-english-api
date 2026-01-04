using QT.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpProductcustomerprice;

[SuppressSniffer]
public class ErpProductcustomerpriceImportDataInput
{
    /// <summary>
    /// 客户名称.
    /// </summary>
    [Display(Name = "客户名称")]
    public string customerName { get; set; }

    /// <summary>
    /// 价格.
    /// </summary>
    [Display(Name = "价格")]
    public string price { get; set; }

    /// <summary>
    /// 定价时间.
    /// </summary>
    [Display(Name = "定价时间")]
    public string date { get; set; }

    /// <summary>
    /// 商品名称.
    /// </summary>
    [Display(Name = "商品名称"),Required]
    public string productName { get; set; }

    /// <summary>
    /// 商品规格.
    /// </summary>
    [Display(Name = "商品规格"), Required]
    public string gName { get; set; }
}
