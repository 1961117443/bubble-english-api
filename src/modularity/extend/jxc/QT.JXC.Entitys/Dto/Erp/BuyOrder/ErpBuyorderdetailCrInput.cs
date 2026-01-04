using SqlSugar;
using System.ComponentModel;

namespace QT.JXC.Entitys.Dto.Erp.BuyOrder;

/// <summary>
/// 采购订单明细修改输入参数.
/// </summary>
public class ErpBuyorderdetailCrInput
{
    /// <summary>
    /// 规格ID.
    /// </summary>
    public virtual string gid { get; set; }

    /// <summary>
    /// 预计采购数量.
    /// </summary>
    public virtual decimal planNum { get; set; }


    /// <summary>
    /// 付款属性.
    /// </summary>
    public virtual string payment { get; set; }


    /// <summary>
    /// 分量统计.
    /// </summary>
    public virtual string remark { get; set; }

    /// <summary>
    /// 明细备注.
    /// </summary>
    public virtual string itRemark { get; set; }

    /// <summary>
    /// 采购渠道.
    /// </summary>
    public virtual string channel { get; set; }


    /// <summary>
    /// 供应商.
    /// </summary>
    public virtual string supplier { get; set; }



    /// <summary>
    /// 入库数量.
    /// </summary>
    public virtual decimal? inNum { get; set; }

    /// <summary>
    /// 采购数量.
    /// </summary>
    public virtual decimal? num { get; set; }


    /// <summary>
    /// 库存数量.
    /// </summary>
    public virtual decimal? storeNum { get; set; }

    /// <summary>
    /// 是否仓库加工
    /// </summary>
    public virtual bool whetherProcess { get; set; }

    /// <summary>
    /// 单价.
    /// </summary>
    public virtual decimal? price { get; set; }

    /// <summary>
    /// 金额.
    /// </summary>
    public virtual decimal? amount { get; set; }


    public virtual DateTime? productionDate { get; set; }

    public virtual string retention { get; set; }

    /// <summary>
    /// 客户类型
    /// </summary>
    public virtual string customerType { get; set; }

    /// <summary>
    /// 销货清单图片
    /// </summary>
    public virtual string salesImageJson { get; set; }


    /// <summary>
    /// 关联的订单明细ID集合，逗号分隔
    /// </summary>
    public string orderDetailIds { get; set; }
}