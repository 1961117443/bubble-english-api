using QT.Common.Filter;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpSupplierprice;

/// <summary>
/// 供应商定价列表查询输入
/// </summary>
public class ErpSupplierpriceListQueryInput : PageInputBase
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
    /// 供应商id.
    /// </summary>
    public string supplierId { get; set; }

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