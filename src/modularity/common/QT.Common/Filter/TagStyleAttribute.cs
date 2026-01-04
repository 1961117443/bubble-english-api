using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Common.Filter;

/// <summary>
/// tag样式 目前主要用于字典扩展属性
/// </summary>
//[AttributeUsage( AllowMultiple =false)]
public class TagStyleAttribute :Attribute
{
     

    public TagStyleAttribute(string style)
    {
        Style = style;
    }

    public string Style { get; }
}
