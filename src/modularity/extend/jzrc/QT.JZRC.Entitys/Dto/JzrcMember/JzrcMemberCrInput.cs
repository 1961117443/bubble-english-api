using QT.Common.Models;
using QT.JZRC.Entitys.Dto.JzrcMemberAmountLog;

namespace QT.JZRC.Entitys.Dto.JzrcMember;

/// <summary>
/// 建筑平台会员信息修改输入参数.
/// </summary>
public class JzrcMemberCrInput
{
    /// <summary>
    /// 账户（手机号码）.
    /// </summary>
    public string account { get; set; }

    /// <summary>
    /// 呢称.
    /// </summary>
    public string nickName { get; set; }

    /// <summary>
    /// 头像.
    /// </summary>
    public string headIcon { get; set; }

    /// <summary>
    /// 账号类型（0：人才，1：企业）.
    /// </summary>
    public string role { get; set; }

    /// <summary>
    /// 关联账号
    /// </summary>
    public string relationId { get; set; }
}