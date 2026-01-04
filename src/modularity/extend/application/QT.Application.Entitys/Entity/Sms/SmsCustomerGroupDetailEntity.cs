using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Application.Entitys;

/// <summary>
/// 短信平台-客户分组明细
/// </summary>
[SugarTable("ext_sms_customer_group_detail")]
[Tenant(ClaimConst.TENANTID)]
public class SmsCustomerGroupDetailEntity : EntityBase<long>
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override long Id { get; set; }


    /// <summary>
    /// 分组Id.
    /// </summary>
    [SugarColumn(ColumnName = "F_GId")]
    public string GId { get; set; }

    /// <summary>
    /// 客户Id.
    /// </summary>
    [SugarColumn(ColumnName = "F_CId")]
    public string CId { get; set; }
}
