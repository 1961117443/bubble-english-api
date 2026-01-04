using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Common.Contracts;

/// <summary>
/// 流程实体接口
/// </summary>
public interface IFlowEntity
{
    /// <summary>
    /// 流程主键.
    /// </summary>
    string FlowId { get; set; }

    /// <summary>
    /// 流程标题.
    /// </summary>
    string FlowTitle { get; set; }

    /// <summary>
    /// 流程等级.
    /// </summary>
    int? FlowUrgent { get; set; }
}
