using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.JZRC.Entitys;

/// <summary>
/// 证书分类实体.
/// </summary>
[SugarTable("jzrc_certificate_category")]
[Tenant(ClaimConst.TENANTID)]
public class JzrcCertificateCategoryEntity :CUDEntityBase
{
    /// <summary>
    /// 名称.
    /// </summary>
    [SugarColumn(ColumnName = "Name")]
    public string Name { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    [SugarColumn(ColumnName = "Order")]
    public int? Order { get; set; }

}