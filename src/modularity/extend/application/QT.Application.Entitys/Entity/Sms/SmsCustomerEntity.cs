using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Application.Entitys;

/// <summary>
/// 短信平台-客户信息
/// </summary>
[SugarTable("ext_sms_customer")]
[Tenant(ClaimConst.TENANTID)]
public class SmsCustomerEntity : CUDEntityBase
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

   
    /// <summary>
    /// 客户名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_CustomerName")]
    public string CustomerName { get; set; }

    /// <summary>
    /// 手机号码.
    /// </summary>
    [SugarColumn(ColumnName = "F_ContactTel")]
    public string ContactTel { get; set; }

    /// <summary>
    /// 客户地址.
    /// </summary>
    [SugarColumn(ColumnName = "F_Address")]
    public string Address { get; set; }
}
