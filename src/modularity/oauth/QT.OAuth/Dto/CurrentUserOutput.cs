using QT.Common.Models.User;
using QT.DependencyInjection;
using QT.Systems.Entitys.Dto.Module;

namespace QT.OAuth.Dto;

/// <summary>
/// 当前客户信息输出.
/// </summary>
[SuppressSniffer]
public class CurrentUserOutput
{
    /// <summary>
    /// 用户信息.
    /// </summary>
    public UserInfoModel userInfo { get; set; }

    /// <summary>
    /// 菜单列表.
    /// </summary>
    public List<ModuleNodeOutput> menuList { get; set; }

    /// <summary>
    /// 权限列表.
    /// </summary>
    public List<PermissionModel> permissionList { get; set; }

    /// <summary>
    /// 系统配置信息.
    /// </summary>
    public SysConfigInfo sysConfigInfo { get; set; }
}

/// <summary>
/// 权限.
/// </summary>
[SuppressSniffer]
public class PermissionModel
{
    /// <summary>
    /// 模块ID.
    /// </summary>
    public string modelId { get; set; }

    /// <summary>
    /// 模块名称.
    /// </summary>
    public string moduleName { get; set; }

    /// <summary>
    /// 模块编码.
    /// </summary>
    public string moduleCode { get; set; }

    /// <summary>
    /// 列.
    /// </summary>
    public List<FunctionalColumnAuthorizeModel> column { get; set; }

    /// <summary>
    /// 按钮.
    /// </summary>
    public List<FunctionalButtonAuthorizeModel> button { get; set; }

    /// <summary>
    /// 表单.
    /// </summary>
    public List<FunctionalFormAuthorizeModel> form { get; set; }

    /// <summary>
    /// 资源.
    /// </summary>
    public List<FunctionalResourceAuthorizeModel> resource { get; set; }
}

/// <summary>
/// 功能权限基类.
/// </summary>
[SuppressSniffer]
public class FunctionalAuthorizeBase
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    public string fullName { get; set; }

    /// <summary>
    /// 编码.
    /// </summary>
    public string enCode { get; set; }
}

/// <summary>
/// 功能权限列.
/// </summary>
[SuppressSniffer]
public class FunctionalColumnAuthorizeModel : FunctionalAuthorizeBase
{
}

/// <summary>
/// 功能权限按钮.
/// </summary>
[SuppressSniffer]
public class FunctionalButtonAuthorizeModel : FunctionalAuthorizeBase
{
}

/// <summary>
/// 功能权限表单.
/// </summary>
[SuppressSniffer]
public class FunctionalFormAuthorizeModel : FunctionalAuthorizeBase
{
}

/// <summary>
/// 授权模块资源.
/// </summary>
[SuppressSniffer]
public class FunctionalResourceAuthorizeModel : FunctionalAuthorizeBase
{
}

/// <summary>
/// 系统配置信息.
/// </summary>
//[SuppressSniffer]
public class SysConfigInfo
{
    /// <summary>
    /// 系统名称.
    /// </summary>
    public string sysName { get; set; }

    /// <summary>
    /// 系统版本.
    /// </summary>
    public string sysVersion { get; set; }

    /// <summary>
    /// 登录图标.
    /// </summary>
    public string loginIcon { get; set; }

    /// <summary>
    /// 版权信息.
    /// </summary>
    public string copyright { get; set; }

    /// <summary>
    /// 公司名称.
    /// </summary>
    public string companyName { get; set; }

    /// <summary>
    /// 导航图标.
    /// </summary>
    public string navigationIcon { get; set; }

    /// <summary>
    /// logo图标.
    /// </summary>
    public string logoIcon { get; set; }

    /// <summary>
    /// App图标.
    /// </summary>
    public string appIcon { get; set; }
}