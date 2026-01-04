using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;


namespace QT.Application.Entitys.Dto.FreshDelivery.ErpProductcustomertypeprice;

[SuppressSniffer]
public class ErpProductcustomertypepriceImportDataInput
{
    /// <summary>
    /// 客户类型.
    /// </summary>
    [Display(Name = "客户类型")]
    public string customerType { get; set; }

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

    /// <summary>
    /// 一级分类
    /// </summary>
    [Display(Name = "一级分类")]
    public string rootProducttype { get; set; }
}
