using QT.Common.Filter;

namespace QT.JZRC.Entitys.Dto.JzrcStoreroom;

/// <summary>
/// 档案室管理列表查询输入
/// </summary>
public class JzrcStoreroomListQueryInput : PageInputBase
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
    /// 名称.
    /// </summary>
    public string name { get; set; }

}