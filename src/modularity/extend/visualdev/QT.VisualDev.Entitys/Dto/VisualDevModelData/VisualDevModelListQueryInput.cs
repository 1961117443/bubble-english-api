using QT.Common.Filter;

namespace QT.VisualDev.Entitys.Dto.VisualDevModelData;

/// <summary>
/// 在线开发功能模块列表查询输入.
/// </summary>
public class VisualDevModelListQueryInput : PageInputBase
{
    /// <summary>
    /// 菜单ID.
    /// </summary>
    public override string menuId { get; set; }

    /// <summary>
    /// 选择导出数据key.
    /// </summary>
    public List<string> selectKey { get; set; }

    /// <summary>
    /// 导出类型.
    /// </summary>
    public string dataType { get; set; } = "0";
}