using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Common.Core.Manager.Tenant;

/// <summary>
/// 范围租户
/// </summary>
public interface ITenantScoped
{
    /// <summary>
    /// 当前租户id
    /// </summary>
    string TenantId { get; }

    /// <summary>
    /// 租户登录
    /// </summary>
    /// <param name="tenantId"></param>
    void Login(string tenantId);
}
