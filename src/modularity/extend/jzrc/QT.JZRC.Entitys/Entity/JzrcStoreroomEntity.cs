using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.JZRC.Entitys;

/// <summary>
/// 档案室管理实体.
/// </summary>
[SugarTable("jzrc_storeroom")]
[Tenant(ClaimConst.TENANTID)]
public class JzrcStoreroomEntity: CUDEntityBase
{
    /// <summary>
    /// 名称.
    /// </summary>
    [SugarColumn(ColumnName = "Name")]
    public string Name { get; set; }

    /// <summary>
    /// 简介.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }

    /// <summary>
    /// 负责人.
    /// </summary>
    [SugarColumn(ColumnName = "AdminId")]
    public string AdminId { get; set; }

    /// <summary>
    /// 电话.
    /// </summary>
    [SugarColumn(ColumnName = "AdminTel")]
    public string AdminTel { get; set; }

    /// <summary>
    /// 编号.
    /// </summary>
    [SugarColumn(ColumnName = "Code")]
    public string Code { get; set; }

    /// <summary>
    /// 上级id.
    /// </summary>
    [SugarColumn(ColumnName = "PId")]
    public string PId { get; set; }

}