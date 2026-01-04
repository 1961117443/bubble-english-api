using QT.Application.Entitys.Dto.FreshDelivery.Base;
using QT.DependencyInjection;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpBuyorder;

/// <summary>
/// 采购订单（采购员）导入参数
/// </summary>
public class ErpBuyorderCgImportData :BaseImportDataInput
{
    [Description("计划采购时间")]
    [Required(ErrorMessage = "计划采购时间必填")]
    public DateTime? taskBuyTime { get; set; }

    [Description("供应商名称")]
    [Required(ErrorMessage = "供应商名称必填")]
    public string supplierName { get; set; }

    [Description("商品名称")]
    [Required(ErrorMessage = "商品名称必填")]
    public string productName { get; set; }

    [Description("规格")]
    [Required(ErrorMessage = "规格必填")]
    public string gidName { get; set; }

    [Description("数量")]
    [Required(ErrorMessage = "数量必填")]
    public string planNum { get; set; }

    [Description("单价")]
    [Required(ErrorMessage = "单价必填")]
    public string price { get; set; }

    [Description("金额")]
    [Required(ErrorMessage = "金额必填")]
    public string amount { get; set; }

    [Description("付款方式")]
    [Required(ErrorMessage = "付款方式必填")]
    public string payment { get; set; }

    [Description("采购渠道")]
    [Required(ErrorMessage = "采购渠道必填")]
    public string channel { get; set; }

    [Description("生产日期")]
    public DateTime? productionDate { get; set; }

    [Description("保质期")]
    public string retention { get; set; }

    public string gid { get; set; }
    public string supplier { get; set; }
}

//public class ErpBuyorderCgImportErrorData : ErpBuyorderCgImportData
//{
//    /// <summary>
//    /// 错误信息
//    /// </summary>
//    [Display(Name = "错误信息")]
//    public override string ErrorMessage { get; set; }
//}


[SuppressSniffer]
public class ErpBuyorderCgImportResultOutput
{
    /// <summary>
    /// 导入成功条数.
    /// </summary>
    public int snum { get; set; }

    /// <summary>
    /// 导入失败条数.
    /// </summary>
    public int fnum { get; set; }

    /// <summary>
    /// 导入结果状态(0：成功，1：失败).
    /// </summary>
    public int resultType { get; set; }

    /// <summary>
    /// 失败结果集合.
    /// </summary>
    public List<ErpBuyorderCgImportData> failResult { get; set; }

    /// <summary>
    /// 失败结果集合文件地址
    /// </summary>
    public string failFileUrl { get; set; }
}
