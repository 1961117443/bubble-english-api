using QT.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Iot.Application.Dto.CrmFinanceRecord;

public class CrmFinanceRecordCrInput
{
    public string pid { get; set; } 

    public DateTime? financeTime { get; set; }

    public string content { get; set; }

    public List<FileControlsModel> attachment { get; set; }

    public decimal amount { get; set; }
}
