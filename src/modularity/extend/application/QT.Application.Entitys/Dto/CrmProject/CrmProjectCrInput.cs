using QT.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Iot.Application.Dto.CrmProject;

public class CrmProjectCrInput
{
    public string name { get; set; } 

    public string adminName { get; set; }

    public string adminTel { get; set; }

    //public List<FileControlsModel> attachment { get; set; }
    public string remark { get; set; }
}
