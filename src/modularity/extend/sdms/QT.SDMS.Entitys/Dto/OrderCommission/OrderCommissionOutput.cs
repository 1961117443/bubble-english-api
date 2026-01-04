namespace QT.SDMS.Entitys.Dto.OrderCommission;

public class OrderCommissionOutput
{
    public string id { get; set; }

    public string no { get; set; }

    public string userIdName { get; set; }

    public decimal proportion { get; set; }

    public decimal amount { get; set; }

    public decimal orderAmount { get; set; }


    public string balanceIdName { get; set; }


    public DateTime? balanceTime { get; set; }

    /// <summary>
    /// 佣金单状态
    /// </summary>
    public int? status { get; set; }
}
