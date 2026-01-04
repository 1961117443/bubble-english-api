namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 商品规格修改输入参数.
/// </summary>
public class ErpProductmodelCrInput
{
    public string id { get; set; }
    /// <summary>
    /// 规格名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 主单位数量比.
    /// </summary>
    public string ratio { get; set; }

    /// <summary>
    /// 成本价.
    /// </summary>
    public decimal? costPrice { get; set; }

    /// <summary>
    /// 销售价.
    /// </summary>
    public decimal? salePrice { get; set; }

    /// <summary>
    /// 起售数.
    /// </summary>
    public decimal? minNum { get; set; }
    /// <summary>
    /// 限售数.
    /// </summary>
    public decimal? maxNum { get; set; }

    /// <summary>
    /// 毛利率.
    /// </summary>
    public decimal? grossMargin { get; set; }

    /// <summary>
    /// 包装物.
    /// </summary>
    public string package { get; set; }

    /// <summary>
    /// 库存.
    /// </summary>
    public decimal? num { get; set; }


    /// <summary>
    /// 条码
    /// </summary>
    public string barCode { get; set; }
    /// <summary>
    /// 计量单位
    /// </summary>
    public string unit { get; set; }


    /// <summary>
    /// 关联产品
    /// </summary>
    public string rid { get; set; }

    /// <summary>
    /// 客户单位
    /// </summary>
    public string customerUnit { get; set; }


    /// <summary>
    /// 规格关联公司.
    /// </summary>
    public List<string> erpProductcompanyList { get; set; }
}