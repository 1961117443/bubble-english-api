using QT.DependencyInjection;

namespace QT.Logistics.Entitys.Dto.LogProduct;

/// <summary>
/// 商家商品信息输入参数.
/// </summary>
public class LogProductListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

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

    /// <summary>
    /// 产地.
    /// </summary>
    public string producer { get; set; }

    /// <summary>
    /// 介绍.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 存储条件.
    /// </summary>
    public string storage { get; set; }

    /// <summary>
    /// 保质期.
    /// </summary>
    public string retention { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    public int state { get; set; }


    /// <summary>
    /// 分类名称.
    /// </summary>
    public string tidName { get; set; }
}

///// <summary>
///// 选择商品信息，分页查询出参
///// </summary>
//[SuppressSniffer]
//public class LogEnterpriseProductListSelectorOutput
//{
//    public string id { get; set; }
//    public string name { get; set; }
//    public decimal costPrice { get; set; }
//    public decimal minNum { get; set; }

//    /// <summary>
//    /// 库存数量
//    /// </summary>
//    public decimal num { get; set; }
//    public string productName { get; set; }
//    public decimal salePrice { get; set; }
//    public string unit { get; set; }
//    public decimal maxNum { get; set; }
//}