using QT.Common.Filter;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseProductStore;

/// <summary>
/// 商家商品信息列表查询输入
/// </summary>
public class LogEnterpriseProductStoreListQueryInput : PageInputBase
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
    /// 分类ID.
    /// </summary>
    public string tid { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 拼音首字母.
    /// </summary>
    public string firstChar { get; set; }

}

public class LogEnterpriseProductStoreDetailQueryInput
{
    /// <summary>
    /// 规格集合
    /// </summary>
    public IEnumerable<string> gids { get; set; }

    /// <summary>
    /// 仓库id
    /// </summary>
    public string storeRoomId { get; set; }
}