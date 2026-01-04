namespace QT.Emp.Interfaces;

/// <summary>
/// 业务抽象：员工档案.
/// </summary>
public interface IEmpEmployeeService
{
    Task Delete(string id);
}