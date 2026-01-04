using QT.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JXC.Entitys.Dto;

[SuppressSniffer]
public abstract class BaseImportDataInput
{
    public virtual string ErrorMessage { get; set; }
}
