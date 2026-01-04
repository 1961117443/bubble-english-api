using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace QT.JXC.Entitys.Dto.Erp;

[SuppressSniffer]
public class ErpProductListImportDataInput
{
    #region 商品主表信息
    /// <summary>
    /// 商品分类
    /// </summary>
    [Display(Name="商品分类")]
    public string tidName { get; set; }
    /// <summary>
    /// 名称.
    /// </summary>
    [Display(Name = "商品名称")]
    public string name { get; set; }

    /// <summary>
    /// 别名.
    /// </summary>
    [Display(Name = "别名")]
    public string nickname { get; set; }

    /// <summary>
    /// 编号.
    /// </summary>
    [Display(Name = "编号")]
    public string no { get; set; }

    /// <summary>
    /// 计量单位.
    /// </summary>
    [Display(Name = "计量单位")]
    public string unit { get; set; }

    /// <summary>
    /// 销售类型.
    /// </summary>
    [Display(Name = "销售类型")]
    public string saletype { get; set; }

    /// <summary>
    /// 产地.
    /// </summary>
    [Display(Name = "产地")]
    public string producer { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    [Display(Name = "排序")]
    public int? sort { get; set; }

    /// <summary>
    /// 存储条件.
    /// </summary>
    [Display(Name = "存储条件")]
    public string storage { get; set; }

    /// <summary>
    /// 保质期.
    /// </summary>
    [Display(Name = "保质期")]
    public string retention { get; set; }


    /// <summary>
    /// 供货商.
    /// </summary>
    [Display(Name = "供货商")]
    public string supplierName { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    [Display(Name = "状态")]
    public string state { get; set; }

    #endregion

    #region 商品规格信息

    /// <summary>
    /// 规格名称.
    /// </summary>
    [Display(Name = "规格名称")]
    public string model_name { get; set; }

    /// <summary>
    /// 计量单位
    /// </summary>
    [Display(Name = "规格单位")]
    public string model_unit { get; set; }

    /// <summary>
    /// 主单位数量比.
    /// </summary>
    [Display(Name = "主单位数量比")]
    public decimal? model_ratio { get; set; }

    /// <summary>
    /// 成本价.
    /// </summary>
    [Display(Name = "成本价")]
    public decimal? model_costPrice { get; set; }

    /// <summary>
    /// 销售价.
    /// </summary>
    [Display(Name = "销售价")]
    public decimal? model_salePrice { get; set; }

    /// <summary>
    /// 起售数.
    /// </summary>
    [Display(Name = "起售数")]
    public decimal? model_minNum { get; set; }


    /// <summary>
    /// 限售数.
    /// </summary>
    [Display(Name = "限售数")]
    public decimal? model_maxNum { get; set; }

    /// <summary>
    /// 毛利率.
    /// </summary>
    [Display(Name = "毛利率")]
    public decimal? model_grossMargin { get; set; }

    /// <summary>
    /// 包装物.
    /// </summary>
    [Display(Name = "包装物")]
    public string model_package { get; set; }

    /// <summary>
    /// 条码
    /// </summary>
    [Display(Name = "条形码")]
    public string model_barCode { get; set; }

    #endregion

    /// <summary>
    /// 关联公司
    /// </summary>
    [Display(Name = "关联公司")]
    public string relationCompany { get; set; }
}


public class ErpProductListExportDataOutput: ErpProductListImportDataInput
{
    public string id { get; set; }

    /// <summary>
    /// 一级品类
    /// </summary>
    [Display(Name = "一级品类")]
    public string rootProducttype { get; set; }
}