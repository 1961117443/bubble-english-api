using QT.Common.Filter;

namespace QT.JXC.Entitys.Dto;

/// <summary>
/// 商品客户类型定价列表查询输入
/// </summary>
public class ErpProductcustomertypepriceListQueryInput : PageInputBase
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
    /// 规格ID.
    /// </summary>
    public string gid { get; set; }

    /// <summary>
    /// 客户类型（数据字典）.
    /// </summary>
    public string tid { get; set; }

    /// <summary>
    /// 公司id
    /// </summary>
    public string oid { get; set; }

    /// <summary>
    /// 商品名称
    /// </summary>
    public string productName { get; set; }

    /// <summary>
    /// 一级分类
    /// </summary>
    public string rootTypeId { get; set; }
    
}