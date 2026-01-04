using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Extend.Entitys;

/// <summary>
/// 个人记账记录
/// </summary>
[SugarTable("ext_financial_record")]
[Tenant(ClaimConst.TENANTID)]
public class FinancialRecordEntity : CLDEntityBase
{
    /// <summary>
    /// 记账日期.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_BillDate")]
    public DateTime? BillDate { get; set; }
    /// <summary>
    /// 记账分类 收入=1，支出=0.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_Category")]
    public int Category { get; set; }

    /// <summary>
    /// 金额.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_Amount")]
    public decimal? Amount { get; set; }

    /// <summary>
    /// 标签集合，多个逗号相连.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_Label")]
    public string? Label { get; set; }

    /// <summary>
    /// 费用说明.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_Remark")]
    public string? Remark { get; set; }

    /// <summary>
    /// 附件图片.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_ImageJson")]
    public string? ImageJson { get; set; }
}
