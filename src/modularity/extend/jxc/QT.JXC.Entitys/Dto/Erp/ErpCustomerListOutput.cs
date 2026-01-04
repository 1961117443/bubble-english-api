using Newtonsoft.Json;

namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 客户信息输入参数.
/// </summary>
public class ErpCustomerListOutput
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
    /// 客户名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 客户编号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 客户地址.
    /// </summary>
    public string address { get; set; }

    /// <summary>
    /// 负责人.
    /// </summary>
    public string admin { get; set; }

    /// <summary>
    /// 负责人电话.
    /// </summary>
    public string admintel { get; set; }

    /// <summary>
    /// 客户简介.
    /// </summary>
    public string description { get; set; }

    /// <summary>
    /// 登录帐号j,角色是“客户”.
    /// </summary>
    public string loginId { get; set; }

    /// <summary>
    /// 登录密码.
    /// </summary>
    public string loginPwd { get; set; }

    /// <summary>
    /// 是否停用
    /// </summary>
    public int stop { get; set; }

    /// <summary>
    /// 公司名称.
    /// </summary>
    public string oidName { get; set; }

    /// <summary>
    /// 客户类型.
    /// </summary>
    public string type { get; set; }

    [JsonIgnore]
    public string srcDiningType { get; set; }
    /// <summary>
    /// 餐别.
    /// </summary>
    public string[] diningType { get; set; }

    /// <summary>
    /// 折扣定价
    /// </summary>
    public decimal? discount { get; set; }


    /// <summary>
    /// 送货人.
    /// </summary>
    public string deliveryManId { get; set; }
}