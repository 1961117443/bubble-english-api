using QT.Common.Filter;

namespace QT.Logistics.Entitys.Dto.LogMember;

/// <summary>
/// 会员信息列表查询输入
/// </summary>
public class LogMemberListQueryInput : PageInputBase
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
    /// 姓名.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 身份证号.
    /// </summary>
    public string cardNumber { get; set; }

    /// <summary>
    /// 手机号码.
    /// </summary>
    public string phoneNumber { get; set; }


    /// <summary>
    /// 数据范围(scope=point 配送点)
    /// </summary>
    public string scope { get; set; }

}