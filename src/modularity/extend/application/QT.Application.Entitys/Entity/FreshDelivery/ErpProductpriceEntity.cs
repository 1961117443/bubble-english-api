using QT.Common.Const;
using QT.Common.Contracts;
using QT.Extras.DatabaseAccessor.SqlSugar;
using SqlSugar;

namespace QT.Application.Entitys.FreshDelivery;

/// <summary>
/// 商品定价实体.
/// </summary>
[SugarTable("erp_productprice")]
[Tenant(ClaimConst.TENANTID)]
public class ErpProductpriceEntity: IEntity<string>, ICompanyEntity
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
    /// 客户ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Cid")]
    public string Cid { get; set; }

    /// <summary>
    /// 价格.
    /// </summary>
    [SugarColumn(ColumnName = "F_Price")]
    public decimal Price { get; set; }

    /// <summary>
    /// 定价时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_Time")]
    public DateTime? Time { get; set; }

    /// <summary>
    /// 操作人.
    /// </summary>
    [SugarColumn(ColumnName = "F_UserId")]
    public string UserId { get; set; }

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
    /// 质检报告.
    /// </summary>
    [SugarColumn(ColumnName = "QualityReportProof")]
    public string QualityReportProof { get; set; }
}