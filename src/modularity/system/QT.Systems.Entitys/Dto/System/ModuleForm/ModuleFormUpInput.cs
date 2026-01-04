using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.ModuleForm;

/// <summary>
/// 功能列修改输入.
/// </summary>
[SuppressSniffer]
public class ModuleFormUpInput : ModuleFormCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }
}