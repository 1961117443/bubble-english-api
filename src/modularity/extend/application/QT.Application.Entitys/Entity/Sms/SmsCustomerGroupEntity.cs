using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Application.Entitys;

/// <summary>
/// 短信平台-客户分组
/// </summary>
[SugarTable("ext_sms_customer_group")]
[Tenant(ClaimConst.TENANTID)]
public class SmsCustomerGroupEntity : CUDEntityBase
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }


    /// <summary>
    /// 分组名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_GroupName")]
    public string GroupName { get; set; }
}