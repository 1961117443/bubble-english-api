using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Systems.Entitys.System;

/// <summary>
/// 单据规则



/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_BILLRULE")]
[Tenant(ClaimConst.TENANTID)]
public class BillRuleEntity : CLDEntityBase
{
    /// <summary>
    /// 单据名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULLNAME")]
    public string FullName { get; set; }

    /// <summary>
    /// 单据编码.
    /// </summary>
    [SugarColumn(ColumnName = "F_ENCODE")]
    public string EnCode { get; set; }

    /// <summary>
    /// 单据前缀.
    /// </summary>
    [SugarColumn(ColumnName = "F_PREFIX")]
    public string Prefix { get; set; }

    /// <summary>
    /// 日期格式.
    /// </summary>
    [SugarColumn(ColumnName = "F_DATEFORMAT")]
    public string DateFormat { get; set; }

    /// <summary>
    /// 流水位数.
    /// </summary>
    [SugarColumn(ColumnName = "F_DIGIT")]
    public int? Digit { get; set; }

    /// <summary>
    /// 流水起始.
    /// </summary>
    [SugarColumn(ColumnName = "F_STARTNUMBER")]
    public string StartNumber { get; set; }

    /// <summary>
    /// 流水范例.
    /// </summary>
    [SugarColumn(ColumnName = "F_EXAMPLE")]
    public string Example { get; set; }

    /// <summary>
    /// 当前流水号.
    /// </summary>
    [SugarColumn(ColumnName = "F_THISNUMBER")]
    public int? ThisNumber { get; set; } = 0;

    /// <summary>
    /// 输出流水号.
    /// </summary>
    [SugarColumn(ColumnName = "F_OUTPUTNUMBER")]
    public string OutputNumber { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }
}