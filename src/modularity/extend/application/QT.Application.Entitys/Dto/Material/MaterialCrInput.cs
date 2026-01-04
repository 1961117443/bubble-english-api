using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Iot.Application.Dto.Material;

public class MaterialCrInput
{
    public string code { get; set; }

    public string name { get; set; }

 
    public string shortName { get; set; }

    public string barCode { get; set; }

    public string categoryId { get; set; }
    public string unit { get; set; }
    public string spec { get; set; }
    public string color { get; set; }
    public string brand { get; set; }
    public decimal? taxRate { get; set; }
    public string size { get; set; }
    public decimal? weight { get; set; }
    public string imageUrl { get; set; }

    public string remark { get; set; }
}
