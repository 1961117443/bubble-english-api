using QT.Common;
using QT.Common.Contracts;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Iot.Application.Entity;

/// <summary>
/// 营销人员
/// </summary>
[SugarTable("crm_marketer")]
[EntityUniqueProperty("UserId")]
public class CrmMarketerEntity : CUDEntityBase
{
    /// <summary>
    /// 用户id
    /// </summary>
    [Description("业务员")]
    [SugarColumn(IsTreeKey = true)]
    public string UserId { get; set; }

    /// <summary>
    /// 直属主管
    /// </summary>
    public string ManagerId { get; set; }

    /// <summary>
    /// 业务状态
    /// </summary>
    public int BusinessCount { get; set; }

    /// <summary>
    /// 营销等级
    /// </summary>
    public MarketLevel Level { get; set; }

    /// <summary>
    /// 附件
    /// </summary>
    public string Attachment { get; set; }
}


/// <summary>
/// 营销账号等级
/// </summary>
public enum MarketLevel
{
    /// <summary>
    /// 部门经理
    /// </summary>
    [Description("部门经理")]
    DivisionManager = 1,

    /// <summary>
    /// 业务主管
    /// </summary>
    [Description("业务主管")]
    BusinessManager = 2,

    /// <summary>
    /// 业务经理
    /// </summary>
    [Description("业务经理")]
    ServiceManager = 3
}