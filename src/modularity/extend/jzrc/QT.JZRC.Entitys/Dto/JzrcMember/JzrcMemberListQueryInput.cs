using QT.Common.Filter;

namespace QT.JZRC.Entitys.Dto.JzrcMember;

/// <summary>
/// 建筑平台会员信息列表查询输入
/// </summary>
public class JzrcMemberListQueryInput : PageInputBase
{
    /// <summary>
    /// 选择导出数据key.
    /// </summary>
    public string selectKey { get; set; }

    /// <summary>
    /// 导出类型.
    /// </summary>
    public int dataType { get; set; }

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
    public string role { get; set; }

}