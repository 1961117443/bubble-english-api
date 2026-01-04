using QT.Common.Models;
using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace QT.JXC.Entitys.Dto.Erp;

[SuppressSniffer]
public class ErpSupplierListImportDataInput
{
    /// <summary>
    /// 名称.
    /// </summary>
    [Display(Name = "名称")]
    public string name { get; set; }



    /// <summary>
    /// 地址.
    /// </summary>
    [Display(Name = "地址")]
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
    /// 入驻时间.
    /// </summary>
    [Display(Name = "入驻时间")]
    public string joinTime { get; set; }

    /// <summary>
    /// 经营周期.
    /// </summary>
    [Display(Name = "经营周期")]
    public string workCycle { get; set; }

    /// <summary>
    /// 经营时间.
    /// </summary>
    [Display(Name = "经营时间")]
    public string workTime { get; set; }

    /// <summary>
    /// 业务人员.
    [Display(Name = "业务人员")] /// </summary>
    public string salesman { get; set; }





    ///// <summary>
    ///// 营业执照
    ///// </summary>
    //[Display(Name = "营业执照")] 
    //public string businessLicense { get; set; }

    ///// <summary>
    ///// 生产许可
    ///// </summary>
    //[Display(Name = "生产许可")] 
    //public string productionLicense { get; set; }

    ///// <summary>
    ///// 食品经营许可证
    ///// </summary>
    //[Display(Name = "食品经营许可证")] 
    //public string foodBusinessLicense { get; set; }
}
