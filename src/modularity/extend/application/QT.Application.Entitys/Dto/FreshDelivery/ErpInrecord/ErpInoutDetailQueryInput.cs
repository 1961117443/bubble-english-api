using QT.Common.Filter;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpInrecord;

public class ErpInoutDetailQueryInput:PageInputBase
{
    /// <summary>
    /// 入库订单号 / 采购订单号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 商品名称
    /// </summary>
    public string productName { get; set; }
}

public class ErpInoutDetailQueryOutput
{
    public string id { get; set; }
    public string inid { get; set; }

    /// <summary>
    /// 单据编号
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 商品名称
    /// </summary>
    public string productName { get; set; }

    /// <summary>
    /// 商品规格
    /// </summary>
    public string gidName { get; set; }

    /// <summary>
    /// 单据日期
    /// </summary>
    public DateTime? date { get; set; }

    /// <summary>
    /// 入库数
    /// </summary>
    public decimal? inNum { get; set; }

    /// <summary>
    /// 出库数
    /// </summary>
    public decimal? outNum { get; set; }

    public decimal?price { get; set; }
    public decimal? amount { get; set; }

    /// <summary>
    /// 库存数量
    /// </summary>
    public decimal? num { get; set; }

    public bool hasChildren { get=> children != null && children.Count > 0; }

    public List<ErpInoutDetailQueryOutput> children { get; set; }
}