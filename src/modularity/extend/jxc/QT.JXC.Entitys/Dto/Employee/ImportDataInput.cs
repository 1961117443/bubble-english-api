using QT.DependencyInjection;

namespace QT.JXC.Entitys.Dto.Employee;

[SuppressSniffer]
public class ImportDataInput
{
    /// <summary>
    /// 导入数据.
    /// </summary>
    public List<EmployeeListOutput>? list { get; set; }
}
