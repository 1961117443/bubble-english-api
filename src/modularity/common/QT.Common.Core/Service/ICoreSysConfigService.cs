using QT.Systems.Entitys.Dto.SysConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Common.Core.Service;

public interface ICoreSysConfigService
{
    /// <summary>
    /// 获取系统配置信息
    /// </summary>
    /// <returns></returns>
    Task<SysConfigOutput> GetSysConfig();

    /// <summary>
    /// 获取配置
    /// </summary>
    /// <typeparam name="TConfig"></typeparam>
    /// <returns></returns>
    Task<TConfig> GetConfig<TConfig>() where TConfig : class, new();

    /// <summary>
    /// 获取配置
    /// </summary>
    /// <typeparam name="TConfig"></typeparam>
    /// <param name="category"></param>
    /// <returns></returns>
    Task<TConfig> GetConfigWithCategory<TConfig>(string category) where TConfig : class, new();
}
