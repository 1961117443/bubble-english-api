using QT.Common.Models;
using QT.JZRC.Entitys.Dto.JzrcMemberAmountLog;
namespace QT.JZRC.Entitys.Dto.JzrcMember;

/// <summary>
/// 建筑平台会员信息更新输入.
/// </summary>
public class JzrcMemberUpInput
{
    /// <summary>
    /// 自然主键.
    /// </summary>
    public string id { get; set; }

    ///// <summary>
    ///// 呢称.
    ///// </summary>
    //public string nickName { get; set; }

    /// <summary>
    /// 头像.
    /// </summary>
    public string headIcon { get; set; }

    /// <summary>
    /// 关联账号
    /// </summary>
    public string relationId { get; set; }
}