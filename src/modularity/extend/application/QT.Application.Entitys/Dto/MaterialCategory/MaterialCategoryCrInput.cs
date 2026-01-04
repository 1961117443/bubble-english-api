using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Iot.Application.Dto.MaterialCategory;

public class MaterialCategoryCrInput
{
    public string code { get; set; }

    public string name { get; set; }

    public string parentId { get; set; }

    public string remark { get; set; }
}
