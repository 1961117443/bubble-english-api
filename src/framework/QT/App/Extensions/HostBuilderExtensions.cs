using QT;
using QT.Extensions;
using QT.Reflection;
using Microsoft.AspNetCore.Hosting;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// 主机构建器拓展类
/// </summary>
[SuppressSniffer]
public static class HostBuilderExtensions
{
    /// <summary>
    /// Web 主机注入
    /// </summary>
    /// <param name="hostBuilder">Web主机构建器</param>
    /// <param name="assemblyName">外部程序集名称</param>
    /// <returns>IWebHostBuilder</returns>
    public static IWebHostBuilder Inject(this IWebHostBuilder hostBuilder, string assemblyName = default)
    {
        // 获取默认程序集名称
        var defaultAssemblyName = assemblyName ?? Reflect.GetAssemblyName(typeof(HostBuilderExtensions));

        //  获取环境变量 ASPNETCORE_HOSTINGSTARTUPASSEMBLIES 配置
        var environmentVariables = Environment.GetEnvironmentVariable("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES");
        var combineAssembliesName = $"{defaultAssemblyName};{environmentVariables}".ClearStringAffixes(1, ";");

        hostBuilder.UseSetting(WebHostDefaults.HostingStartupAssembliesKey, combineAssembliesName);
        return hostBuilder;
    }

    /// <summary>
    /// 泛型主机注入
    /// </summary>
    /// <param name="hostBuilder">泛型主机注入构建器</param>
    /// <param name="autoRegisterBackgroundService">是否自动注册 BackgroundService</param>
    /// <returns>IWebHostBuilder</returns>
    public static IHostBuilder Inject(this IHostBuilder hostBuilder, bool autoRegisterBackgroundService = true)
    {
        InternalApp.ConfigureApplication(hostBuilder, autoRegisterBackgroundService);

        return hostBuilder;
    }
}