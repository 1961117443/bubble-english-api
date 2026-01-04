
using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpProductcheck;

/// <summary>
/// 盘点记录输出参数.
/// </summary>
public class ErpProductcheckInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 规格ID.
    /// </summary>
    public string gid { get; set; }

    /// <summary>
    /// 系统数量.
    /// </summary>
    public decimal systemNum { get; set; }

    /// <summary>
    /// 实际数量.
    /// </summary>
    public decimal realNum { get; set; }

    /// <summary>
    /// 拆损数量.
    /// </summary>
    public decimal loseNum { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 商品规格.
    /// </summary>
    public string gidName { get; set; }

    /// <summary>
    /// 商品名称.
    /// </summary>
    public string productName { get; set; }


    /// <summary>
    /// 仓库.
    /// </summary>
    public string storeRomeId { get; set; }
    public string storeRomeIdName { get; set; }

    /// <summary>
    /// 库区.
    /// </summary>
    public string storeRomeAreaId { get; set; }
    public string storeRomeAreaIdName { get; set; }


    public decimal? ratio { get; set; }

    /// <summary>
    /// 入库单价.
    /// </summary>
    public decimal price { get; set; }

    /// <summary>
    /// 入库金额.
    /// </summary>
    public decimal amount { get; set; }


    /// <summary>
    /// 商品单位.
    /// </summary>
    public string productUnit { get; set; }

    /// <summary>
    /// 生产日期.
    /// </summary>
    public DateTime? productionDate { get; set; }

    /// <summary>
    /// 批次号.
    /// </summary>
    public string batchNumber { get; set; }


    /// <summary>
    /// 保质期.
    /// </summary>
    public string retention { get; set; }
}

/// <summary>
/// 盘点单导出记录
/// </summary>
[SuppressSniffer]
public class ErpProductcheckInfoExportOutput
{
    ///// <summary>
    ///// 公司ID.
    ///// </summary>
    //public string oid { get; set; }

    [DisplayAttribute(Name = "公司")]
    public string oidName { get; set; }

    /// <summary>
    /// 盘点日期.
    /// </summary>
    [DisplayAttribute(Name = "盘点日期")]
    public string? checkTime { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    [DisplayAttribute(Name = "订单编号")]
    public string no { get; set; }

    /// <summary>
    /// 审核时间.
    /// </summary>
    [DisplayAttribute(Name = "审核时间")]
    public DateTime? auditTime { get; set; }


    //////////////////////////////////////////////////////////////

    /// <summary>
    /// 商品名称.
    /// </summary>
    [DisplayAttribute(Name = "商品名称")]
    public string productName { get; set; }

    /// <summary>
    /// 商品规格.
    /// </summary>
    [DisplayAttribute(Name = "商品规格")]
    public string gidName { get; set; }


    /// <summary>
    /// 商品单位.
    /// </summary>
    [DisplayAttribute(Name = "商品单位")]
    public string productUnit { get; set; }


    /// <summary>
    /// 系统数量.
    /// </summary>
    [DisplayAttribute(Name = "系统数量")]
    public decimal systemNum { get; set; }

    /// <summary>
    /// 实际数量.
    /// </summary>

    [DisplayAttribute(Name = "实际数量")]
    public decimal realNum { get; set; }

    /// <summary>
    /// 拆损数量.
    /// </summary>
    [DisplayAttribute(Name = "差异数量")]
    public decimal loseNum { get; set; }


    /// <summary>
    /// 入库单价.
    /// </summary>
    [DisplayAttribute(Name = "入库单价")]
    public decimal price { get; set; }

    /// <summary>
    /// 入库金额.
    /// </summary>
    [DisplayAttribute(Name = "入库金额")]
    public decimal amount { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [DisplayAttribute(Name = "仓库")]
    public string storeRomeIdName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [DisplayAttribute(Name = "库区")]
    public string storeRomeAreaIdName { get; set; }



    /// <summary>
    /// 备注.
    /// </summary>
    [DisplayAttribute(Name = "备注")]
    public string remark { get; set; }

    /// <summary>
    /// 生产日期.
    /// </summary>
    [DisplayAttribute(Name = "生产日期")]
    public string? productionDate { get; set; }

    /// <summary>
    /// 批次号.
    /// </summary>
    [DisplayAttribute(Name = "批次号")]
    public string batchNumber { get; set; }


    /// <summary>
    /// 保质期.
    /// </summary>
    [DisplayAttribute(Name = "保质期")]
    public string retention { get; set; }

}