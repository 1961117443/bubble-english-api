using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 商品分类实体.
/// </summary>
[SugarTable("log_enterprise_producttype")]
[Tenant(ClaimConst.TENANTID)]
public class LogEnterpriseProducttypeEntity : CUDEntityBase, ILogEnterpriseEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 商家ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_EId")]
    public string EId { get; set; }

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
    /// 拼音首字母.
    /// </summary>
    [SugarColumn(ColumnName = "F_FirstChar")]
    public string FirstChar { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }
}