using QT.Common.Models;
using QT.Logistics.Entitys.Dto.LogEnterpriseAttachment;
using QT.Logistics.Entitys.Dto.LogEnterpriseAuditrecord;
using QT.Logistics.Entitys.Dto.LogEnterpriseFinancial;
using QT.Logistics.Entitys.Dto.LogEnterpriseProduct;
using QT.Logistics.Entitys.Dto.LogEnterpriseSupplyProduct;

namespace QT.Logistics.Entitys.Dto.LogEnterprise;

/// <summary>
/// 入驻商家输出参数.
/// </summary>
public class LogEnterpriseInfoOutput
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
    /// 入驻附件.
    /// </summary>
    public List<LogEnterpriseAttachmentInfoOutput> logEnterpriseAttachmentList { get; set; }

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
    public int? status { get; set; }

    /// <summary>
    /// 入驻商家审批记录.
    /// </summary>
    public List<LogEnterpriseAuditrecordInfoOutput> logEnterpriseAuditrecordList { get; set; }

    /// <summary>
    /// 入驻商家缴费记录.
    /// </summary>
    public List<LogEnterpriseFinancialListOutput> logEnterpriseFinancialList { get; set; }



    /// <summary>
    /// 简图.
    /// </summary>
    public List<FileControlsModel> imageUrl { get; set; }


    /// <summary>
    /// 入驻商家ERP商品记录.
    /// </summary>
    public List<LogEnterpriseProductListOutput> logEnterpriseProductList { get; set; }

    /// <summary>
    /// 入驻商家供应商品记录.
    /// </summary>
    public List<LogEnterpriseSupplyProductListOutput> logEnterpriseSupplyProductList { get; set; }

}