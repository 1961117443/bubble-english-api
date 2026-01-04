namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 商品定价输出参数.
/// </summary>
public class ErpProductpriceInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 规格ID.
    /// </summary>
    public string gid { get; set; }

    /// <summary>
    /// 价格.
    /// </summary>
    public decimal price { get; set; }

    /// <summary>
    /// 定价时间.
    /// </summary>
    public DateTime? time { get; set; }

    /// <summary>
    /// 规格名称
    /// </summary>
    public string gidName { get; set; }

    /// <summary>
    /// 商品名称
    /// </summary>
    public string productName { get; set; }


    /// <summary>
    /// 规格原价.
    /// </summary>
    public decimal salePrice { get; set; }

    /// <summary>
    /// 客户id.
    /// </summary>
    public string cid { get; set; }


    /// <summary>
    /// 客户名称
    /// </summary>
    public string cName { get; set; }

    /// <summary>
    /// 客户关联的公司
    /// </summary>
    public string cidOName { get; set; }


    /// <summary>
    /// 一级分类
    /// </summary>
    public string rootProducttype { get; set; }
}