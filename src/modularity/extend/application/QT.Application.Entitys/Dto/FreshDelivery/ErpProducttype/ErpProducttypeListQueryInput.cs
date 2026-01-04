using QT.Common.Filter;
namespace QT.Application.Entitys.Dto.FreshDelivery.ErpProducttype;

/// <summary>
/// 商品分类管理列表查询输入
/// </summary>
public class ErpProducttypeListQueryInput : PageInputBase
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
    /// 分类编号.
    /// </summary>
    public string code { get; set; }

    /// <summary>
    /// 拼音首字母.
    /// </summary>
    public string firstChar { get; set; }

    /// <summary>
    /// 父级id
    /// </summary>
    public string fid { get; set; }

}

public class ErpProducttypeChangeOrderInput
{
    public string id { get; set; }

    public int order { get; set; }
}