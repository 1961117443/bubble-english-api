namespace QT.Iot.Application.Dto.MaintenancePlanOrder;

public class MaintenancePlanRecordListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 物资ID.
    /// </summary>
    public string mid { get; set; }

    /// <summary>
    /// 工时.
    /// </summary>
    public decimal? num { get; set; }

    ///// <summary>
    ///// 人工合价.
    ///// </summary>
    //public decimal artificialAmount { get; set; }

    ///// <summary>
    ///// 材料合价.
    ///// </summary>
    //public decimal materialAmount { get; set; }

    ///// <summary>
    ///// 小计金额.
    ///// </summary>
    //public decimal amount { get; set; }


    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    public string midName { get; set; }
    public string midCode { get; set; }
    public string midUnit { get; set; }
    public string midSpec { get; set; }
}
