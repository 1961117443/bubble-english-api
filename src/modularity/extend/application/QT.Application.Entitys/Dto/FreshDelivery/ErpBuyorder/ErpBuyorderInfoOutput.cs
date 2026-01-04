using QT.Common.Models;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpBuyorder;

/// <summary>
/// 采购任务订单输出参数.
/// </summary>
public class ErpBuyorderInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 公司ID.
    /// </summary>
    public string oid { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 指派采购人员.
    /// </summary>
    public string[] taskToUserId { get; set; }

    /// <summary>
    /// 计划采购时间.
    /// </summary>
    public DateTime? taskBuyTime { get; set; }

    /// <summary>
    /// 任务备注.
    /// </summary>
    public string taskRemark { get; set; }

    /// <summary>
    /// 采购订单明细.
    /// </summary>
    public List<ErpBuyorderdetailInfoOutput> erpBuyorderdetailList { get; set; }

    /// <summary>
    /// 订单状态(0:未完成,1：已采购，2：已入库).
    /// </summary>
    public string state { get; set; }

    /// <summary>
    /// 凭据.
    /// </summary>
    public List<FileControlsModel> proof { get; set; }

    /// <summary>
    /// 供应商
    /// </summary>
    public string supplierName { get; set; }

    /// <summary>
    /// 配送日期.
    /// </summary>
    public string[] posttime { get; set; }

    /// <summary>
    /// 审核时间
    /// </summary>
    public DateTime? auditTime { get; set; }


    /// <summary>
    /// 入库单号
    /// </summary>
    public string rkNo { get; set; }
}