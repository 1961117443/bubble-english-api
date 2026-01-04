using QT.Common.Models;

namespace QT.JXC.Entitys.Dto.Erp.BuyOrder;

/// <summary>
/// 采购任务订单修改输入参数.
/// </summary>
public class ErpBuyorderCrInput
{
    /// <summary>
    /// 公司ID.
    /// </summary>
    public virtual string oid { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    public virtual string no { get; set; }

    /// <summary>
    /// 指派采购人员.
    /// </summary>
    public virtual string[] taskToUserId { get; set; }

    /// <summary>
    /// 计划采购时间.
    /// </summary>
    public virtual DateTime? taskBuyTime { get; set; }

    /// <summary>
    /// 任务备注.
    /// </summary>
    public virtual string taskRemark { get; set; }

    /// <summary>
    /// 采购订单明细.
    /// </summary>
    public virtual List<ErpBuyorderdetailCrInput> erpBuyorderdetailList { get; set; }

    /// <summary>
    /// 配送日期
    /// </summary>
    public virtual List<string> posttime { get; set; }
}


/// <summary>
/// 采购任务订单 入库审核修改入库数量.
/// </summary>
public class ErpBuyorderCkUpInput
{
    /// <summary>
    /// ID.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 采购订单明细.
    /// </summary>
    public List<ErpBuyorderdetailCkUpInput> erpBuyorderdetailList { get; set; }


}

/// <summary>
/// 采购任务订单 入库审核修改入库数量.
/// </summary>
public class ErpBuyorderdetailCkUpInput
{
    /// <summary>
    /// ID.
    /// </summary>
    public string id { get; set; }

    public decimal? inNum { get; set; }

    /// <summary>
    /// 仓库
    /// </summary>
    public string storeRomeId { get; set; }

    /// <summary>
    /// 库区
    /// </summary>
    public string storeRomeAreaId { get; set; }



    public decimal? price { get; set; }

    public decimal? amount { get; set; }

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
}

/// <summary>
/// 更新附件.
/// </summary>
public class ErpBuyorderCkUpProofInput
{
    /// <summary>
    /// ID.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 质检报告.
    /// </summary>
    public List<FileControlsModel> qualityReportProof { get; set; }

    /// <summary>
    /// 自检报告.
    /// </summary>
    public List<FileControlsModel> selfReportProof { get; set; }


}