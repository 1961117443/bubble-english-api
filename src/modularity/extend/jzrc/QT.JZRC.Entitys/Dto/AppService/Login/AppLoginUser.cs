namespace QT.JZRC.Entitys.Dto.AppService;

/// <summary>
/// 建筑人才： 用户登录信息
/// </summary>
public class AppLoginUser
{
    /// <summary>
    /// 主键
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    public string RealName { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    public string Account { get; set; }

    /// <summary>
    /// 角色
    /// </summary>

    public AppLoginUserRole Role { get; set; }

      
}


public enum AppLoginUserRole
{
    /// <summary>
    /// 人才
    /// </summary>
    Talent,
    /// <summary>
    /// 企业
    /// </summary>
    Company
}
