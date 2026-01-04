using QT.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Iot.Application.Dto.CrmUserDelayApply;

public class CrmUserDelayApplyCrInput
{
    public string userId { get; set; } 

    public int status { get; set; }

    public DateTime? expireTime { get; set; }

    //public List<FileControlsModel> attachment { get; set; }
    public string content { get; set; }
}
