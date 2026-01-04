
using QT.API.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class QTHostedConfigureExtensions
{
    /// <summary>
    /// 添加后台服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddQTHosted(this IServiceCollection services)
    {
        services.AddHostedService<TenantDbContextHostService>();
        return services;
    }
}
