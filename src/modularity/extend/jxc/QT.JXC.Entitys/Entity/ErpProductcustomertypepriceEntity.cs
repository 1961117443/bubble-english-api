using QT.Common.Const;
using SqlSugar;

namespace QT.JXC.Entitys.Entity;

/// <summary>
/// 商品客户类型定价实体.
/// </summary>
[SugarTable("erp_productcustomertypeprice")]
[Tenant(ClaimConst.TENANTID)]
public class ErpProductcustomertypepriceEntity :ICompanyEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public string Id { get; set; }

    /// <summary>
    /// 公司ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Oid")]
    public string Oid { get; set; }

    /// <summary>
    /// 规格ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Gid")]
    public string Gid { get; set; }

    /// <summary>
    /// 客户类型（数据字典）.
    /// </summary>
    [SugarColumn(ColumnName = "F_Tid")]
    public string Tid { get; set; }

    /// <summary>
    /// 折扣（=价格/原价）.
    /// </summary>
    [SugarColumn(ColumnName = "F_Discount")]
    public decimal Discount { get; set; }

    /// <summary>
    /// 价格（=原价*折扣）.
    /// </summary>
    [SugarColumn(ColumnName = "F_Price")]
    public decimal Price { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_CreatorTime")]
    public DateTime? CreatorTime { get; set; }

    /// <summary>
    /// 创建用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_CreatorUserId")]
    public string CreatorUserId { get; set; }

    /// <summary>
    /// 修改时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_LastModifyTime")]
    public DateTime? LastModifyTime { get; set; }

    /// <summary>
    /// 修改用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_LastModifyUserId")]
    public string LastModifyUserId { get; set; }


    /// <summary>
    /// 计价类型（1：按折扣，2：按价格）.
    /// </summary>
    [SugarColumn(ColumnName = "F_PricingType")]
    public int PricingType { get; set; }

    /// <summary>
    /// 质检报告.
    /// </summary>
    [SugarColumn(ColumnName = "QualityReportProof")]
    public string QualityReportProof { get; set; }
}