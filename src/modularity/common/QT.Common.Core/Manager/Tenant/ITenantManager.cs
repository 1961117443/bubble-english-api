using QT.Common.Dtos.OAuth;
using QT.Extras.DatabaseAccessor.SqlSugar;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Common.Core.Manager.Tenant;

/// <summary>
/// 租户管理
/// </summary>
public interface ITenantManager
{
    /// <summary>
    /// 是否设置了租户号
    /// </summary>
    bool IsLoggedIn { get; }

    /// <summary>
    /// 租户编号
    /// </summary>
    public string TenantId { get; }

    ///// <summary>
    ///// 租户登录
    ///// </summary>
    ///// <param name="tenantId"></param>
    //bool Login(string tenantId,ISqlSugarClient db);

    /// <summary>
    /// 获取租户配置信息
    /// </summary>
    /// <returns></returns>
    public ITenantDbInfo GetTenantInfo();
}
