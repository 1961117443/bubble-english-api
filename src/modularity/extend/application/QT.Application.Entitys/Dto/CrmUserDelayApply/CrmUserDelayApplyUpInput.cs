namespace QT.Iot.Application.Dto.CrmUserDelayApply;

public class CrmUserDelayApplyUpInput: CrmUserDelayApplyCrInput
{
    public string id { get; set; }

    //public string userId { get; set; }

    public int status { get; set; }

    public DateTime? expireTime { get; set; }

    //public List<FileControlsModel> attachment { get; set; }
    public string content { get; set; }
}
