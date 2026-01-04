using System.ComponentModel.DataAnnotations;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpInrecord;

public class ErpInorderSumListExportOutput
{
    /// <summary>
    /// 规格id
    /// </summary>
    public string mid { get; set; }

    /// <summary>
    /// 商品id
    /// </summary>
    public string pid { get; set; }

    /// <summary>
    /// 商品名称
    /// </summary>
    [Display(Name = "商品名称")]
    public string productName { get; set; }

    /// <summary>
    /// 规格名称
    /// </summary>
    [Display(Name = "规格名称")]
    public string name { get; set; }


    /// <summary>
    /// 订单数量
    /// </summary>
    [Display(Name = "入库数量")]
    public decimal num { get; set; }
}
