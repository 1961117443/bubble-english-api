using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.Department;

/// <summary>
/// 部门修改输入.
/// </summary>
[SuppressSniffer]
public class DepartmentUpInput : DepartmentCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }
}