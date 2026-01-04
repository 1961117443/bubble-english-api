using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.JXC.Entitys.Entity;

/// <summary>
/// 大数据测试
/// </summary>
[SugarTable("EXT_BIGDATA")]
[Tenant(ClaimConst.TENANTID)]
public class BigDataEntity : EntityBase<string>
{
    /// <summary>
    /// 编码.
    /// </summary>
    [SugarColumn(ColumnName = "F_ENCODE")]
    public string? EnCode { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULLNAME")]
    public string? FullName { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string? Description { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_CREATORTIME")]
    public DateTime? CreatorTime { get; set; }
}
