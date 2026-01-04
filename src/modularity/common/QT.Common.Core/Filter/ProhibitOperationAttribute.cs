using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using QT.Common.Const;
using QT.Common.Core.Configs;
using QT.Common.Core.Service;
using QT.Common.Extension;
using QT.Common.Security;
using QT.FriendlyException;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QT.Common.Core.Filter;

/// <summary>
/// 接口层面的操作拦截
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class ProhibitOperationAttribute : Attribute
{
    public ProhibitOperationEnum Operation { get; set; }

    public ProhibitOperationAttribute(ProhibitOperationEnum prohibitOperation)
    {
        this.Operation = prohibitOperation;
    }

    public ProhibitOperationAttribute():this(ProhibitOperationEnum.Forbid)
    {
    }
}

public enum ProhibitOperationEnum
{
    /// <summary>
    /// 禁止操作
    /// </summary>
    Forbid,

    /// <summary>
    /// 允许操作
    /// </summary>
    Allow
}


/// <summary>
/// 禁止操作拦截
/// </summary>
public class ProhibitOperationFilter : IAsyncActionFilter
{
    private ICoreSysConfigService _coreSysConfigService;

    public ProhibitOperationFilter(/*ICoreSysConfigService coreSysConfigService*/)
    {
        //_coreSysConfigService = coreSysConfigService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        try
        {
            _coreSysConfigService = context.HttpContext.RequestServices.GetService<ICoreSysConfigService>();
            // 获取动作方法描述器
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            var method = actionDescriptor.MethodInfo;

            // 判断是否贴有工作单元特性
            if (!await IsDefined(context))
            {
                // 如果是只读用户，判断是否为白名单接口
                if (context.HttpContext.Items.TryGetValue(CommonConst.LoginUserDisableChangeDatabase, out var value) && value != null && Boolean.TryParse(value.ToString(), out var b) && b)
                {
                    if (await IsWhiteList(context))
                    {
                        context.HttpContext.Items.Remove(CommonConst.LoginUserDisableChangeDatabase);
                    }
                }
                    
                // 调用方法
                _ = await next();
            }
            else
            {
                // 判断是否为禁止更新的角色
                if (context.HttpContext.Items.TryGetValue(CommonConst.LoginUserDisableChangeDatabase, out var value) && value != null && Boolean.TryParse(value.ToString(), out var b) && b)
                {
                    var attr = method.GetCustomAttribute<ProhibitOperationAttribute>() ?? actionDescriptor.ControllerTypeInfo.GetCustomAttribute<ProhibitOperationAttribute>();

                    switch (attr.Operation)
                    {
                        case ProhibitOperationEnum.Forbid:
                            throw Oops.Oh("当前用户没有权限进行操作！");
                        case ProhibitOperationEnum.Allow:
                            context.HttpContext.Items.Remove(CommonConst.LoginUserDisableChangeDatabase);
                            break;
                    }
                }
                await next();

            }
        }
        catch (Exception ex)
        {
            Log.Error(ex.StackTrace);
            throw;
        }
    }

    /// <summary>
    /// 判断是否包含ProhibitOperationAttribute特性
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<bool> IsDefined(ActionExecutingContext context)
    {
        // 获取动作方法描述器
        var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
        var method = actionDescriptor.MethodInfo; 

        var flag = method.IsDefined(typeof(ProhibitOperationAttribute), true) || actionDescriptor.ControllerTypeInfo.IsDefined(typeof(ProhibitOperationAttribute), true);

        return flag;
    }

    /// <summary>
    /// 判断是否为白名单
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<bool> IsWhiteList(ActionExecutingContext context)
    {
        // 获取动作方法描述器
        var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
        var method = actionDescriptor.MethodInfo;
        ProhibitOperationWhiteItem prohibitOperationWhiteItem = new ProhibitOperationWhiteItem
        {
            ControllerName = actionDescriptor.ControllerName ?? "",
            ActionName = actionDescriptor.ActionName ?? "",
            ClassName = actionDescriptor.ControllerTypeInfo?.Name ?? "",
            MethodName = method?.Name ?? "",
            HttpMethod = context.HttpContext?.Request?.Method ?? "",
        };
        var configs = await _coreSysConfigService.GetConfig<ReadonlyRoleConfigs>();

        if (configs != null && configs.readonlyRoleWhiteList.IsNotEmptyOrNull())
        {
            var whiteList = configs.readonlyRoleWhiteList.ToObject<List<ProhibitOperationWhiteItem>>();

            // 1、如果只配置了Controller且只有1条，默认整个控制器为白名单
            // 2、优先级： 1、controller+action+httpmethod
            //2、controller+action

            if (whiteList.IsAny())
            {
                whiteList = whiteList.Where(x => x.ControllerName == prohibitOperationWhiteItem.ControllerName || x.ClassName == prohibitOperationWhiteItem.ClassName)
                    .Where(x => x.GetScore(prohibitOperationWhiteItem) > -1)
                    .OrderByDescending(x => x.GetScore(prohibitOperationWhiteItem))
                    .ToList();

                if (whiteList.IsAny())
                {
                    if (whiteList.Count == 1)
                    {
                        return true;
                    }
                    // 判断是否有符合的条件
                    if (whiteList.Any(x=>x.GetScore(prohibitOperationWhiteItem)>0))
                    {
                        return true;
                    }

                }
            }
        }

        return false;
    }
}


public class ProhibitOperationWhiteItem
{
    public string HttpMethod { get; set; }

    public string ControllerName { get; set; }
    public string ClassName { get; set; }

    public string ActionName { get; set; }

    public string MethodName { get; set; }


    public int GetScore(ProhibitOperationWhiteItem target)
    {
        int score = -1;
        if (target.ControllerName.Equals( ControllerName, StringComparison.OrdinalIgnoreCase) 
            || target.ClassName.Equals(ClassName, StringComparison.OrdinalIgnoreCase))
        {
            score = 0;

            if (target.ActionName.Equals(ActionName, StringComparison.OrdinalIgnoreCase)
            || target.MethodName.Equals(MethodName, StringComparison.OrdinalIgnoreCase))
            {
                score += 1000;

                if (HttpMethod.IsNotEmptyOrNull())
                {
                    if (target.HttpMethod.Equals(HttpMethod, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 10000;
                    }
                    else
                    {
                        score -= 1000;
                    }
                }
               
            }

           
        }

        return score;
    }
}