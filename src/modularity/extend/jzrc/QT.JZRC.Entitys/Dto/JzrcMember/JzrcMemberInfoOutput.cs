using QT.Common.Models;
using QT.JZRC.Entitys.Dto.JzrcMemberAmountLog;

namespace QT.JZRC.Entitys.Dto.JzrcMember;

/// <summary>
/// 建筑平台会员信息输出参数.
/// </summary>
public class JzrcMemberInfoOutput
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
    /// 头像.
    /// </summary>
    public string headIcon { get; set; }

    /// <summary>
    /// 账号类型（0：人才，1：企业）.
    /// </summary>
    public string role { get; set; }

    /// <summary>
    /// 会员余额记录.
    /// </summary>
    public List<JzrcMemberAmountLogInfoOutput> jzrcMemberAmountLogList { get; set; }

    /// <summary>
    /// 关联账号
    /// </summary>
    public string relationId { get; set; }

    /// <summary>
    /// 账户余额
    /// </summary>
    public decimal amount { get; set; }

}