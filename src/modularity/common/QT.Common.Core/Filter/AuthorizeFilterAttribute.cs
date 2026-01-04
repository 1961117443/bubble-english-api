using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using QT.Common.Core.Emum;
using QT.FriendlyException;
using System.Security.Claims;

namespace QT.Common.Core.Filters
{
    /// <summary>
    /// 验证授权过滤器
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizeFilterAttribute : Attribute, IAsyncAuthorizationFilter
    {
        /// <summary>
        /// 模块名
        /// </summary>
        public string Module { get; }
        /// <summary>
        /// 操作名
        /// </summary>
        public ActionType Method { get; }
        /// <summary>
        /// 参数名
        /// </summary>
        public string? Param { get; }

        public AuthorizeFilterAttribute(string module, ActionType method, string? param = null)
        {
            Module = module;
            Method = method;
            Param = param;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            //检测是否有路由参数
            //有路由形式为Module@RouteData.Method组合成权限码
            //无路由形式为Module.Method组合成权限码
            string Permission = $"{Module}.{Method}";
            if (!string.IsNullOrWhiteSpace(Param))
            {
                var routeData = context.RouteData.Values[Param];
                if (routeData != null)
                {
                    Permission = $"{Module}@{routeData}.{Method}";
                }
                else
                {
                    Permission = $"{Module}@{Param}.{Method}";
                }
            }

            //获取当前已登录的用户
            ClaimsPrincipal claimsPrincipal = context.HttpContext.User;
            //检查用户是否已通过身份验证
            if (claimsPrincipal.Identity != null && !claimsPrincipal.Identity.IsAuthenticated)
            {
                HandlerAuthenticationFailed(context, "认证失败，请检查请求头或者重新登陆", ErrorCode.AuthenticationFailed);
                return;
            }
            //注意判断及检查权限是否满足在PermissionAuthorizationHandler里面
            IAuthorizationService? authorizationService = context.HttpContext.RequestServices.GetService(typeof(IAuthorizationService)) as IAuthorizationService;
            if (authorizationService == null)
            {
                HandlerAuthenticationFailed(context, "认证失败，无法获取授权服务", ErrorCode.AuthenticationFailed);
                return;
            }
            //检查用户是否满足指定资源的特定要求
            AuthorizationResult authorizationResult = await authorizationService.AuthorizeAsync(context.HttpContext.User, null, new OperationAuthorizationRequirement() { Name = Permission });
            //不满足策略时提示错误消息
            if (!authorizationResult.Succeeded)
            {
                //通过报业务异常，统一返回结果，平均执行速度在500ms以上，直接返回无权限，则除第一次访问慢外，基本在80ms左右
                HandlerAuthenticationFailed(context, $"您没有权限：{Module}-{Method}", ErrorCode.NoPermission);
            }
        }

        /// <summary>
        /// 错误码的响应消息
        /// </summary>
        /// <param name="context">过滤器上下文</param>
        /// <param name="errorMsg">错误消息</param>
        /// <param name="errorCode">错误代码</param>
        public void HandlerAuthenticationFailed(AuthorizationFilterContext context, string errorMsg, ErrorCode errorCode)
        {
            ////响应状态码
            //context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            ////统一格式消息
            //context.Result = new JsonResult(new RESTfulResult<object>
            //{
            //    code = (int)errorCode,
            //    msg = errorMsg,
            //    extras = UnifyContext.Take(),
            //    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            //});

            throw Oops.Bah(errorMsg).StatusCode(StatusCodes.Status401Unauthorized);
        }
    }
}
