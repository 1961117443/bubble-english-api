namespace QT.JXC.Entitys.Dto;

/// <summary>
/// 盘点记录修改输入参数.
/// </summary>
public class ErpProductcheckCrInput
{
    /// <summary>
    /// 规格ID.
    /// </summary>
    public string gid { get; set; }

    /// <summary>
    /// 系统数量.
    /// </summary>
    public decimal systemNum { get; set; }

    /// <summary>
    /// 实际数量.
    /// </summary>
    public decimal realNum { get; set; }

    /// <summary>
    /// 拆损数量.
    /// </summary>
    public decimal loseNum { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }


    /// <summary>
    /// 仓库.
    /// </summary>
    public string storeRomeId { get; set; }

    /// <summary>
    /// 库区.
    /// </summary>
    public string storeRomeAreaId { get; set; }

    /// <summary>
    /// 入库单价.
    /// </summary>
    public decimal price { get; set; }

    /// <summary>
    /// 入库金额.
    /// </summary>
    public decimal amount { get; set; }

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