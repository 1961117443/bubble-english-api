using Newtonsoft.Json;

namespace QT.JXC.Entitys.Dto.Erp.gz_cycs;

public class CycsInAddInfo
{
    public int cycsId { get; set; } // 所属门店id

    public int supplyId { get; set; } // 供应商id

    public int goodsCode { get; set; } // 商品id

    public decimal amount { get; set; } // 商品重量(kg)

    [JsonConverter(typeof(Newtonsoft.Json.Converters.IsoDateTimeConverter))]
    public DateTime? ticketDate { get; set; } // 进货日期（yyyy-MM-dd）

    public List<Ticket> tickets { get; set; } // 票证信息

    public string divisionCode { get; set; } // 产地编号

    [JsonConverter(typeof(Newtonsoft.Json.Converters.IsoDateTimeConverter))]
    public DateTime? manufactureDate { get; set; } // 生产日期（yyyy-MM-dd）

    [JsonConverter(typeof(Newtonsoft.Json.Converters.IsoDateTimeConverter))]
    public DateTime? expiryDate { get; set; } // 保质期（yyyy-MM-dd）

    public string batchNumber { get; set; } // 生产批次
}


public class Ticket
{
    public string url { get; set; } // base64编码

    public string ticketTypeId { get; set; }

    public string ticketTypeName { get; set; }
}

public class CycsInAddInfoInput: CycsInAddInfo
{
    /// <summary>
    /// 采购明细
    /// </summary>
    public string buyId { get; set; }

    /// <summary>
    /// 缺少商品关联
    /// </summary>
    public bool lossPid { get; set; }

    /// <summary>
    /// 缺少供应商关联
    /// </summary>
    public bool lossSid { get; set; }

    /// <summary>
    /// 商品分类编码
    /// </summary>
    public string code { get; set; }

    /// <summary>
    /// 商品名称
    /// </summary>
    public string productName { get; set; }

    /// <summary>
    /// 供应商
    /// </summary>
    public string supplierName { get; set; }

    public string pid { get; set; }
    public string sid { get; set; }
}
