using System.ComponentModel.DataAnnotations;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOutorder;

/// <summary>
/// 出库订单表输入参数.
/// </summary>
public class ErpOutorderListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 公司ID.
    /// </summary>
    public string oid { get; set; }

    /// <summary>
    /// 出库订单号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 出库类型.
    /// </summary>
    public string inType { get; set; }

    /// <summary>
    /// 订单金额.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 销售订单编号.
    /// </summary>
    public string xsNo { get; set; }

    /// <summary>
    /// 公司名称.
    /// </summary>
    public string oidName { get; set; }

    /// <summary>
    /// 出库明细信息
    /// </summary>
    public string items { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    public DateTime? creatorTime { get; set; }

    /// <summary>
    /// 调入公司
    /// </summary>
    public string inOidName { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public string state { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? creator { get; set; }


    /// <summary>
    /// 供应商
    /// </summary>
    public string? supplierName { get; set; }


    /// <summary>
    /// 成本合计
    /// </summary>
    public decimal? costAmount { get; set; }

    /// <summary>
    /// 入库单号
    /// </summary>
    public string? inNo { get; set; }
}

public class ErpOutorderXsListOutput: ErpOutorderListOutput
{
    public List<ErpOutorderListOutput> children { get; set; }
}

public class ErpOutorderListDbExportOutput
{
    //创建时间、商品名称、规格、数量、单价、金额、备注

    [Display(Name = "创建时间")]
    public string createTime { get; set; }
    [Display(Name = "商品名称")]
    public string productName { get; set; }
    [Display(Name = "规格")]
    public string name { get; set; }
    [Display(Name = "数量")]
    public decimal num { get; set; }
    [Display(Name = "单价")]
    public decimal price { get; set; }
    [Display(Name = "金额")]
    public decimal amount { get; set; }
    [Display(Name = "备注")]
    public string remark { get; set; }
}