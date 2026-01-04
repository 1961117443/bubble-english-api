namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 订单处理记录表输出参数.
/// </summary>
public class ErpOrderoperaterecordInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 状态值.
    /// </summary>
    public string state { get; set; }

    /// <summary>
    /// 时间.
    /// </summary>
    public DateTime? time { get; set; }

    /// <summary>
    /// 处理人.
    /// </summary>
    public string userId { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

}

public class ErpPrintrecordInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; } 

    /// <summary>
    /// 时间.
    /// </summary>
    public DateTime? creatorTime { get; set; }

    /// <summary>
    /// 处理人id.
    /// </summary>
    public string creatorUserId { get; set; }

    /// <summary>
    /// 处理人.
    /// </summary>
    public string creatorUserIdName { get; set; }
}