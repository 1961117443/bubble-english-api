using QT.Common.Const;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JXC.Entitys.Entity.ERP;

[SugarTable("erp_store_history")]
[Tenant(ClaimConst.TENANTID)]
public class ErpStoreHistoryEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int autoId { get; set; }

    /// <summary>
    /// 截止时间.
    /// </summary>
    [SugarColumn(ColumnName = "cutDate")]
    public DateTime? cutDate { get; set; }

    /// <summary>
    /// 创建用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_CreatorUserId")]
    public string CreatorUserId { get; set; }

    /// <summary>
    /// 物理主键.
    /// </summary>
    public string id { get; set; }


    /// <summary>
    /// 公司.
    /// </summary>
    [Display(Name = "公司", Order = 1)]
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
    public string supplierName { get; set; }

    /// <summary>
    /// 入库单号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 质检报告
    /// </summary>
    public string qualityReportProof { get; set; }
}
