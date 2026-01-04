
using System.Security.AccessControl;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseAuditrecord;

/// <summary>
/// 入驻商家审批记录修改输入参数.
/// </summary>
public class LogEnterpriseAuditrecordCrInput
{
    /// <summary>
    /// 审批时间.
    /// </summary>
    public DateTime? auditTime { get; set; }

    /// <summary>
    /// 审批用户.
    /// </summary>
    public string auditUserId { get; set; }

    /// <summary>
    /// 审批意见.
    /// </summary>
    public string remark { get; set; }


    /// <summary>
    /// id
    /// </summary>
    public string id { get; set; }
}


public class LogEnterpriseAuditrecordInput
{
    /// <summary>
    /// 审核状态 
    /// 0：不通过，1：通过
    /// </summary>
    public int status { get; set; }

    /// <summary>
    /// 审批意见.
    /// </summary>
    public string remark { get; set; }


    /// <summary>
    /// id
    /// </summary>
    public string id { get; set; }
}