using QT.DependencyInjection;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Common.Dtos.OAuth;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class TenantScopeModel: TenantInterFaceOutput
{
    /// <summary>
    /// 租户编码
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 是否自动登录，根据域名判断
    /// </summary>
    public bool AutoLogin { get; set; }
}
