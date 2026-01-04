using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace QT.CMS.Entitys.Dto.Login;

public class CurrentUserLoginInfo
{
    /// <summary>
    /// 用户信息
    /// </summary>
    public UserInfo userInfo { get; set; }
    /// <summary>
    /// 用户拥有的权限
    /// </summary>
    public List<LoginMenuOutput> menuList { get; set; } = new List<LoginMenuOutput>();

    /// <summary>
    /// 用户权限
    /// </summary>
    public LoginUserAuth permission { get; set; }
}

public enum LoginMenuEnum
{
    //Default,
    /// <summary>
    /// 首页菜单
    /// </summary>
    Home,
    /// <summary>
    /// 个人中心菜单
    /// </summary>
    Account
}

public class UserInfo
{
    /// <summary>
    /// 主键
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    public string realName { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    public string account { get; set; }

    ///// <summary>
    ///// 角色
    ///// </summary>

    //public AppLoginUserRole role { get; set; }

    /// <summary>
    /// 头像地址
    /// </summary>
    public string avatar { get; set; }
}

public class LoginMenuOutput
{
    /// <summary>
    /// 页面路由地址
    /// </summary>
    public string path { get; set; }
    /// <summary>
    /// 组件地址
    /// 默认带上了 @/views/
    /// </summary>
    public string component { get; set; }

    ///// <summary>
    ///// 路由权限（求职者或者企业）如果是公共的为空
    ///// </summary>
    //public AppLoginUserRole? role { get; set; }

    public LoginMenuEnum category { get; set; }

    /// <summary>
    /// 是否隐藏
    /// </summary>
    public bool hidden { get; set; }

    /// <summary>
    /// 页面名称
    /// </summary>
    public string title { get; set; }
}


/// <summary>
/// 用户权限
/// </summary>
[Flags]
public enum LoginUserAuth
{
    /// <summary>
    /// 普通用户
    /// </summary>
    Common = 1,

    /// <summary>
    /// 招聘人才
    /// </summary>
    Recruit = 2,

    /// <summary>
    /// 申请职位
    /// </summary>
    Post = 4
}