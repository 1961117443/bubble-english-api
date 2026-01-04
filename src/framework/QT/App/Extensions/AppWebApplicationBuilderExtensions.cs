using QT;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// WebApplication 拓展
/// </summary>
public static class AppWebApplicationBuilderExtensions
{
    /// <summary>
    /// Web 应用注入
    /// </summary>
    /// <param name="webApplicationBuilder">Web应用构建器</param>
    /// <returns>IWebHostBuilder</returns>
    public static WebApplicationBuilder Inject(this WebApplicationBuilder webApplicationBuilder)
    {
        // 为了兼容 .NET 5 无缝升级至 .NET 6，故传递 WebHost 和 Host
        InternalApp.WebHostEnvironment = webApplicationBuilder.Environment;
        InternalApp.ConfigureApplication(webApplicationBuilder.WebHost, webApplicationBuilder.Host);

        return webApplicationBuilder;
    }

    /// <summary>
    /// 解决 .NET6 WebApplication 模式下二级虚拟目录错误问题
    /// </summary>
    /// <param name="app"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseVirtualPath(this WebApplication app, Action<IApplicationBuilder> configuration)
    {
        if (!string.IsNullOrWhiteSpace(App.Settings.VirtualPath))
        {
            return app.Map(App.Settings.VirtualPath, configuration);
        }

        configuration(app);
        return app;
    }
}