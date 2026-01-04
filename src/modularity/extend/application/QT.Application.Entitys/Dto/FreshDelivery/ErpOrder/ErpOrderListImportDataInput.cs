using Microsoft.AspNetCore.Mvc;
using QT.Application.Entitys.Dto.FreshDelivery.Base;
using QT.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOrder;

/// <summary>
/// 订单数据导入 输入.
/// </summary>
[SuppressSniffer]
public class ErpOrderImportDataInput
{
    /// <summary>
    /// 导入的数据列表.
    /// </summary>
    public List<ErpOrderListImportDataInput> list { get; set; }
}

/// <summary>
/// 订单数据 导出 结果 输出.
/// </summary>
[SuppressSniffer]
public class ErpOrderImportResultOutput
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
    public List<ErpOrderListImportDataInput> failResult { get; set; }
}


[SuppressSniffer]
[NonValidation]
public class ErpOrderListImportDataInput:BaseImportDataInput
{
    ///// <summary>
    ///// 序号.
    ///// </summary>
    //[Display(Name = "序号")]
    //public string no { get; set; }

    /// <summary>
    /// 下单日期.
    /// </summary>
    [Display(Name = "下单日期")]
    public string CreateTime { get; set; }

    /// <summary>
    /// 配送日期.
    /// </summary>
    [Display(Name = "配送日期")]
    [Required(ErrorMessage ="配送日期必填")]
    public string Posttime { get; set; }

    /// <summary>
    /// 客户名称.
    /// </summary>
    [Display(Name = "客户名称")]
    [Required(ErrorMessage = "客户名称必填")]
    public string CidName { get; set; }

    /// <summary>
    /// 商品名称.
    /// </summary>
    [Display(Name = "商品名称")]
    [Required(ErrorMessage = "商品名称必填")]
    public string ProductName { get; set; }
    /// <summary>
    /// 商品类别.
    /// </summary>
    [Display(Name = "商品类别")]
    public string ProductType { get; set; }
    /// <summary>
    /// 规格.
    /// </summary>
    [Display(Name = "规格")]
    [Required(ErrorMessage = "规格必填")]
    public string MidName { get; set; }
    /// <summary>
    /// 单位.
    /// </summary>
    [Display(Name = "单位")]
    [Required(ErrorMessage = "单位必填")]
    public string ProductUnit { get; set; }
    /// <summary>
    /// 数量.
    /// </summary>
    [Display(Name = "数量")]
    public string Num { get; set; }
    /// <summary>
    /// 单价.
    /// </summary>
    [Display(Name = "单价")]
    public string SalePrice { get; set; }
    /// <summary>
    /// 总额.
    /// </summary>
    [Display(Name = "总额")]
    public string Amount { get; set; }
    /// <summary>
    /// 餐别.
    /// </summary>
    [Display(Name = "餐别")]
    [Required(ErrorMessage = "餐别必填")]
    public string DiningType { get; set; }
    /// <summary>
    /// 代客下单员.
    /// </summary>
    [Display(Name = "代客下单员")]
    public string CreateUidName { get; set; }
    /// <summary>
    /// 备注.
    /// </summary>
    [Display(Name = "备注")]
    public string Remark { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    [Display(Name = "错误信息")]
    public override string ErrorMessage { get; set; }
}


[SuppressSniffer]
[NonValidation]
public class ErpOrderListExportDataOutput
{
    ///// <summary>
    ///// 序号.
    ///// </summary>
    //[Display(Name = "序号")]
    //public string no { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    [Display(Name = "订单编号")]
    [DisplayName("订单编号")]
    public string OrderNo { get; set; }

    /// <summary>
    /// 下单日期.
    /// </summary>
    [Display(Name = "下单日期")]
    [DisplayName("下单日期")]
    public string CreateTime { get; set; }

    /// <summary>
    /// 配送日期.
    /// </summary>
    [Display(Name = "配送日期")]
    [Required(ErrorMessage = "配送日期必填")]
    [DisplayName("配送日期")]
    public string Posttime { get; set; }

    /// <summary>
    /// 客户名称.
    /// </summary>
    [Display(Name = "客户名称")]
    [Required(ErrorMessage = "客户名称必填")]
    [DisplayName("客户名称")]
    public string CidName { get; set; }

    /// <summary>
    /// 商品名称.
    /// </summary>
    [Display(Name = "商品名称")]
    [Required(ErrorMessage = "商品名称必填")]
    [DisplayName("商品名称")]
    public string ProductName { get; set; }
    /// <summary>
    /// 商品类别.
    /// </summary>
    [Display(Name = "商品类别")]
    [DisplayName("商品类别")]
    public string ProductType { get; set; }
    /// <summary>
    /// 规格.
    /// </summary>
    [Display(Name = "规格")]
    [DisplayName("规格")]
    [Required(ErrorMessage = "规格必填")]
    public string MidName { get; set; }
    /// <summary>
    /// 单位.
    /// </summary>
    [Display(Name = "单位")]
    [DisplayName("单位")]
    [Required(ErrorMessage = "单位必填")]
    public string ProductUnit { get; set; }
    /// <summary>
    /// 数量.
    /// </summary>
    [Display(Name = "数量")]
    [DisplayName("数量")]
    public decimal Num { get; set; }
    /// <summary>
    /// 单价.
    /// </summary>
    [Display(Name = "单价")]
    [DisplayName("单价")]
    public decimal SalePrice { get; set; }
    /// <summary>
    /// 总额.
    /// </summary>
    [Display(Name = "总额")]
    [DisplayName("总额")]
    public decimal Amount { get; set; }
    /// <summary>
    /// 餐别.
    /// </summary>
    [Display(Name = "餐别")]
    [DisplayName("餐别")]
    [Required(ErrorMessage = "餐别必填")]
    public string DiningType { get; set; }
    /// <summary>
    /// 代客下单员.
    /// </summary>
    [Display(Name = "代客下单员")]
    [DisplayName("代客下单员")]
    public string CreateUidName { get; set; }
    /// <summary>
    /// 备注.
    /// </summary>
    [Display(Name = "备注")]
    [DisplayName("备注")]
    public string Remark { get; set; }

    /// <summary>
    /// 分拣数量.
    /// </summary>
    [Display(Name = "分拣数量")]
    [DisplayName("分拣数量")]
    public decimal Num1 { get; set; }


    /// <summary>
    /// 送货人.
    /// </summary>
    [Display(Name = "送货人")]
    [DisplayName("送货人")]
    public string deliveryManIdName { get; set; }
}
