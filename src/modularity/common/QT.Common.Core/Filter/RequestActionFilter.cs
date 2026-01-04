using System.Diagnostics;
using System.Security.Claims;
using QT.Common.Const;
using QT.Common.Extension;
using QT.Common.Net;
using QT.Common.Core.Security;
using QT.DataEncryption;
using QT.EventBus;
using QT.EventHandler;
using QT.Logging.Attributes;
using QT.Systems.Entitys.System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QT.Common.Security;

namespace QT.Common.Core.Filter;

/// <summary>
/// 请求日志拦截.
/// </summary>
public class RequestActionFilter : IAsyncActionFilter
{
    private readonly IEventPublisher _eventPublisher;

    public RequestActionFilter(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// 请求日记写入.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userContext = App.User;
        var httpContext = context.HttpContext;
        var httpRequest = httpContext.Request;

        Stopwatch sw = new Stopwatch();
        sw.Start();
        var sp1 = Stopwatch.StartNew();

        // 执行前先记录入参
        var args = context.ActionArguments?.ToJsonString();
        var actionContext = await next();
        sw.Stop();
        // 判断是否请求成功（没有异常就是请求成功）
        var isRequestSucceed = actionContext.Exception == null;
        var headers = httpRequest.Headers;
        if (!context.ActionDescriptor.EndpointMetadata.Any(m => m.GetType() == typeof(IgnoreLogAttribute)))
        {
            var tenantId = userContext?.FindFirstValue(ClaimConst.TENANTID);
            var tenantDbName = userContext?.FindFirstValue(ClaimConst.TENANTDBNAME);
            var userId = userContext?.FindFirstValue(ClaimConst.CLAINMUSERID);
            var userName = userContext?.FindFirstValue(ClaimConst.CLAINMREALNAME);

            if (!App.HttpContext.Request.Headers.ContainsKey("Authorization"))
            {
                string token = App.HttpContext.Request.QueryString.Value.Matches(@"[?&]token=Bearer%20([\w\.-]+)($|&)").LastOrDefault();
                if (!string.IsNullOrEmpty(token))
                {
                    IEnumerable<Claim> claims = JWTEncryption.ReadJwtToken(token.Replace("Bearer ", string.Empty).Replace("bearer ", string.Empty))?.Claims;
                    tenantId = claims.FirstOrDefault(e => e.Type == ClaimConst.TENANTID)?.Value;
                    tenantDbName = claims.FirstOrDefault(e => e.Type == ClaimConst.TENANTDBNAME)?.Value;
                    userId = claims.FirstOrDefault(e => e.Type == ClaimConst.CLAINMUSERID)?.Value;
                    userName = claims.FirstOrDefault(e => e.Type == ClaimConst.CLAINMREALNAME)?.Value;
                }
                else
                {
                    if (App.HttpContext.Items.ContainsKey(ClaimConst.TENANTID))
                    {
                        tenantId = App.HttpContext.Items[ClaimConst.TENANTID].ToString();
                        tenantDbName = tenantId;
                        userId = tenantId;
                        userName = tenantId;
                    }
                }
            }

            await _eventPublisher.PublishAsync(new LogEventSource("Log:CreateReLog", tenantId, tenantDbName, new SysLogEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                UserId = userId,
                UserName = userName,
                Category = 5,
                IPAddress = NetHelper.Ip,
                RequestURL = httpRequest.Path,
                RequestDuration = (int)sw.ElapsedMilliseconds,
                RequestMethod = httpRequest.Method,
                PlatForm = string.Format("{0}-{1}", UserAgent.GetSystem(), UserAgent.GetBrowser()),
                CreatorTime = DateTime.Now,
                Json = args
            }));

            if (context.ActionDescriptor.EndpointMetadata.Any(m => m.GetType() == typeof(OperateLogAttribute)))
            {
                // 操作参数
                //var args = context.ActionArguments.ToJsonString();
                var result = (actionContext.Result as JsonResult)?.Value;
                var module = context.ActionDescriptor.EndpointMetadata.Where(x => x.GetType() == typeof(OperateLogAttribute)).ToList().FirstOrDefault() as OperateLogAttribute;

                await _eventPublisher.PublishAsync(new LogEventSource("Log:CreateOpLog", tenantId, tenantDbName, new SysLogEntity
                {
                    Id = SnowflakeIdHelper.NextId(),
                    UserId = userId,
                    UserName = userName,
                    Category = 3,
                    IPAddress = NetHelper.Ip,
                    RequestURL = httpRequest.Path,
                    RequestDuration = (int)sw.ElapsedMilliseconds,
                    RequestMethod = module.Action,
                    PlatForm = string.Format("{0}-{1}", UserAgent.GetSystem(), UserAgent.GetBrowser()),
                    CreatorTime = DateTime.Now,
                    ModuleName = module.ModuleName,
                    Json = string.Format("{0}应用【{1}】【{2}】", module.Action, args, result.ToJsonString())
                }));
            }
        }
        sp1.Stop();
        context.HttpContext.Response.Headers.Add("x-elapsed", sp1.ElapsedMilliseconds.ToString());

        context.HttpContext.Response.Headers.Add("x-worker-id", SnowflakeIdHelper.CurrentWorkerId());
    }
}