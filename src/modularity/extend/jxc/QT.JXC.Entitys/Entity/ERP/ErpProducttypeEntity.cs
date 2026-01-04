using QT.Common.Const;
using QT.Extras.DatabaseAccessor.SqlSugar;
using SqlSugar;

namespace QT.JXC.Entitys.Entity.ERP;

/// <summary>
/// 商品分类管理实体.
/// </summary>
[SugarTable("erp_producttype")]
[Tenant(ClaimConst.TENANTID)]
public class ErpProducttypeEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public string Id { get; set; }


    /// <summary>
    /// 父级ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Fid")]
    public string Fid { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_Name")]
    public string Name { get; set; }

    /// <summary>
    /// 分类编号.
    /// </summary>
    [SugarColumn(ColumnName = "F_Code")]
    public string Code { get; set; }

    /// <summary>
    /// 拼音首字母.
    /// </summary>
    [SugarColumn(ColumnName = "F_FirstChar")]
    public string FirstChar { get; set; }

    /// <summary>
    /// 分类说明.
    /// </summary>
    [SugarColumn(ColumnName = "F_Remark")]
    public string Remark { get; set; }

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
    /// 序号.
    /// </summary>
    [SugarColumn(ColumnName = "F_Order")]
    public int? Order { get; set; }

}