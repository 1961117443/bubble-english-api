using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Application.Entitys;

/// <summary>
/// 报销明细实体.
/// </summary>
[SugarTable("ext_expense_detail")]
[Tenant(ClaimConst.TENANTID)]
public class ExtExpenseDetailEntity: CLDEntityBase
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

  

    /// <summary>
    /// 报销单id.
    /// </summary>
    [SugarColumn(ColumnName = "F_Fid")]
    public string Fid { get; set; }

    /// <summary>
    /// 报销说明.
    /// </summary>
    [SugarColumn(ColumnName = "F_Title")]
    public string Title { get; set; }

    /// <summary>
    /// 金额.
    /// </summary>
    [SugarColumn(ColumnName = "F_Amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 转入id.
    /// </summary>
    [SugarColumn(ColumnName = "F_InId")]
    public string InId { get; set; }

}