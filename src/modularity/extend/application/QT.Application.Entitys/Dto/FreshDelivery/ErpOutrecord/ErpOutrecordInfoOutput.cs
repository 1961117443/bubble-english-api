using QT.Application.Entitys.Dto.FreshDelivery.ErpInrecord;
using System.ComponentModel;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOutrecord;

/// <summary>
/// 商品出库记录输出参数.
/// </summary>
public class ErpOutrecordInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 商品规格ID.
    /// </summary>
    public string gid { get; set; }

    /// <summary>
    /// 数量.
    /// </summary>
    public decimal num { get; set; }

    /// <summary>
    /// 单价.
    /// </summary>
    public decimal price { get; set; }

    /// <summary>
    /// 总价.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    public string gidName { get; set; }
    public string productName { get; set; }

    public List<ErpStoreInfoOutput> storeDetailList { get; set; }

    /// <summary>
    /// 成本金额
    /// </summary>
    public decimal costAmount { get; set; }


    /// <summary>
    /// 成本单价
    /// </summary>
    public decimal costPrice => num != 0 ? Math.Round(costAmount / num, 2) : 0;
}


/// <summary>
/// 商品出库记录输出参数.
/// </summary>
public class ErpOutrecordExportOutput
{
    #region 主表
    public string outid { get; set; }
    ///// <summary>
    ///// 主键.
    ///// </summary>
    //public string id { get; set; }

    ///// <summary>
    ///// 公司ID.
    ///// </summary>
    //public string oid { get; set; }

    /// <summary>
    /// 公司.
    /// </summary>
    [Description("公司")]
    public string oidName { get; set; }

    /// <summary>
    /// 出库订单号.
    /// </summary>
    [Description("出库订单号")]
    public string no { get; set; }

    /// <summary>
    /// 出库类型.
    /// </summary>
    [Description("出库类型")]
    public string inType { get; set; }

    ///// <summary>
    ///// 订单金额.
    ///// </summary>
    //public decimal amount { get; set; }

    ///// <summary>
    ///// 销售订单编号.
    ///// </summary>
    //public string xsNo { get; set; }

    ///// <summary>
    ///// 商品出库记录.
    ///// </summary>
    //public List<ErpOutrecordInfoOutput> erpOutrecordList { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [Description("主备注")]
    public string remark { get; set; }


    ///// <summary>
    ///// 调入公司ID.
    ///// </summary>
    //public string inOid { get; set; }

    ///// <summary>
    ///// 订单状态（0：正常，1：调拨中，2：已调入，）.
    ///// </summary>
    //public string state { get; set; } 
    #endregion



    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    ///// <summary>
    ///// 商品规格ID.
    ///// </summary>
    //public string gid { get; set; }



    ///// <summary>
    ///// 单价.
    ///// </summary>
    //public decimal price { get; set; }

    ///// <summary>
    ///// 总价.
    ///// </summary>
    //public decimal amount { get; set; }

  

    [Description("商品名称")]
    public string productName { get; set; }


    [Description("商品规格")]
    public string gidName { get; set; }


    /// <summary>
    /// 数量.
    /// </summary>
    [Description("数量")]
    public decimal num { get; set; }


    /// <summary>
    /// 备注.
    /// </summary>

    [Description("备注")]
    public string itremark { get; set; }


}

public class ErpOutrecordThExportOutput: ErpOutrecordExportOutput
{
    /// <summary>
    /// 退货供应商.
    /// </summary>

    [Description("退货供应商")]
    public string supplierName { get; set; }

    /// <summary>
    /// 入库单号.
    /// </summary>

    [Description("入库单号")]
    public string inNo { get; set; }
}