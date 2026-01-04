namespace QT.Iot.Application.Dto.CrmOrderCommission;

public class CrmOrderCommissionOutput
{
    public string id { get; set; }

    public string no { get; set; }

    public string userIdName { get; set; }

    public decimal proportion { get; set; }

    public decimal amount { get; set; }

    public decimal orderAmount { get; set; }


    public string balanceIdName { get; set; }


    public DateTime? balanceTime { get; set; }
}
