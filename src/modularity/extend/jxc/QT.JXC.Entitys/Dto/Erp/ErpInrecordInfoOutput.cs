using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 商品入库记录输出参数.
/// </summary>
public class ErpInrecordInfoOutput
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
    /// 采购订单数量.
    /// </summary>
    public decimal orderNum { get; set; }

    /// <summary>
    /// 入库数量.
    /// </summary>
    public decimal inNum { get; set; }

    /// <summary>
    /// 入库单价.
    /// </summary>
    public decimal price { get; set; }

    /// <summary>
    /// 入库金额.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    public string gidName { get; set; }
    public string productName { get; set; }

    public string storeRomeAreaId { get; set; }
    public string storeRomeId { get; set; }

    public string bid { get; set; }

    /// <summary>
    /// 商品价格.
    /// </summary>
    public decimal productPrice { get; set; }

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


    /// <summary>
    /// 关联特殊入库
    /// </summary>
    public decimal tsNum { get; set; }

    /// <summary>
    /// 订单编号（特殊入库才显示）
    /// </summary>
    public string orderNo { get; set; }


    /// <summary>
    /// 客户名称
    /// </summary>
    public string cidName { get; set; }
}

[Description("库存明细")]
public class ErpStoreInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }


    /// <summary>
    /// 公司.
    /// </summary>
    [Display(Name ="公司",Order =1)]
    public string oidName { get; set; }
    /// <summary>
    /// 商品规格ID.
    /// </summary>
    public string gid { get; set; }

    /// <summary>
    /// 库存数量.
    /// </summary>
    [Display(Name = "剩余数量", Order = 9)]
    public decimal num { get; set; }

    /// <summary>
    /// 规格名称.
    /// </summary>
    [Display(Name = "商品规格", Order = 4)]
    public string gidName { get; set; }

    /// <summary>
    /// 单位.
    /// </summary>
    [Display(Name = "单位", Order = 4)]
    public string gidUnit { get; set; }

    /// <summary>
    /// 商品名称.
    /// </summary>
    [Display(Name = "商品名称", Order = 3)]
    public string productName { get; set; }

    /// <summary>
    /// 盘点时间.
    /// </summary>
    public DateTime? checkTime { get; set; }

    /// <summary>
    /// 入库时间.
    /// </summary>
    [Display(Name = "入库时间", Order = 5)]
    public DateTime? creatorTime { get; set; }

    /// <summary>
    /// 库区.
    /// </summary>
    [Display(Name = "库区", Order = 8)]
    public string storeRomeArea { get; set; }

    /// <summary>
    /// 仓库.
    /// </summary>
    [Display(Name = "仓库", Order = 7)]
    public string storeRome { get; set; }

    /// <summary>
    /// 公司id.
    /// </summary>
    public string oid { get; set; }


    /// <summary>
    /// 分类id.
    /// </summary>
    public string tid { get; set; }

    /// <summary>
    /// 分类名称.
    /// </summary>
    [Display(Name = "商品分类", Order = 2)]
    public string tidName { get; set; }

    /// <summary>
    /// 入库单价.
    /// </summary>
    [Display(Name = "单价", Order = 10)]
    public decimal price { get; set; }

    /// <summary>
    /// 保质期
    /// </summary>
    [Display(Name = "保质期", Order = 13)]
    public string retention { get; set; }

    /// <summary>
    /// 入库天数
    /// </summary>
    [Display(Name = "入库天数", Order = 6)]
    public int days { get; set; }

    /// <summary>
    /// 规格转化比
    /// </summary>
    public decimal? ratio { get; set; }

    /// <summary>
    /// 商品id.
    /// </summary>
    public string productId { get; set; }

    /// <summary>
    /// 库区Id.
    /// </summary>
    public string storeRomeAreaId { get; set; }

    /// <summary>
    /// 仓库Id.
    /// </summary>
    public string storeRomeId { get; set; }


    /// <summary>
    /// 金额.
    /// </summary>
    [Display(Name = "金额", Order = 11)]
    public decimal amount { get; set; }


    /// <summary>
    /// 生产日期.
    /// </summary>
    [Display(Name = "生产日期", Order = 12)]
    public DateTime? productionDate { get; set; }

    /// <summary>
    /// 批次号.
    /// </summary>
    public string batchNumber { get; set; }


    ///// <summary>
    ///// 保质期.
    ///// </summary>
    //public string _retention { get; set; }

    /// <summary>
    /// 入库类型.
    /// </summary>
    public string inType { get; set; }

    /// <summary>
    /// 单据id.
    /// </summary>
    public string billId { get; set; }


    /// <summary>
    /// 采购单号.
    /// </summary>
    public string cgNo { get; set; }

    /// <summary>
    /// 供应商
    /// </summary>
    [Display(Name = "供应商", Order = 15)]
    public string supplierName { get; set; }

    /// <summary>
    /// 入库单号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 质检报告
    /// </summary>
    public string qualityReportProof { get; set; }

    /// <summary>
    /// 一级分类
    /// </summary>
    public string rootProducttype { get; set; }

    //public string rootTypeName { get; set; }
}

public class ErpStoreHistoryInfoOutput: ErpStoreInfoOutput
{
    public int autoId { get; set; }

    [Display(Name = "截止日期", Order = 0)]
    public DateTime? cutDate { get; set; }
}



/// <summary>
/// 商品入库记录输出参数.
/// </summary>
public class ErpInrecordInfoExportDetailOutput
{
    /// <summary>
    /// 公司ID.
    /// </summary>
    [Display(Name = "公司", Order = -2)]
    public string oidName { get; set; }

    /// <summary>
    /// 入库订单号.
    /// </summary>
    [Display(Name = "入库订单号", Order = -1)]
    public string no { get; set; }


    public string id { get; set; }
    /// <summary>
    /// 入库数量.
    /// </summary>
    [Display(Name = "入库数量", Order = 2)]
    public decimal inNum { get; set; }

    /// <summary>
    /// 入库单价.
    /// </summary>
    [Display(Name = "入库单价", Order = 3)]
    public decimal price { get; set; }

    /// <summary>
    /// 入库金额.
    /// </summary>
    [Display(Name = "入库金额", Order = 4)]
    public decimal amount { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>

    [Display(Name = "备注", Order = 5)]
    public string remark { get; set; }

    [Display(Name = "商品规格", Order = 1)]
    public string gidName { get; set; }
    
    [Display(Name = "商品名称", Order = 0)]
    public string productName { get; set; }


    [Display(Name = "库区", Order = 7)]
    public string storeRomeAreaIdName { get; set; }


    [Display(Name = "仓库", Order = 6)]
    public string storeRomeIdName { get; set; }


    /// <summary>
    /// 订单编号（特殊入库才显示）
    /// </summary>

    [Display(Name = "订单编号", Order = 8)]
    public string orderNo { get; set; }


    /// <summary>
    /// 客户名称
    /// </summary>

    [Display(Name = "客户名称", Order = 9)]
    public string cidName { get; set; }
}
