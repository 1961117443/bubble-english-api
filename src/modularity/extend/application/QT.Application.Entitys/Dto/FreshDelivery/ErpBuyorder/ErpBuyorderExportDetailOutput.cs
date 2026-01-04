using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpBuyorder;

public class ErpBuyorderExportDetailOutput
{
    #region 主表数据

    /// <summary>
    /// 公司.
    /// </summary>
    [Display(Name = "公司")]
    public string oidName { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    [Display(Name = "订单编号")]
    public string no { get; set; }

    /// <summary>
    /// 任务备注.
    /// </summary>
    [Display(Name = "任务备注")]
    public string taskRemark { get; set; }

    /// <summary>
    /// 计划采购时间.
    /// </summary>
    [Display(Name = "计划采购时间")]
    public DateTime? taskBuyTime { get; set; }

    /// <summary>
    /// 指派采购人员名称.
    /// </summary>
    [Display(Name = "指派采购人员")]
    public string taskToUserName { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    [Display(Name = "状态")]
    public string state { get; set; }
    #endregion

    #region 从表数据

    /// <summary>
    /// 供应商.
    /// </summary>
    [Display(Name = "供应商")]
    public string supplierName { get; set; }

    /// <summary>
    /// 商品名称.
    /// </summary>
    [Display(Name = "商品")]
    public string productName { get; set; }

    /// <summary>
    /// 规格.
    /// </summary>
    [Display(Name = "规格")]
    public string gidName { get; set; }

    /// <summary>
    /// 单位.
    /// </summary>
    [Display(Name = "单位")]
    public string gidUnit { get; set; }

    /// <summary>
    /// 付款属性.
    /// </summary>
    [Display(Name = "付款属性")]
    public string payment { get; set; }

    /// <summary>
    /// 预计采购数量.
    /// </summary>
    [Display(Name = "预计采购数量")]
    public decimal planNum { get; set; }

    /// <summary>
    /// 采购数量.
    /// </summary>
    [Display(Name = "采购数量")]
    public decimal? num { get; set; }

    /// <summary>
    /// 采购渠道.
    /// </summary>
    [Display(Name = "采购渠道")]
    public string channel { get; set; }

    /// <summary>
    /// 一级类目.
    /// </summary>
    [Display(Name = "一级类目")]
    public string rootProducttype { get; set; }


    ///// <summary>
    ///// 采购时间.
    ///// </summary>
    //[Display(Name = "采购时间")]
    //public DateTime? buyTime { get; set; }


    /// <summary>
    /// 备注
    /// </summary>
    [Display(Name = "备注")]
    public string remark { get; set; }

    [Display(Name = "单价")]
    public decimal? price { get; set; }

    [Display(Name = "金额")]
    public decimal? amount { get; set; }

    /// <summary>
    /// 库存数量.
    /// </summary>
    [Display(Name = "库存数量")]
    public decimal? storeNum { get; set; }
    #endregion
}

public class ErpBuyorderCkExportDetailOutput
{
    public string id { get; set; }
    /// <summary>
    /// 计划采购时间.
    /// </summary>
    [Display(Name = "计划采购时间")]
    public string taskBuyTime { get; set; }

    /// <summary>
    /// 入库单号.
    /// </summary>
    [Display(Name = "入库单号")]
    public string rkNo { get; set; }


    /// <summary>
    /// 供应商.
    /// </summary>
    [Display(Name = "供应商")]
    public string supplierName { get; set; }



    /// <summary>
    /// 商品名称.
    /// </summary>
    [Display(Name = "商品")]
    public string productName { get; set; }

    /// <summary>
    /// 规格.
    /// </summary>
    [Display(Name = "规格")]
    public string gidName { get; set; }

    /// <summary>
    /// 单位.
    /// </summary>
    [Display(Name = "单位")]
    public string gidUnit { get; set; }



    /// <summary>
    /// 实际入库数量.
    /// </summary>
    [Display(Name = "实际入库数量")]
    public decimal realInNum { get; set; }



    [Display(Name = "单价")]
    public decimal? price { get; set; }

    [Display(Name = "金额")]
    public decimal? amount { get; set; }

    /// <summary>
    /// 一级类别.
    /// </summary>
    [Display(Name = "一级类别")]
    public string rootProducttype { get; set; }

    /// <summary>
    /// 仓库
    /// </summary>
    [Display(Name = "仓库")]
    public string storeRomeIdName { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [Display(Name = "备注")]
    public string remark { get; set; }

    [Display(Name = "退回数量")]
    public decimal? thNum { get; set; }

    [Display(Name = "退回金额")]
    public decimal? thAmount { get; set; }

    /// <summary>
    /// 付款属性.
    /// </summary>
    [Display(Name = "付款属性")]
    public string payment { get; set; }


    /// <summary>
    /// 采购人员.
    /// </summary>
    [Display(Name = "采购人员")]
    public string taskToUserName { get; set; }

    /// <summary>
    /// 审核人员.
    /// </summary>
    [Display(Name = "审核人员")]
    public string auditUserName { get; set; }


    //#region 主表数据

    ///// <summary>
    ///// 公司.
    ///// </summary>
    //[Display(Name = "公司")]
    //public string oidName { get; set; }

    ///// <summary>
    ///// 订单编号.
    ///// </summary>
    //[Display(Name = "订单编号")]
    //public string no { get; set; }

    ///// <summary>
    ///// 任务备注.
    ///// </summary>
    //[Display(Name = "任务备注")]
    //public string taskRemark { get; set; }


    ///// <summary>
    ///// 状态.
    ///// </summary>
    //[Display(Name = "状态")]
    //public string state { get; set; }
    //#endregion

    //#region 从表数据


    ///// <summary>
    ///// 预计采购数量.
    ///// </summary>
    //[Display(Name = "预计采购数量")]
    //public decimal planNum { get; set; }

    ///// <summary>
    ///// 采购数量.
    ///// </summary>
    //[Display(Name = "采购数量")]
    //public decimal? num { get; set; }

    ///// <summary>
    ///// 采购渠道.
    ///// </summary>
    //[Display(Name = "采购渠道")]
    //public string channel { get; set; }




    /////// <summary>
    /////// 采购时间.
    /////// </summary>
    ////[Display(Name = "采购时间")]
    ////public DateTime? buyTime { get; set; }




    ///// <summary>
    ///// 库存数量.
    ///// </summary>
    //[Display(Name = "库存数量")]
    //public decimal? storeNum { get; set; }
    //#endregion
}



public class ErpBuyorderExportDetailInfoOutput
{
    /// 商品名称.
    /// </summary>
    [Display(Name = "商品")]
    public string productName { get; set; }



    /// <summary>
    /// 一级类目.
    /// </summary>
    [Display(Name = "一级类目")]
    public string rootProducttype { get; set; }

    /// <summary>
    /// 规格.
    /// </summary>
    [Display(Name = "规格")]
    public string gidName { get; set; }

 

    /// <summary>
    /// 预计采购数量.
    /// </summary>
    [Display(Name = "预计采购数量")]
    public decimal planNum { get; set; }

    /// <summary>
    /// 库存数量.
    /// </summary>
    [Display(Name = "库存数量")]
    public decimal? storeNum { get; set; }


    /// <summary>
    /// 备注
    /// </summary>
    [Display(Name = "备注")]
    public string remark { get; set; }
}