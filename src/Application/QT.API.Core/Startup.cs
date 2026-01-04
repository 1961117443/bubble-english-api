using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using QT.API.Core.Extensions;
using QT.API.Core.Handlers;
using QT.Common.Cache;
using QT.Common.Configuration;
using QT.Common.Core.Manager.Files;
using QT.Common.Core.Manager.Tenant;
using QT.Common.Core.Security;
using QT.Common.Dtos.OAuth;
using QT.Common.Extension;
using QT.Common.Options;
using QT.Message.Handlers;
using QT.NET.Core;
using Senparc.CO2NET;
using Senparc.CO2NET.RegisterServices;
using Senparc.Weixin;
using Senparc.Weixin.Entities;
using Senparc.Weixin.RegisterServices;
using Serilog;
using SqlSugar;

namespace QT.API.Core;

public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // SqlSugar
        services.SqlSugarConfigure();

        // Jwt处理程序
        services.AddJwt<JwtHandler>(enableGlobalAuthorize: true);

        // 跨域
        services.AddCorsAccessor();

        // 注册远程请求
        services.AddRemoteRequest(options =>
        {
            //options.Appro
            options.AddHttpClient(string.Empty)
            .ConfigurePrimaryHttpMessageHandler(u => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            });

            var iotBaseUrl = App.Configuration["QT_App:IotApi"];
            if (iotBaseUrl.IsNotEmptyOrNull())
            {
                options.AddHttpClient("qt-iot", client =>
                {
                    client.BaseAddress = new Uri(iotBaseUrl);
                });
            }
        });

        services.AddConfigurableOptions<CacheOptions>();
        services.AddConfigurableOptions<ConnectionStringsOptions>();
        services.AddConfigurableOptions<TenantOptions>();

        // 视图引擎
        services.AddViewEngine();

        // 脱敏词汇检测
        services.AddSensitiveDetection();

        services.AddWebSocketManager();

        // 微信
        services.AddSenparcGlobalServices(App.Configuration) // Senparc.CO2NET 全局注册
                    .AddSenparcWeixinServices(App.Configuration); // Senparc.Weixin 注册（如果使用Senparc.Weixin SDK则添加）
        services.AddSession();
        services.AddMemoryCache(); // 使用本地缓存必须添加
        services.AddSenparcWeixinServices(App.Configuration);

        // 注册EventBus服务
        services.AddEventBus(builder =>
        {
            //// 注册 Log 日志订阅者
            //builder.AddSubscriber<LogEventSubscriber>();
            //builder.AddSubscriber<UserEventSubscriber>();
            //builder.AddSubscriber<TaskEventSubscriber>();

            builder.AddSubscribers(App.Assemblies.ToArray());
        });

        services.OSSServiceConfigure();
        services.AddSingleton<IDynamicOSSServiceFactory, DynamicOSSServiceFactory>();

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        // 添加后台服务
        services.AddQTHosted();

        services.AddLazyCaptcha();


        // 数组库租户接口需要特别注入，使用同一个对象
        //services.AddScoped(typeof(ISqlSugarTenant), provider => provider.GetRequiredService<ITenantManager>());

        // 文件管理器 是否使用租户文件管理器
        services.AddScoped(typeof(IFileManager), provider =>
        {
            var tenantManager = provider.GetRequiredService<ITenantManager>();
            if (tenantManager != null && tenantManager.IsLoggedIn && tenantManager.GetTenantInfo() is TenantListInterFaceOutput tenantListInterFaceOutput && tenantListInterFaceOutput != null && tenantListInterFaceOutput.ossConfig != null)
            {

                return provider.GetRequiredService(typeof(TenantOSSFileManager));
            }
            else
            {
                return provider.GetRequiredService(typeof(FileManager));
            }
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<SenparcSetting> senparcSetting, IOptions<SenparcWeixinSetting> senparcWeixinSetting)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        // NGINX 反向代理获取真实IP
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        // 添加状态码拦截中间件
        app.UseUnifyResultStatusCodes();

        app.UseHttpsRedirection(); // 强制https
        app.UseStaticFiles();

        // 微信
        IRegisterService register = RegisterService.Start(senparcSetting.Value).UseSenparcGlobal(); // 启动 CO2NET 全局注册，必须！
        register.UseSenparcWeixin(senparcWeixinSetting.Value, senparcSetting.Value); // 微信全局注册,必须！

        app.UseWebSockets();

        // Serilog请求日志中间件---必须在 UseStaticFiles 和 UseRouting 之间
        app.UseSerilogRequestLogging();

        app.UseRouting();

        app.UseCorsAccessor();

        app.UseAuthentication();

        // 验证权限之后
        if (KeyVariable.MultiTenancy)
        {
            app.UseTenant();
        }

        app.UseAuthorization();

       

        app.UseInject(string.Empty);

        app.MapWebSocketManager("/api/message/websocket", app.ApplicationServices.GetService<IMHandler>());

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
        });

        SnowflakeIdHelper.initIdWorker();

        try
        {
#if !DEBUG
            new Aspose.Words.License().SetLicense("Aspose.Total.NET.lic");
#endif
        }
        catch (Exception)
        {
        }
    }
}