using Microsoft.AspNetCore.Http;
using NPOI.POIFS.Crypt.Dsig;
using QT.Common.Const;
using QT.Common.Models.User;
using QT.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Common.Core.Manager.Parameter;


/// <summary>
/// 参数管理类
/// </summary>
public class ParameterManager : IParameterManager, IScoped
{
    private readonly IUserManager _userManager;
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 系统参数
    /// </summary>
    private static Dictionary<string, Func<object>> _sysParameters = new();
    static ParameterManager()
    {
        _sysParameters.Add("@当前用户", () => App.GetService<IUserManager>()?.RealName ?? "-");
        _sysParameters.Add("@当前时间", () => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    public ParameterManager(IUserManager userManager,ICacheManager cacheManager )
    {
        _userManager = userManager;
        _cacheManager = cacheManager;
    }


    private UserInfoModel _userInfo;


    /// <summary>
    /// 获取参数值
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<object> GetParameterValue(string key)
    {
        if (_sysParameters.ContainsKey(key))
        {
            return _sysParameters[key]();
        }
        var userInfo = _userInfo ?? (await _cacheManager.GetAsync<UserInfoModel>(string.Format("{0}{1}_{2}", CommonConst.CACHEKEYUSER, _userManager.TenantId, _userManager.UserId)));
        //如果参数是@开头的话，先去掉进行判断
        key = key.TrimStart('@');
        // 1、先从请求的url中判断
        if (App.HttpContext != null)
        {
            var p = App.HttpContext.Request.Query.FirstOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (p.Key != null)
            {
                return p.Value.ToString();
            }

        }


        // 2、获取userinfo的值
        if (userInfo != null)
        {
            var p = EntityHelper<UserInfoModel>.InstanceProperties.FirstOrDefault(x => x.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (p != null)
            {
                return p.GetValue(userInfo, null);
            }
        }

        return null;
    }
}
