using QT.Common.Filter;
using QT.DependencyInjection;

namespace QT.SDMS.Entitys.Dto.UserMarket;

/// <summary>
/// 用户列表查询输入.
/// </summary>
[SuppressSniffer]
public class UserListQueryInput : PageInputBase
{
    /// <summary>
    /// 机构ID.
    /// </summary>
    public string organizeId { get; set; }

    /// <summary>
    /// 岗位ID.
    /// </summary>
    public string positionId { get; set; }

    ///// <summary>
    ///// 隐藏客户信息
    ///// </summary>
    //public bool hideCustomer { get; set; }

    ///// <summary>
    ///// 来源
    ///// </summary>
    //public int? origin { get; set; }

    /// <summary>
    /// 销售人员
    /// </summary>
    public string userId { get; set; }


    /// <summary>
    /// 姓名
    /// </summary>
    public string realName { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    public string mobilePhone { get; set; }
}