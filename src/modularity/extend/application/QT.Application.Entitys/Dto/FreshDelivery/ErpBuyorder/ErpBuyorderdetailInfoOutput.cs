
using QT.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpBuyorder;

/// <summary>
/// 采购订单明细输出参数.
/// </summary>
public class ErpBuyorderdetailInfoOutput
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
    /// 预计采购数量.
    /// </summary>
    public decimal planNum { get; set; }

    /// <summary>
    /// 规格.
    /// </summary>
    public string gidName { get; set; }

    /// <summary>
    /// 商品名称.
    /// </summary>
    public string productName { get; set; }

    /// <summary>
    /// 供应商id.
    /// </summary>
    public string supplier { get; set; }

    /// <summary>
    /// 供应商.
    /// </summary>
    public string supplierName { get; set; }

    /// <summary>
    /// 采购数量.
    /// </summary>
    public decimal? num { get; set; }


    /// <summary>
    /// 采购时间.
    /// </summary>
    public DateTime? buyTime { get; set; }


    /// <summary>
    /// 分量统计
    /// </summary>
    public string remark { get; set; }

    public decimal? price { get; set; }

    public decimal? amount { get; set; }


    /// <summary>
    /// 付款属性.
    /// </summary>
    public string payment { get; set; }

    /// <summary>
    /// 采购渠道.
    /// </summary>
    public string channel { get; set; }

    /// <summary>
    /// 入库数量.
    /// </summary>
    public decimal? inNum { get; set; }

    /// <summary>
    /// 采购状态
    /// </summary>
    public string buyState { get; set; }

    /// <summary>
    /// 库存数量.
    /// </summary>
    public decimal? storeNum { get; set; }

    /// <summary>
    /// 明细备注
    /// </summary>
    public string itRemark { get; set; }

    /// <summary>
    /// 仓库
    /// </summary>
    public string storeRomeId { get; set; }

    /// <summary>
    /// 库区
    /// </summary>
    public string storeRomeAreaId { get; set; }

    /// <summary>
    /// 是否仓库加工
    /// </summary>
    public bool whetherProcess { get; set; }

    /// <summary>
    /// 计量单位
    /// </summary>
    public string unit { get; set; }

    /// <summary>
    /// 商品拼音首字母
    /// </summary>
    public string firstChar { get; set; }

    /// <summary>
    /// 一级分类
    /// </summary>
    public string rootProducttype { get; set; }

    /// <summary>
    /// 特殊入库数量
    /// </summary>
    public decimal tsNum { get; set; }

    /// <summary>
    /// 生产日期.
    /// </summary>
    public DateTime? productionDate { get; set; }

    /// <summary>
    /// 批次号.
    /// </summary>
    public string batchNumber { get; set; }


    /// <summary>
    /// 保质期.
    /// </summary>
    public string retention { get; set; }

    /// <summary>
    /// 退回数量
    /// </summary>
    public decimal thNum { get; set; }

    /// <summary>
    /// 退回金额
    /// </summary>
    public decimal thAmount => thNum == (inNum + tsNum) ? (amount ?? 0) : thNum * (price ?? 0);

    /// <summary>
    /// 客户类型.
    /// </summary>
    public string customerType { get; set; }

    /// <summary>
    /// 销货清单图片
    /// </summary>
    public virtual string salesImageJson { get; set; }

    /// <summary>
    /// 是否赠品
    /// </summary>
    public int isFree { get; set; }
}

public class ErpBuyorderdetailDoneInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 采购数量.
    /// </summary>
    public decimal? num { get; set; }


    /// <summary>
    /// 采购时间.
    /// </summary>
    public DateTime? buyTime { get; set; }

    /// <summary>
    /// 凭据.
    /// </summary>
    //[Required(ErrorMessage = "凭据不能为空")]
    public List<FileControlsModel> proof { get; set; }


    /// <summary>
    /// 分量统计
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 采购单价.
    /// </summary>
    public decimal? price { get; set; }


    /// <summary>
    /// 采购金额.
    /// </summary>
    public decimal? amount { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string itRemark { get; set; }

    /// <summary>
    /// 生产日期
    /// </summary>
    public DateTime? productionDate { get; set; }

    /// <summary>
    /// 保质期
    /// </summary>
    public string retention { get; set; }
}


public class ErpBuyorderdetailOutput : ErpBuyorderdetailDoneInput
{
    /// <summary>
    /// 预计采购数量.
    /// </summary>
    public decimal planNum { get; set; }
}

