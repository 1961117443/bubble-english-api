using System.ComponentModel.DataAnnotations;

namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 商品入库记录修改输入参数.
/// </summary>
public class ErpInrecordCrInput
{
    public string id { get; set; }
    /// <summary>
    /// 商品规格ID.
    /// </summary>
    [Required(ErrorMessage ="商品不能为空！")]
    public string gid { get; set; }

    /// <summary>
    /// 采购订单数量.
    /// </summary>
    public decimal orderNum { get; set; }

    /// <summary>
    /// 入库数量.
    /// </summary>
    public decimal inNum { get; set; }

    /// <summary>
    /// 入库单价.
    /// </summary>
    public decimal price { get; set; }

    /// <summary>
    /// 入库金额.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }


    public string storeRomeAreaId { get; set; }
    public string storeRomeId { get; set; }

    public string bid { get;set; }


    /// <summary>
    /// 扣减的库存记录
    /// 调拨、加工（需要生成出库记录的才会有这个属性）
    /// </summary>
    public List<ErpStorerecordInput> storeDetailList { get; set; }


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