using QT.Common.Models;
using QT.Application.Entitys.Dto.FreshDelivery.ErpProductprice;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpCustomer;

/// <summary>
/// 客户信息修改输入参数.
/// </summary>
public class ErpCustomerCrInput
{
    ///// <summary>
    ///// 公司ID.
    ///// </summary>
    //public List<string> oid { get; set; }

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
    /// 营业执照.
    /// </summary>
    public List<FileControlsModel> license { get; set; }

    /// <summary>
    /// 登录帐号j,角色是“客户”.
    /// </summary>
    public string loginId { get; set; }

    /// <summary>
    /// 登录密码.
    /// </summary>
    public string loginPwd { get; set; }

    /// <summary>
    /// 商品定价.
    /// </summary>
    public List<ErpProductpriceCrInput> erpProductpriceList { get; set; }

    public string type { get; set; }

    /// <summary>
    /// 是否停用
    /// </summary>
    public int stop { get; set; }


    /// <summary>
    /// 客户简称.
    /// </summary>
    public string shortName { get; set; }

    /// <summary>
    /// 餐别.多个逗号相连
    /// </summary>
    public string diningType { get; set; }

    /// <summary>
    /// 送货人id.
    /// </summary>
    public string deliveryManId { get; set; }

    /// <summary>
    /// 折扣定价
    /// </summary>
    public decimal? discount { get;set; }
}