
using NPOI.Util;
using SqlSugar;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOrderdetail;

/// <summary>
/// 订单商品表输出参数.
/// </summary>
public class ErpOrderdetailInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }


    /// <summary>
    /// 订单主键.
    /// </summary>
    public string fid { get; set; }

    /// <summary>
    /// 商品规格ID.
    /// </summary>
    public string mid { get; set; }

    /// <summary>
    /// 配送数量.
    /// </summary>
    public decimal num1 { get; set; }

    /// <summary>
    /// 配送总价.
    /// </summary>
    public decimal amount1 { get; set; }

    /// <summary>
    /// 复核数量.
    /// </summary>
    public decimal num2 { get; set; }

    /// <summary>
    /// 复核总价.
    /// </summary>
    public decimal amount2 { get; set; }

    /// <summary>
    /// 复核时间.
    /// </summary>
    public DateTime? checkTime { get; set; }

    /// <summary>
    /// 订单数量.
    /// </summary>
    public decimal num { get; set; }

    /// <summary>
    /// 订单总价.
    /// </summary>
    public decimal amount { get; set; }


    /// <summary>
    /// 商品规格.
    /// </summary>
    public string midName { get; set; }

    /// <summary>
    /// 商品单价.
    /// </summary>
    public decimal salePrice { get; set; }

    /// <summary>
    /// 订单备注
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 商品名称.
    /// </summary>
    public string productName { get; set; }

    /// <summary>
    /// 分拣时间.
    /// </summary>
    public DateTime? sorterTime { get; set; }

    /// <summary>
    /// 分拣人员.
    /// </summary>
    public string sorterUserId { get; set; }

    /// <summary>
    /// 分拣备注.
    /// </summary>
    public string sorterDes { get; set; }

    /// <summary>
    /// 分拣状态.
    /// </summary>
    public string sorterState { get; set; }

    /// <summary>
    /// 送达状态.
    /// </summary>
    public int receiveState { get; set; }

    /// <summary>
    /// 送达时间.
    /// </summary>
    public DateTime? receiveTime { get; set; }

    public string sorterUserName { get; set; }

    /// <summary>
    /// 规格单位
    /// </summary>
    public string midUnit { get; set; }

    /// <summary>
    /// 主单位数量比.
    /// </summary>
    public decimal? ratio { get; set; }

    /// <summary>
    /// 商品一级分类
    /// </summary>
    public string rootProducttype { get; set; }

    /// <summary>
    /// 图片集合
    /// </summary>
    public IEnumerable<string> imageList { get; set; }

    /// <summary>
    /// 商品id
    /// </summary>
    public string productId { get; set; }

    /// <summary>
    /// 订单编号
    /// </summary>
    public string orderNo { get; set; }


    /// <summary>
    /// 客户单位
    /// </summary>
    public string customerUnit { get; set; }

    /// <summary>
    /// 客户名称
    /// </summary>
    public string customerName { get; set; }

    /// <summary>
    /// 退回数量
    /// </summary>
    public decimal rejectNum { get; set; }

    /// <summary>
    /// 分拣数量.
    /// </summary>
    public decimal fjNum { get; set; }

    /// <summary>
    /// 子单实际分拣.
    /// </summary>
    public decimal cnum1 { get; set; }


    /// <summary>
    /// 子单分拣数量.
    /// </summary>
    public decimal cfjNum { get; set; }

    /// <summary>
    /// 待分拣数量（会根据单位判断）
    /// </summary>
    public decimal wnum
    {
        get
        {
            if (string.IsNullOrEmpty(customerUnit) || customerUnit == midUnit)
            {
                return num - num1 - cnum1;
            }

            if (customerUnit != midUnit)
            {
                return num - fjNum - cfjNum;
            }

            return 0;
        }
    }

    /// <summary>
    /// 是否分拣完成（会根据单位判断）
    /// 存在子单的情况下才会判断
    /// </summary>
    public bool isCompleted
    {
        get
        {
            if (cnum1>0)
            {
                if (this.sorterFinishTime.HasValue)
                {
                    return true;
                }
                if (string.IsNullOrEmpty(customerUnit) || customerUnit == midUnit)
                {
                    return cnum1>=num;
                }

                if (customerUnit != midUnit)
                {
                    return cfjNum>=num;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// 分拣完成时间.
    /// </summary>
    public DateTime? sorterFinishTime { get; set; }

    /// <summary>
    /// 餐别
    /// </summary>
    public string diningType { get; set; }

    public int? order { get; set; }

    /// <summary>
    /// 配送日期
    /// </summary>
    public DateTime? posttime { get; set; }

    /// <summary>
    /// 打印别名
    /// </summary>
    public string printAlais { get; set; }

    /// <summary>
    /// 关联特殊入库数量
    /// </summary>
    public decimal? tsNum { get; set; }


    /// <summary>
    /// 生产日期.
    /// </summary>
    //public string productDate { get; set; }
    public DateTime? productionDate { get; set; }
    /// <summary>
    /// 保质期.
    /// </summary>
    public string retention { get; set; }

    /// <summary>
    /// 打印次数
    /// </summary>
    public int printCount { get; set; }
}

public class ErpOrderdetailXsInfoOutput: ErpOrderdetailInfoOutput
{
    /// <summary>
    /// 起售数.
    /// </summary>
    public decimal minNum { get; set; }

    /// <summary>
    /// 限购数.
    /// </summary>
    public decimal maxNum { get; set; }

}