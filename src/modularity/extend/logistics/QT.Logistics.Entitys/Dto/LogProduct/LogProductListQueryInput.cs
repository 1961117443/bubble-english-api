using QT.Common.Filter;
using QT.DependencyInjection;

namespace QT.Logistics.Entitys.Dto.LogProduct;

/// <summary>
/// 商家商品信息列表查询输入
/// </summary>
public class LogProductListQueryInput : PageInputBase
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

///// <summary>
///// 选择商品，分页查询入参
///// </summary>
//[SuppressSniffer]
//public class LogEnterpriseProductListSelectorQueryInput: PageInputBase
//{

//}