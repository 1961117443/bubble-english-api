using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.SDMS.Entitys;

/// <summary>
/// 售电系统-合同模板
/// </summary>
[SugarTable("sdms_contract_template")]
[Tenant(ClaimConst.TENANTID)]
public class ContractTemplateEntity : CLDEntityBase
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