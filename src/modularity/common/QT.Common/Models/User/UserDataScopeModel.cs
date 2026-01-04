using QT.DependencyInjection;

namespace QT.Common.Models.User;

/// <summary>
/// 用户数据范围集合.
/// </summary>
[SuppressSniffer]
public class UserDataScopeModel
{
    /// <summary>
    /// 机构ID.
    /// </summary>
    public string organizeId { get; set; }

    /// <summary>
    /// 新增.
    /// </summary>
    public bool Add { get; set; }

    /// <summary>
    /// 编辑.
    /// </summary>
    public bool Edit { get; set; }

    /// <summary>
    /// 删除.
    /// </summary>
    public bool Delete { get; set; }
}