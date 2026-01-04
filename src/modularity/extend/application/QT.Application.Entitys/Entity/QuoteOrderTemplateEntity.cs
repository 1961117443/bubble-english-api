using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Extend.Entitys;

/// <summary>
/// 报价订单输出模板
/// </summary>
[SugarTable("EXT_Quote_Order_Template")]
[Tenant(ClaimConst.TENANTID)]
public class QuoteOrderTemplateEntity : CLDEntityBase
{
    /// <summary>
    /// 模板名称.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_Name")]
    public string? Name { get; set; }

    /// <summary>
    /// 模板文件.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_FileUrl")]
    public string? FileUrl { get; set; }

    /// <summary>
    /// 模板变量.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_Property")]
    public string? Property { get; set; }
}