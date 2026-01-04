using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JXC.Entitys.Dto.Erp;

public class ErpProductSelectorOutput: ErpProductmodelInfoOutput
{
    /// <summary>
    /// 商品id.
    /// </summary>
    public string productId { get; set; }
    /// <summary>
    /// 商品编码.
    /// </summary>
    public string productCode { get; set; }

    /// <summary>
    /// 商品名称.
    /// </summary>
    public string productName { get; set; }

    /// <summary>
    /// 商品单位.
    /// </summary>
    public string productUnit { get; set; }


    /// <summary>
    /// 供应商.
    /// </summary>
    public string supplier { get; set; }
    /// <summary>
    /// 供应商.
    /// </summary>
    public string supplierName { get; set; }

    /// <summary>
    /// 图片集合
    /// </summary>
    public IEnumerable<string> imageList { get; set; }

    /// <summary>
    /// 一级分类
    /// </summary>
    public string rootProducttype { get; set; }
}

/// <summary>
/// 采购建议输出模型
/// </summary>
public class ErpProductRecommendOutput : ErpProductSelectorOutput
{
    /// <summary>
    /// 明细备注.
    /// </summary>
    public string remark { get; set; }

    public decimal storeNum { get; set; }

    /// <summary>
    /// 关联库存
    /// </summary>
    public decimal relationStoreNum { get; set; }

    /// <summary>
    /// 总库存
    /// </summary>
    public decimal totalStoreNum { get; set; }

    /// <summary>
    /// 订单明细集合，逗号相连
    /// </summary>
    public string orderDetails { get; set; }

    /// <summary>
    /// 申购数量
    /// </summary>
    public decimal buyNum { get; set; }
}


public class ErpProductRecommendInput
{
    public string oid { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string posttime { get; set; }

    /// <summary>
    /// 类型: 0:不包含特殊入库（正常的模式），1：包含特殊入库
    /// </summary>
    public int? dataType { get; set; }
}

/// <summary>
/// 货品销售价
/// </summary>
public class QueryProductSalePriceOutput
{
    /// <summary>
    /// 规格id
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 销售价.
    /// </summary>
    public decimal salePrice { get; set; }
}