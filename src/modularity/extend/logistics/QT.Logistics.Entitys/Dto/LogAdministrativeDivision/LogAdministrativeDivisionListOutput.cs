namespace QT.Logistics.Entitys.Dto.LogAdministrativeDivision;

/// <summary>
/// 区域管理输入参数.
/// </summary>
public class LogAdministrativeDivisionListOutput
{
    /// <summary>
    /// 自然主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 区域名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 范围介绍.
    /// </summary>
    public string description { get; set; }

}