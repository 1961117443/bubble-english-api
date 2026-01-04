using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Application.Entitys.Enum;

[Description("工作日志类型")]
public enum WorkLogCategory
{
    [Description("日报")]
    Day=1,
    [Description("周报")]
    Week=2,
    [Description("月报")]
    Month=3
}

[Description("工单类型")]
public enum WorkOrderCategory
{
    [Description("个人")]
    Personal = 1,

    [Description("团队")]
    Team = 2,

    [Description("项目")]
    Project = 3,
}