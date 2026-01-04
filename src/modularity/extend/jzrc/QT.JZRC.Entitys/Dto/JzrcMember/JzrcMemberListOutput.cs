using QT.JZRC.Entitys.Dto.AppService;

namespace QT.JZRC.Entitys.Dto.JzrcMember;

/// <summary>
/// 建筑平台会员信息输入参数.
/// </summary>
public class JzrcMemberListOutput
{
    /// <summary>
    /// 自然主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 账户（手机号码）.
    /// </summary>
    public string account { get; set; }

    /// <summary>
    /// 呢称.
    /// </summary>
    public string nickName { get; set; }

    /// <summary>
    /// 账号类型（0：人才，1：企业）.
    /// </summary>
    public AppLoginUserRole role { get; set; }

    /// <summary>
    /// 关联信息
    /// </summary>
    public string relationId { get; set; }

}