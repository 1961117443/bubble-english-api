using QT.Common.Filter;

namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 商品信息列表查询输入
/// </summary>
public class ErpProductListQueryInput : PageInputBase
{
    /// <summary>
    /// 选择导出数据key.
    /// </summary>
    public string selectKey { get; set; }

    /// <summary>
    /// 导出类型 (0：分页数据，其他：全部数据).
    /// </summary>
    public int dataType { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 别名.
    /// </summary>
    public string nickname { get; set; }

    /// <summary>
    /// 拼音首字母.
    /// </summary>
    public string firstChar { get; set; }

    /// <summary>
    /// 编号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 商品分类
    /// </summary>
    public string tid { get; set; }

    /// <summary>
    /// 关联公司
    /// </summary>
    public string relationCompanyId { get; set; }

}


public class ErpProductSelectorQueryInput: PageInputBase
{
    /// <summary>
    /// 客户id
    /// </summary>
    public string cid { get; set; }

    /// <summary>
    /// 商品规格id.
    /// </summary>
    public string gid { get; set; }

    /// <summary>
    /// 公司id
    /// </summary>
    public string oid { get; set; }

    /// <summary>
    /// 分类id.
    /// </summary>
    public string tid { get; set; }

    /// <summary>
    /// 是否判断定价
    /// </summary>
    public bool? filterPrice { get; set; }

    /// <summary>
    /// 过滤有库存的数据
    /// </summary>
    public bool? hasNum { get; set; }

    public List<string> gidList { get; set; }
}