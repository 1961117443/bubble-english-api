using QT.Common.Models;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpSupplier;


/// <summary>
/// 供货商信息修改输入参数.
/// </summary>
public class ErpSupplierCrInput
{
    /// <summary>
    /// 名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 地址.
    /// </summary>
    public string address { get; set; }

    /// <summary>
    /// 负责人.
    /// </summary>
    public string admin { get; set; }

    /// <summary>
    /// 负责人电话.
    /// </summary>
    public string adminTel { get; set; }

    /// <summary>
    /// 入驻时间.
    /// </summary>
    public DateTime? joinTime { get; set; }

    /// <summary>
    /// LOGO.
    /// </summary>
    public List<FileControlsModel> logo { get; set; }

    /// <summary>
    /// 经营周期.
    /// </summary>
    public string workCycle { get; set; }

    /// <summary>
    /// 经营时间.
    /// </summary>
    public string workTime { get; set; }

    /// <summary>
    /// 登录帐号，角色为“供货商”.
    /// </summary>
    public string loginId { get; set; }

    /// <summary>
    /// 登录密码.
    /// </summary>
    public string loginPwd { get; set; }

    /// <summary>
    /// 业务人员.
    /// </summary>
    public string salesman { get; set; }

    /// <summary>
    /// 营业执照.
    /// </summary>
    public List<FileControlsModel> businessLicense { get; set; }

    /// <summary>
    /// 座机号码
    /// </summary>
    public string landlineNumber { get; set; }

    /// <summary>
    /// 生产许可证.
    /// </summary>
    public List<FileControlsModel> productionLicense { get; set; }

    /// <summary>
    /// 食品经营许可证.
    /// </summary>
    public List<FileControlsModel> foodBusinessLicense { get; set; }
}