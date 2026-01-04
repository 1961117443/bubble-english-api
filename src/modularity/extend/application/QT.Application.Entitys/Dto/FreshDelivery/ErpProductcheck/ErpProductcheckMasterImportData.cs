using QT.Application.Entitys.Dto.FreshDelivery.Base;
using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpProductcheck;

public class ErpProductcheckMasterImportData: BaseImportDataInput
{
    /// <summary>
    /// 盘点公司.
    /// </summary>
    [Display(Name = "盘点公司")]
    [Required(ErrorMessage = "盘点公司必填")]
    public string OidName { get; set; }

    /// <summary>
    /// 盘点日期.
    /// </summary>
    [Display(Name = "盘点日期")]
    [Required(ErrorMessage = "盘点日期必填")]
    public DateTime? checkTime { get; set; }

    /// <summary>
    /// 商品名称.
    /// </summary>
    [Display(Name = "商品名称")]
    [Required(ErrorMessage = "商品名称必填")]
    public string ProductName { get; set; }

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
    //[Required(ErrorMessage = "单位必填")]
    public string ProductUnit { get; set; }
    /// <summary>
    /// 规格ID.
    /// </summary>
    public string gid { get; set; }

    /// <summary>
    /// 系统数量.
    /// </summary>
    public string systemNum { get; set; }

    /// <summary>
    /// 实际数量.
    /// </summary>
    [Display(Name = "实际数量")]
    public string realNum { get; set; }

    /// <summary>
    /// 拆损数量.
    /// </summary>
    public decimal loseNum { get; set; }



    /// <summary>
    /// 仓库.
    /// </summary>
    [Display(Name = "仓库")]
    public string storeRome { get; set; }

    public string storeRomeId { get; set; }

    /// <summary>
    /// 库区.
    /// </summary>
    [Display(Name = "库区")]
    public string storeRomeArea { get; set; }

    public string storeRomeAreaId { get; set; }

    /// <summary>
    /// 入库单价.
    /// </summary>
    [Display(Name = "入库单价")]
    public string price { get; set; }

    /// <summary>
    /// 入库金额.
    /// </summary>
    [Display(Name = "入库金额")]
    public string amount { get; set; }

    /// <summary>
    /// 生产日期.
    /// </summary>
    [Display(Name = "生产日期")]
    public DateTime? productionDate { get; set; }

    /// <summary>
    /// 批次号.
    /// </summary>
    [Display(Name = "批次号")]
    public string batchNumber { get; set; }


    /// <summary>
    /// 保质期.
    /// </summary>
    [Display(Name = "保质期")]
    public string retention { get; set; }

    public string oid { get; set; }
    //public string mid { get; set; }


    /// <summary>
    /// 备注.
    /// </summary>
    [Display(Name = "备注")]
    public string remark { get; set; }
}

public class ErpProductcheckMasterImportErrorData : ErpProductcheckMasterImportData
{
    /// <summary>
    /// 错误信息
    /// </summary>
    [Display(Name = "错误信息")]
    public override string ErrorMessage { get; set; }
}

/// <summary>
/// 调出数据导入 输入.
/// </summary>
[SuppressSniffer]
public class ErpProductcheckMasterImportDataInput
{
    /// <summary>
    /// 导入的数据列表.
    /// </summary>
    public List<ErpProductcheckMasterImportData> list { get; set; }
}

/// <summary>
/// 调出 导出 结果 输出.
/// </summary>
[SuppressSniffer]
public class ErpProductcheckMasterImportResultOutput
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
    public List<ErpProductcheckMasterImportData> failResult { get; set; }

    /// <summary>
    /// 失败结果集合文件地址
    /// </summary>
    public string failFileUrl { get; set; }
}
