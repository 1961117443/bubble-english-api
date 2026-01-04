namespace QT.SDMS.Entitys.Dto.CustomerMeterPoint;

public class CustomerMeterPointCrInput
{
    public string customerId { get; set; } 

    public string meterCode { get; set; }

    public string meterName { get; set; }

    public string address { get; set; } 

    public decimal? multiplier { get; set; }

    public string voltageLevel { get; set; }

    public int meterType { get; set; }
    public int status { get; set; }
}