using QT.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Iot.Application.Dto.CrmMarketer;

public class CrmMarketerCrInput
{
    public string userId { get; set; }

    public string managerId { get; set; }

    public int level { get; set; }

    //public int businessCount { get; set; }

    public List<FileControlsModel> attachment { get; set; }
}
