using QT.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace QT.Common.Const;

/// <summary>
/// 公共常量.
/// </summary>
[SuppressSniffer]
public class CommonConst
{
    // 不带自定义转换器
    public static JsonSerializerSettings options => new JsonSerializerSettings
    {
        // 默认命名规则
        ContractResolver = new DefaultContractResolver(),

        // 设置时区为 UTC
        DateTimeZoneHandling = DateTimeZoneHandling.Utc,

        // 格式化json输出的日期格式
        DateFormatString = "yyyy-MM-dd HH:mm:ss",
    };

    /// <summary>
    /// 默认密码.
    /// </summary>
    public const string DEFAULTPASSWORD = "123456";

    /// <summary>
    /// 雪花ID机器码缓存.
    /// </summary>
    public const string CACHEKEYWORKERID = "workerid_";

    /// <summary>
    /// 雪花ID缓存.
    /// </summary>
    public const string CACHEKEYSNOWFLAKEID = "snowflake_index";

    /// <summary>
    /// 用户缓存.
    /// </summary>
    public const string CACHEKEYUSER = "user_";

    /// <summary>
    /// 菜单缓存.
    /// </summary>
    public const string CACHEKEYMENU = "menu_";

    /// <summary>
    /// 权限缓存.
    /// </summary>
    public const string CACHEKEYPERMISSION = "permission_";

    /// <summary>
    /// 数据范围缓存.
    /// </summary>
    public const string CACHEKEYDATASCOPE = "datascope_";

    /// <summary>
    /// 验证码缓存.
    /// </summary>
    public const string CACHEKEYCODE = "vercode_";

    /// <summary>
    /// 单据编码缓存.
    /// </summary>
    public const string CACHEKEYBILLRULE = "billrule_";

    /// <summary>
    /// 在线用户缓存.
    /// </summary>
    public const string CACHEKEYONLINEUSER = "onlineuser_";

    /// <summary>
    /// 岗位缓存.
    /// </summary>
    public const string CACHEKEYPOSITION = "position_";

    /// <summary>
    /// 角色缓存.
    /// </summary>
    public const string CACHEKEYROLE = "role_";

    /// <summary>
    /// 在线开发缓存.
    /// </summary>
    public const string VISUALDEV = "visualdev_";

    /// <summary>
    /// 定时任务缓存.
    /// </summary>
    public const string CACHEKEYTIMERJOB = "timerjob_";


    /// <summary>
    /// 超级管理员的账号
    /// </summary>
    public const string SUPPER_ADMIN_ACCOUNT = "sysadmin";

    /// <summary>
    /// 超级管理员的id
    /// </summary>
    public const string SUPPER_ADMIN_ID = "admin";

    /// <summary>
    /// 当前登录用户禁止更新数据库的数据
    /// </summary>
    public const string LoginUserDisableChangeDatabase = "LoginUserDisableChangeDatabase";
}