using QT.Emp.Entitys.Dto.EmpEmployee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Emp.Interfaces;

public interface IEmpChangeEmployeeService
{
    Task<List<EmpEmployeeChangeLogOutput>> ChangeList(string id);
}
