using QT.Common.Const;
using QT.Extras.DatabaseAccessor.SqlSugar;
using SqlSugar;

namespace QT.JXC.Entitys.Entity.ERP;

/// <summary>
/// 仓库信息实体.
/// </summary>
[SugarTable("erp_storeroom")]
[Tenant(ClaimConst.TENANTID)]
public class ErpStoreroomEntity :ICompanyEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public string Id { get; set; }


    /// <summary>
    /// 父级仓库ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Fid")]
    public string Fid { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_Name")]
    public string Name { get; set; }

    /// <summary>
    /// 拼音首字母.
    /// </summary>
    [SugarColumn(ColumnName = "F_FirstChar")]
    public string FirstChar { get; set; }

    /// <summary>
    /// 仓库地址.
    /// </summary>
    [SugarColumn(ColumnName = "F_Address")]
    public string Address { get; set; }

    /// <summary>
    /// 联系电话.
    /// </summary>
    [SugarColumn(ColumnName = "F_Phone")]
    public string Phone { get; set; }

    /// <summary>
    /// 负责人.
    /// </summary>
    [SugarColumn(ColumnName = "F_Admin")]
    public string Admin { get; set; }

    /// <summary>
    /// 负责人电话.
    /// </summary>
    [SugarColumn(ColumnName = "F_Admintel")]
    public string Admintel { get; set; }

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
    /// 公司id.
    /// </summary>
    [SugarColumn(ColumnName = "F_Oid")]
    public string Oid { get; set; }
}