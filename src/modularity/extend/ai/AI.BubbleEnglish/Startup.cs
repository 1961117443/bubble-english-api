using AI.BubbleEnglish.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.BubbleEnglish;

public class Startup: AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        IConfiguration configuration = App.Configuration;
        services.AddBubbleEnglishPipeline(configuration);
    }
}
