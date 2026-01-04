using QT.Common.Filter;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpStoreroom;

/// <summary>
/// 仓库信息列表查询输入
/// </summary>
public class ErpStoreroomListQueryInput : PageInputBase
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

    /// <summary>
    /// 拼音首字母.
    /// </summary>
    public string firstChar { get; set; }

}