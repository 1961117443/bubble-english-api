using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Employee.Entitys.Dto.EmpChangeEmployee;

public class ChangeSalaryForm
{
    public decimal lastSalary { get; set; }
    public string lastSalaryType { get; set; }
    public string lastSalaryRemark { get; set; }
    public decimal salary { get; set; }
    public string salaryType { get; set; }
    public string salaryRemark { get; set; }
}
