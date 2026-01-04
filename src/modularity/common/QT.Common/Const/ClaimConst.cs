using QT.DependencyInjection;

namespace QT.Common.Const;

/// <summary>
/// Claim常量.
/// </summary>
[SuppressSniffer]
public class ClaimConst
{
    /// <summary>
    /// 用户Id.
    /// </summary>
    public const string CLAINMUSERID = "UserId";

    /// <summary>
    /// 用户姓名.
    /// </summary>
    public const string CLAINMREALNAME = "UserName";

    /// <summary>
    /// 账号.
    /// </summary>
    public const string CLAINMACCOUNT = "Account";

    /// <summary>
    /// 是否超级管理.
    /// </summary>
    public const string CLAINMADMINISTRATOR = "Administrator";

    /// <summary>
    /// 租户ID.
    /// </summary>
    public const string TENANTID = "TenantId";

    /// <summary>
    /// 租户DbName.
    /// </summary>
    public const string TENANTDBNAME = "TenantDbName";

    /// <summary>
    /// 单一登录方式（1：后登录踢出先登录 2：同时登录）.
    /// </summary>
    public const string SINGLELOGIN = "SingleLogin";

    /// <summary>
    /// 公司id
    /// </summary>
    public const string CLAINMCOMPANYID = "CompanyId";


    /// <summary>
    /// 是否集团账号
    /// </summary>
    public const string CLAINM_JT_COMPANY_ACCOUNT = "CLAINM_JT_COMPANY_ACCOUNT";
}