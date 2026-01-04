namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 库存更新输入.
/// </summary>
public class ErpOutdetailRecordUpInput
{
    /// <summary>
    /// 出库明细id.
    /// </summary>
    public string id { get; set; }

    ///// <summary>
    ///// 商品规格id.
    ///// </summary>
    //public string gid { get; set; }

    /// <summary>
    /// 出库数量
    /// </summary>
    public decimal num { get; set; }

    /// <summary>
    /// 分拣数量（处理单位不一致的情况，比如下单个，但是分拣按斤，此时fjNum={x}个）
    /// </summary>
    public decimal? fjNum { get; set; }

    /// <summary>
    /// 是否分拣完成
    /// </summary>
    public int? done { get; set; }

    /// <summary>
    /// 需要扣减的入库记录.
    /// </summary>
    public List<ErpOutdetailRecordInInput> records { get; set; }
}

/// <summary>
/// 入库明细
/// </summary>
public class ErpOutdetailRecordInInput
{
    /// <summary>
    /// 入库id.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 出库数量.
    /// </summary>
    [Obsolete]
    public decimal num { get; set; }
}

public class ErpOutdetailRecordUpOutput
{
    /// <summary>
    /// 本次出库成本
    /// </summary>
    public decimal CostAmount { get; set; }
}