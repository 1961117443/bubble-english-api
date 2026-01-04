using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.ModuleDataAuthorize;

/// <summary>
/// 功能权限数据修改输入.
/// </summary>
[SuppressSniffer]
public class ModuleDataAuthorizeUpInput : ModuleDataAuthorizeCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }
}
