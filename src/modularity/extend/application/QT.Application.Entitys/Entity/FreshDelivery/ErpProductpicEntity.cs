using QT.Common.Const;
using QT.Extras.DatabaseAccessor.SqlSugar;
using SqlSugar;

namespace QT.Application.Entitys.FreshDelivery;

/// <summary>
/// 商品图片实体.
/// </summary>
[SugarTable("erp_productpic")]
[Tenant(ClaimConst.TENANTID)]
public class ErpProductpicEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public string Id { get; set; }


    /// <summary>
    /// 商品ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Pid")]
    public string Pid { get; set; }

    /// <summary>
    /// 标题.
    /// </summary>
    [SugarColumn(ColumnName = "F_Title")]
    public string Title { get; set; }

    /// <summary>
    /// 简介.
    /// </summary>
    [SugarColumn(ColumnName = "F_Remark")]
    public string Remark { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    [SugarColumn(ColumnName = "F_Sort")]
    public int? Sort { get; set; }

    /// <summary>
    /// 上传时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_Uptime")]
    public DateTime? Uptime { get; set; }

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
    /// 商品图片地址.
    /// </summary>
    [SugarColumn(ColumnName = "F_Pic")]
    public string Pic { get; set; }

}