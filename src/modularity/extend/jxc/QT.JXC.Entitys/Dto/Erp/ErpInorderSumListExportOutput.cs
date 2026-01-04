using System.ComponentModel.DataAnnotations;

namespace QT.JXC.Entitys.Dto.Erp;

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
    /// 单位
    /// </summary>
    [Display(Name = "单位")]
    public string unit { get; set; }


    /// <summary>
    /// 订单数量
    /// </summary>
    [Display(Name = "入库数量")]
    public decimal num { get; set; }

    /// <summary>
    /// 已关联数量
    /// </summary>
    [Display(Name = "已关联")]
    public decimal tsnum { get; set; }

    /// <summary>
    /// 未关联数量
    /// </summary>
    [Display(Name = "未关联")]
    public decimal untsnum { get; set; }
}
