using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 入库订单表输入参数.
/// </summary>
public class ErpInorderListOutput
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
    /// 入库订单号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 入库类型.
    /// </summary>
    public string inType { get; set; }

    /// <summary>
    /// 订单金额.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 采购订单编号.
    /// </summary>
    public string cgNo { get; set; }


    public string oidName { get; set; }

    public string specialState { get; set; }


    /// <summary>
    /// 调出公司
    /// </summary>
    public string outOidName { get; set; }


    /// <summary>
    /// 订单编号（特殊入库才显示）
    /// </summary>
    public string orderNo { get; set; }

    /// <summary>
    /// 商品数量
    /// </summary>
    public int num { get; set; }

    /// <summary>
    /// 商品名称集合
    /// </summary>
    public string productNames { get; set; }

    /// <summary>
    /// 商品数量汇总
    /// </summary>
    public decimal totalNum { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? creatorTime { get; set; }


    /// <summary>
    /// 供应商名称
    /// </summary>
    public string supplierName { get; set; }

    /// <summary>
    /// 客户名称
    /// </summary>
    public string cidName { get; set; }

    /// <summary>
    /// 规格
    /// </summary>
    public string gidName { get; set; }

    /// <summary>
    /// 单位
    /// </summary>
    public string gidUnit { get; set; }


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


/// <summary>
/// 特殊入库订单列表导出.
/// </summary>
public class ErpInorderListTSExportOutput
{


    [Description("公司")]
    public string oidName { get; set; }

    ///// <summary>
    ///// 公司ID.
    ///// </summary>
    //public string oid { get; set; }

    /// <summary>
    /// 入库订单号.
    /// </summary>
    [Description("入库订单号")]
    public string no { get; set; }


    /// <summary>
    /// 采购订单编号.
    /// </summary>
    [Description("采购订单号")]
    public string cgNo { get; set; }


    /// <summary>
    /// 订单编号（特殊入库才显示）
    /// </summary>
    [Description("销售订单号")]
    public string orderNo { get; set; }


    /// <summary>
    /// 商品名称集合
    /// </summary>
    [Description("商品")]
    public string productNames { get; set; }

    /// <summary>
    /// 规格
    /// </summary>
    [Description("规格")]
    public string gidName { get; set; }

    /// <summary>
    /// 单位
    /// </summary>
    [Description("单位")]
    public string gidUnit { get; set; }

    /// <summary>
    /// 商品数量汇总
    /// </summary>
    [Description("入库数量")]
    public decimal totalNum { get; set; }


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


    /// <summary>
    /// 创建时间
    /// </summary>
    [Description("创建时间")]
    public DateTime? creatorTime { get; set; }



    /// <summary>
    /// 入库类型.
    /// </summary>
    [Description("入库类型")]
    public string inType { get; set; }

    ///// <summary>
    ///// 订单金额.
    ///// </summary>
    //public decimal amount { get; set; }


    [Description("处理状态")]
    public string specialState { get; set; }




    ///// <summary>
    ///// 调出公司
    ///// </summary>
    //public string outOidName { get; set; }



    ///// <summary>
    ///// 商品数量
    ///// </summary>
    //public int num { get; set; }



    ///// <summary>
    ///// 供应商名称
    ///// </summary>
    //public string supplierName { get; set; }

    ///// <summary>
    ///// 客户名称
    ///// </summary>
    //public string cidName { get; set; }
}