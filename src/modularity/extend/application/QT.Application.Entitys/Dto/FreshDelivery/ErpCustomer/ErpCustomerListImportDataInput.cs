using QT.Common.Models;
using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpCustomer;

[SuppressSniffer]
public class ErpCustomerListImportDataInput
{
    /// <summary>
    /// 客户名称.
    /// </summary>
    [Display(Name = "客户名称")]
    public string name { get; set; }

    /// <summary>
    /// 客户编号.
    /// </summary>
    [Display(Name = "客户编号")]
    public string no { get; set; }

    /// <summary>
    /// 客户地址.
    /// </summary>
    [Display(Name = "客户地址")]
    public string address { get; set; }

    /// <summary>
    /// 负责人.
    /// </summary>
    [Display(Name = "负责人")]
    public string admin { get; set; }

    /// <summary>
    /// 负责人电话.
    /// </summary>
    [Display(Name = "负责人电话")]
    public string admintel { get; set; }

    /// <summary>
    /// 客户简介.
    /// </summary>
    [Display(Name = "客户简介")]
    public string description { get; set; }

    /// <summary>
    /// 客户类型.
    /// </summary>
    [Display(Name = "客户类型")]
    public string type { get; set; }

    /// <summary>
    /// 餐别.
    /// </summary>
    [Display(Name = "餐别")]
    public string diningType { get; set; }

    /// <summary>
    /// 客户前缀.
    /// </summary>
    [Display(Name = "客户前缀")]
    public string prefix { get; set; }
}
