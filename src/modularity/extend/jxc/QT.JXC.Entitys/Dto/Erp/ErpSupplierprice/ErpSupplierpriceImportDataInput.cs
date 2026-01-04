using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace QT.JXC.Entitys.Dto.Erp.ErpSupplierprice;

[SuppressSniffer]
public class ErpSupplierpriceImportDataInput
{
    /// <summary>
    /// 供应商名称.
    /// </summary>
    [Display(Name = "供应商名称")]
    public string supplier { get; set; }

    /// <summary>
    /// 计价类型.
    /// </summary>
    [Display(Name = "计价类型")]
    public string pricingType { get; set; }

    /// <summary>
    /// 价格.
    /// </summary>
    [Display(Name = "价格")]
    public string price { get; set; }

    /// <summary>
    /// 折扣.
    /// </summary>
    [Display(Name = "折扣")]
    public string discount { get; set; }
    
    ///// <summary>
    ///// 定价时间.
    ///// </summary>
    //[Display(Name = "定价时间")]
    //public string date { get; set; }

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
