using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Employee.Entitys.Dto.EmpChangeEmployee;

/// <summary>
/// 员工档案变更类型.
/// </summary>
public enum ChangeType
{
    [Description("通用变更")]
    Common,
    [Description("薪资变更")]
    Salary
}
