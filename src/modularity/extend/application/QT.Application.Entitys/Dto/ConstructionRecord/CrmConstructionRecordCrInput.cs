using QT.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Iot.Application.Dto.CrmConstructionRecord;

public class CrmConstructionRecordCrInput
{
    public string pid { get; set; } 

    public DateTime? constructionTime { get; set; }

    public string content { get; set; }

    public List<FileControlsModel> attachment { get; set; }
}
