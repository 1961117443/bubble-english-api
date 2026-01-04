using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using QT.API.Core.Handlers;
using QT.Common.Cache;
using QT.Common.Configuration;
using QT.Common.Const;
using QT.Common.Core.Manager.Tenant;
using QT.Common.Dtos.OAuth;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Security;
using QT.DataEncryption;
using QT.DependencyInjection;
using QT.DistributedIDGenerator;
using QT.Extras.DatabaseAccessor.SqlSugar;
using QT.FriendlyException;
using QT.JsonSerialization;
using QT.RemoteRequest.Extensions;
using QT.Systems.Entitys.System;
using QT.UnifyResult;
using SqlSugar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.API.Core.Extensions;

public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenant(this IApplicationBuilder builder)
    {
        if (KeyVariable.MultiTenancy)
        {
            builder.UseMiddleware<TenantMiddleware>();
            builder.UseMiddleware<TenantOssMiddleware>();
        }
        return builder;
    }
}


public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _memoryCache;

    public TenantMiddleware(RequestDelegate next, IMemoryCache memoryCache)
    {
        _next = next;
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// 判断是否要检查token
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private bool CheckSaasToken(HttpContext context)
    {
        if (context.Request.Method == "OPTIONS")
        {
            return false;
        }

        if (context.Request.Headers.ContainsKey("Upgrade") && context.Request.Headers["Upgrade"] == "websocket")
        {
            return false;
        }

        if (context.Request.Headers.ContainsKey("Sec-Fetch-Dest") && context.Request.Headers["Sec-Fetch-Dest"] == "image")
        {
            return false;
        }

        var saasToken = context.Request.Headers["x-saas-token"].ToString();

        return string.IsNullOrEmpty(saasToken);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 在调用下一个中间件之前可以执行一些操作
        var host = context.Request.Host.Host;
        //Console.WriteLine(host);

        string scopeTenantId = string.Empty;

        if (CheckSaasToken(context))
        {
            var tenantId = await _memoryCache.GetOrCreateAsync($"saas:host:{host}", async entry =>
            {
                var interFace = string.Format("{0}Host/{1}", KeyVariable.MultiTenancyDBInterFace, context.Request.Host.Host);
                try
                {
                    var response = await interFace.GetAsStringAsync();
                    var result = response.ToObject<RESTfulResult<string>>();
                    if (result.data.IsNotEmptyOrNull())
                    {
                        // 租户不存在，缓存一分钟
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                    }
                    else
                    {
                        //entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
                        entry.SlidingExpiration = TimeSpan.FromMinutes(1);
                    }
                    
                    return result.data;
                }
                catch (Exception)
                {
                }
                return string.Empty;

                //Dictionary<string, string> temp = new Dictionary<string, string>();
                //temp.Add("wx.erp.95033.cn", "1001");
                //temp.Add("localhost", "2001");
                //entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                //return await Task.FromResult(temp.ContainsKey(host) ? temp[host] : "");
            });


            if (string.IsNullOrEmpty(tenantId))
            {
                var token = context.Request.Query["x-saas-token"].ToString();
                if (!string.IsNullOrEmpty(token))
                {
                    tenantId = token;
                }
            }


            if (!string.IsNullOrEmpty(tenantId))
            {
                context.Items[ClaimConst.TENANTID] = tenantId;
                TenantScopeModel tenantScopeModel = new TenantScopeModel
                {
                    Code = tenantId,
                    AutoLogin = true
                };

                // 加密对象
                var encrypt = DESCEncryption.Encrypt(JSON.Serialize(tenantScopeModel), "QT");
                // 写入响应头，这里的登录要同时返回租户id
                context.Response.Headers.Add("x-saas-token", $"{encrypt}|{tenantId}");
                // 写入请求头
                context.Request.Headers.Add("x-saas-token", encrypt);
            }

            scopeTenantId = tenantId;
        }

       
        if (string.IsNullOrEmpty(scopeTenantId) && context.Request.Headers.TryGetValue("x-saas-token", out var saastoken))
        {
            if (!string.IsNullOrEmpty(saastoken))
            {
                var tenantList = _memoryCache.Get<List<ITenantDbInfo>>("tenant:dbcontext:list");
                if (tenantList.IsAny() && tenantList.Any(x=>x.enCode == saastoken))
                {
                    scopeTenantId = saastoken;
                }
                else
                {
                    try
                    {
                        var decrypt = DESCEncryption.Decrypt(saastoken, "QT");
                        var model = JSON.Deserialize<TenantScopeModel>(decrypt);
                        scopeTenantId = model?.Code;
                    }
                    catch (Exception)
                    {
                        throw Oops.Bah("x-saas-token解析失败");
                    }
                }
            }
        }

        // 如果用户登录了，判断租户是否异常
        if (App.User.HasClaim(it => it.Type == ClaimConst.TENANTID))
        {
            var userTenantId = App.User?.FindFirst(ClaimConst.TENANTID)?.Value ?? "";

            if (string.IsNullOrEmpty(scopeTenantId))
            {
                scopeTenantId = userTenantId;
               
            }
            else
            {
                if (userTenantId != scopeTenantId)
                {
                    // 如果允许匿名登录，可以直接通过
                    var AllowAnonymous = context.GetMetadata<AllowAnonymousAttribute>();
                    if (AllowAnonymous == null)
                    {
                        // 如果用户登录了，但是租户id不一致，说明用户登录的租户异常
                        //throw Oops.Bah("当前用户登录的租户异常，请重新登录");
                        context.Response.Headers.Remove("x-saas-token");
                        // 清除缓存
                        _memoryCache.Remove($"saas:host:{host}");
                        var result = new RESTfulResult<object>
                        {
                            code = 610001,
                            data = null,
                            msg = "当前用户登录的租户异常，请重新登录",
                            extras = UnifyContext.Take(),
                            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                        };
                        await context.Response.WriteAsJsonAsync(result, App.GetOptions<JsonOptions>()?.JsonSerializerOptions);
                        return;
                    }
                    //if ()
                    //{

                    //}
                }
            }
        }
        //if (string.IsNullOrEmpty(scopeTenantId) && App.User.HasClaim(it => it.Type == ClaimConst.TENANTID))
        //{
        //    scopeTenantId = App.User?.FindFirst(ClaimConst.TENANTID)?.Value ?? "";
        //}


        // 租户登录
        if (!string.IsNullOrEmpty(scopeTenantId))
        {
            try
            {
                context.Response.Headers.Add("Scope-Tenant-Id", scopeTenantId);
                TenantScoped.LoginScoped(scopeTenantId, context.RequestServices);
            }
            catch (AppFriendlyException ex)
            {
                context.Response.Headers.Remove("x-saas-token");
                // 清除缓存
                _memoryCache.Remove($"saas:host:{host}");
                var result = new RESTfulResult<object>
                {
                    code = ex.StatusCode,
                    data = null,
                    msg = ex.ErrorMessage,
                    extras = UnifyContext.Take(),
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };
                await context.Response.WriteAsJsonAsync(result, App.GetOptions<JsonOptions>()?.JsonSerializerOptions);
                return;
            }
        }

        // 调用下一个中间件
        await _next(context);

        // 在调用下一个中间件之后可以执行一些操作
        // ...
    }
}