using QT.Common.Models;
using QT.DataValidation;
using QT.Logistics.Entitys.Dto.LogOrderAttachment;
using QT.Logistics.Entitys.Dto.LogOrderDetail;
using QT.Logistics.Entitys.Dto.LogOrderFinancial;
using System.ComponentModel.DataAnnotations;

namespace QT.Logistics.Entitys.Dto.LogOrder;

/// <summary>
/// 订单管理修改输入参数.
/// </summary>
public class LogOrderCrInput
{
    /// <summary>
    /// 订单编号.
    /// </summary>
    public string orderNo { get; set; }

    /// <summary>
    /// 寄件配送点id.
    /// </summary>
    public string sendPointId { get; set; }

    /// <summary>
    /// 送达配送点id.
    /// </summary>
    public string reachPointId { get; set; }

    /// <summary>
    /// 订单日期.
    /// </summary>
    public DateTime? orderDate { get; set; }

    /// <summary>
    /// 订单状态.
    /// </summary>
    public string orderStatus { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 寄件人姓名.
    /// </summary>
    public string shipperName { get; set; }

    /// <summary>
    /// 寄件人电话.
    /// </summary>
    [DataValidation(ValidationTypes.PhoneNumber,ErrorMessage ="请输入正确的寄件人手机号码")]
    public string shipperPhone { get; set; }

    /// <summary>
    /// 寄件地址.
    /// </summary>
    public string shipperAddress { get; set; }

    /// <summary>
    /// 收件人姓名.
    /// </summary>
    public string recipientName { get; set; }

    /// <summary>
    /// 收件人电话.
    /// </summary>
    [DataValidation(ValidationTypes.PhoneNumber, ErrorMessage = "请输入正确的收件人手机号码")]
    public string recipientPhone { get; set; }

    /// <summary>
    /// 收件地址.
    /// </summary>
    public string recipientAddress { get; set; }

    /// <summary>
    /// 附件路径.
    /// </summary>
    public List<LogOrderAttachmentCrInput> logOrderAttachmentList { get; set; }

    /// <summary>
    /// 订单物品明细.
    /// </summary>
    public List<LogOrderDetailCrInput> logOrderDetailList { get; set; }

    /// <summary>
    /// 订单财务明细.
    /// </summary>
    public List<LogOrderFinancialCrInput> logOrderFinancialList { get; set; }

    /// <summary>
    /// 订单收款明细.
    /// </summary>
    public List<LogOrderCollectionCrInput> logOrderCollectionList { get; set; }

}