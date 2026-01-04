namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 送货员输出参数.
/// </summary>
public class ErpDeliverymanInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 公司ID.
    /// </summary>
    public string oid { get; set; }

    /// <summary>
    /// 姓名.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 电话.
    /// </summary>
    public string mobile { get; set; }

    /// <summary>
    /// 登录帐号，角色叫送货员.
    /// </summary>
    public string loginId { get; set; }

    /// <summary>
    /// 登录密码.
    /// </summary>
    public string loginPwd { get; set; }

    /// <summary>
    /// 线路送货员（中间表）.
    /// </summary>
    public List<ErpDeliveryrouteDeliverymanInfoOutput> erpDeliveryrouteDeliverymanList { get; set; }


    /// <summary>
    /// 车辆
    /// </summary>
    public string deliveryCar { get; set; }



    /// <summary>
    /// 车队长
    /// </summary>
    public string carCaptainId { get; set; }
}