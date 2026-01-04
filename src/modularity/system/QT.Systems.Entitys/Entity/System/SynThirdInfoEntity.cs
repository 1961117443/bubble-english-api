using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Systems.Entitys.System;

/// <summary>
/// 第三方工具对象同步表



/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_SYNTHIRDINFO")]
[Tenant(ClaimConst.TENANTID)]
public class SynThirdInfoEntity : CLEntityBase
{
    /// <summary>
    /// 第三方类型(1:企业微信;2:钉钉).
    /// </summary>
    [SugarColumn(ColumnName = "F_THIRDTYPE")]
    public int? ThirdType { get; set; }

    /// <summary>
    /// 数据类型(1:公司;2:部门;3:用户).
    /// </summary>
    [SugarColumn(ColumnName = "F_DATATYPE")]
    public int? DataType { get; set; }

    /// <summary>
    /// 系统对象ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_SYSOBJID")]
    public string SysObjId { get; set; }

    /// <summary>
    /// 第三对象ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_THIRDOBJID")]
    public string ThirdObjId { get; set; }

    /// <summary>
    /// 0:未同步;1:同步成功;2:同步失败.
    /// </summary>
    [SugarColumn(ColumnName = "F_SYNSTATE")]
    public string SynState { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string Description { get; set; }
}