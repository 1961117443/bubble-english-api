using QT.JZRC.Entitys.Dto.AppService;
using QT.JZRC.Entitys.Dto.AppService.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JZRC.Interfaces;

/// <summary>
/// 登录服务
/// </summary>
public interface IJzrcAppLogin
{
    ///// <summary>
    ///// 用户登录
    ///// </summary>
    ///// <param name="loginInput"></param>
    ///// <returns></returns>
    //Task<AppLoginUser> Login(LoginDto loginInput);

    ///// <summary>
    ///// 根据account获取信息
    ///// </summary>
    ///// <param name="id"></param>
    ///// <returns></returns>
    //Task<AppLoginUser?> GetByAccount(string account);

    /// <summary>
    /// 不存在创建
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<AppLoginUser?> GetOrCreateAsync(AppLoginCrInput input);
}
