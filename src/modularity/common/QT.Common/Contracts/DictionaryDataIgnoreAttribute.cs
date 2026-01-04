using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Common.Contracts;

/// <summary>
/// 忽略Enum变成字典项
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class DictionaryDataIgnoreAttribute: Attribute { }