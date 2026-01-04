using QT.Common.Models;
using QT.Common.Models.User;
using QT.Logistics.Entitys.Dto.LogEnterprise;

namespace QT.Logistics.Entitys.Dto.LogPCWeb;

public class LogEnterpriseWebInfoOutput
{
    public string id { get; set; }
    public string name { get; set; }
    public string admin { get; set; }
    public string phone { get; set; }

    /// <summary>
    /// 商家扩展属性
    /// </summary>
    public List<LogEnterprisePropertyWebInfoOutput> properties { get; set; }

    public List<FileControlsModel> imageUrl { get; set; }
}


public class LogEnterprisePropertyWebInfoOutput
{
    public string label { get; set; }
    public string value { get; set; }
}