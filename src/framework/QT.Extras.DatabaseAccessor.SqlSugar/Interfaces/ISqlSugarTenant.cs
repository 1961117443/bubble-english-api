using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSugar;

/// <summary>
/// sqlsugar租户.
/// </summary>
public interface ISqlSugarTenant
{
    /// <summary>
    /// 租户是否登录成功
    /// </summary>
    bool IsLoggedIn { get; }

    /// <summary>
    /// 创建链接
    /// </summary>
    /// <returns></returns>
    ConnectionConfig CreateConnection();
}
