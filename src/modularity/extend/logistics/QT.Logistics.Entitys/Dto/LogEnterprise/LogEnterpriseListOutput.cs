namespace QT.Logistics.Entitys.Dto.LogEnterprise;

/// <summary>
/// 入驻商家输入参数.
/// </summary>
public class LogEnterpriseListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 电话.
    /// </summary>
    public string phone { get; set; }

    /// <summary>
    /// 负责人.
    /// </summary>
    public string leader { get; set; }

    /// <summary>
    /// 管理员id.
    /// </summary>
    public string adminId { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    public int status { get; set; }

    /// <summary>
    /// 负责人姓名.
    /// </summary>
    public string adminIdName { get; set; }

    /// <summary>
    /// 登录账号
    /// </summary>
    public string account { get; set; }
}