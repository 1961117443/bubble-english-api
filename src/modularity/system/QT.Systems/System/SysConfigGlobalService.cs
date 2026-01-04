using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QT.Common.Dtos.OAuth;
using QT.Common.Options;
using QT.DataEncryption;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.JsonSerialization;
using QT.Logging.Attributes;
using QT.Systems.Entitys.Dto.System.SysConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Systems.System;

/// <summary>
/// 
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "SysConfig", Order = 211)]
[Route("api/system/[controller]")]
public class SysConfigGlobalService: IDynamicApiController
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private TenantOptions _tenantOptions;

    /// <summary>
    /// 尽量少引用外部类，特别是数据访问层
    /// </summary>
    /// <param name="tenantOptions"></param>
    public SysConfigGlobalService(IOptions<TenantOptions> tenantOptions, IHttpContextAccessor httpContextAccessor)
    {
        _tenantOptions = tenantOptions.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 获取全局系统配置
    /// </summary>
    /// <returns></returns>
    [HttpGet("global")]
    [AllowAnonymous,IgnoreLog]
    public SysConfigGlobalOutput GetGlobalConfig()
    {
        SysConfigGlobalOutput output = new SysConfigGlobalOutput()
        {
            multiTenancy = _tenantOptions.MultiTenancy,
        };

        // 多租户才需要判断
        if (output.multiTenancy)
        {
            var str = _httpContextAccessor?.HttpContext?.Request.Headers["x-saas-token"];

            if (!string.IsNullOrEmpty(str))
            {
                var decrypt = DESCEncryption.Decrypt(str, "QT");

                var model = JSON.Deserialize<TenantScopeModel>(decrypt);

                output.autoLogin = model.AutoLogin;
            }
        }
        else
        {
            output.autoLogin = true;
        }      

        return output;
    }
}
