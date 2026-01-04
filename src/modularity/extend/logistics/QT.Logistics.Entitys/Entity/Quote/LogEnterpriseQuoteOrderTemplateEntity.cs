using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 报价订单输出模板
/// </summary>
[SugarTable("log_enterprise_Quote_Order_Template")]
[Tenant(ClaimConst.TENANTID)]
public class LogEnterpriseQuoteOrderTemplateEntity : CLDEntityBase, ILogEnterpriseEntity
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

    /// <summary>
    /// 商家ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_EId")]
    public string EId { get; set; }
}