using System.Security.Claims;
using Serilog;
using QT.Common.Const;
using QT.Common.Extension;
using QT.Common.Net;
using QT.Common.Core.Security;
using QT.DataEncryption;
using QT.DependencyInjection;
using QT.EventBus;
using QT.EventHandler;
using QT.FriendlyException;
using QT.Logging.Attributes;
using Microsoft.AspNetCore.Mvc.Filters;
using QT.Systems.Entitys.System;
using QT.Common.Security;

namespace QT.Common.Core.Filter;

/// <summary>
/// 全局异常处理.
/// </summary>
public class LogExceptionHandler : IGlobalExceptionHandler, ISingleton
{
    private readonly IEventPublisher _eventPublisher;

    public LogExceptionHandler(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// 异步写入异常日记.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        Log.Error($"LogExceptionHandler:{context.Exception.StackTrace}");
        var userContext = App.User;
        var httpContext = context.HttpContext;
        var httpRequest = httpContext?.Request;
        var headers = httpRequest?.Headers;

        if (!context.ActionDescriptor.EndpointMetadata.Any(m => m.GetType() == typeof(IgnoreLogAttribute)))
        {
            string tenantId = userContext?.FindFirstValue(ClaimConst.TENANTID);
            string tenantDbName = userContext?.FindFirstValue(ClaimConst.TENANTDBNAME);
            string userId = userContext?.FindFirstValue(ClaimConst.CLAINMUSERID);
            string userName = userContext?.FindFirstValue(ClaimConst.CLAINMREALNAME);

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

            await _eventPublisher.PublishAsync(new LogEventSource("Log:CreateExLog", tenantId, tenantDbName, new SysLogEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                UserId = userId,
                UserName = userName,
                Category = 4,
                IPAddress = NetHelper.Ip,
                RequestURL = httpRequest.Path,
                RequestMethod = httpRequest.Method,
                Json = context.Exception.Message + "\n" + context.Exception.StackTrace + "\n" + context.Exception.TargetSite.GetParameters().ToString(),
                PlatForm = string.Format("{0}-{1}", UserAgent.GetSystem(), UserAgent.GetBrowser()),
                CreatorTime = DateTime.Now
            }));
        }

        // 写日志文件
        Log.Error(context.Exception.ToString());
    }
}