using Microsoft.AspNetCore.Mvc;
using QT.Common.Core.Manager;
using QT.Common.Core.Manager.Parameter;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Logging.Attributes;
using QT.Systems.Entitys.Dto.Module;
using QT.Systems.Entitys.Dto.System.ModuleRemind;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.System;
using SqlSugar;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace QT.Systems;

/// <summary>
/// 模块提醒功能
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "ModuleRemind", Order = 200)]
[Route("api/system/[controller]")]
public class ModuleRemindService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ModuleRemindEntity> _repository;

    ///// <summary>
    ///// 数据字典服务.
    ///// </summary>
    //private readonly IDictionaryDataService _dictionaryDataService;

    ///// <summary>
    ///// 数据连接服务.
    ///// </summary>
    //private readonly IDbLinkService _dbLinkService;

    ///// <summary>
    ///// 文件服务.
    ///// </summary>
    //private readonly IFileManager _fileManager;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly IModuleService _moduleService;
    private readonly IParameterManager _parameterManager;

    ///// <summary>
    ///// 数据库管理.
    ///// </summary>
    //private readonly IDataBaseManager _dataBaseManager;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    ///// <summary>
    ///// 系统参数
    ///// </summary>
    //private static Dictionary<string, Func<object>> _sysParameters = new();
    

    /// <summary>
    /// 初始化一个<see cref="ModuleRemindService"/>类型的新实例.
    /// </summary>
    public ModuleRemindService(
        ISqlSugarRepository<ModuleRemindEntity> ModuleRemindRepository,
        //IDictionaryDataService dictionaryDataService,
        //IFileManager fileManager,
        //IDataBaseManager dataBaseManager,
        IUserManager userManager,
        //IDbLinkService dbLinkService,
        ISqlSugarClient context,
        IModuleService moduleService,
        IParameterManager parameterManager)
    {
        _repository = ModuleRemindRepository;
        //_dictionaryDataService = dictionaryDataService;
        //_dbLinkService = dbLinkService;
        //_fileManager = fileManager;
        //_dataBaseManager = dataBaseManager;
        _userManager = userManager;
        _moduleService = moduleService;
        _parameterManager = parameterManager;
        _db = context.AsTenant();
    }
     

    /// <summary>
    /// 获取打印模块关联的模块.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("Actions/UserData")]
    [IgnoreLog]
    public async Task<Dictionary<string, ModuleRemindOutput>> ActionsUserData()
    {
        var modules = ((await _moduleService.GetUserModueList("Web") as List<ModuleOutput>) ?? new List<ModuleOutput>()).Select(x => x.id).ToArray();
        var list = await _repository.Where(x => x.EnabledMark == 1).ToListAsync();
        //var list = await _repository
        //    .Where(it => modules.Contains(it.ModuleId))
        //    .ToListAsync();
        list = list.Where(it => modules.Contains(it.ModuleId)).ToList();

        // 获取模块集合
        var moduleList = await _repository.Context.Queryable<ModuleEntity>().Select(x => new ModuleEntity { Id = x.Id, ParentId = x.ParentId }).ToListAsync();

        Dictionary<string, ModuleRemindOutput> result = new Dictionary<string, ModuleRemindOutput>();

        // 匹配自定义的变量 {}
        string pattern = @"\{([^\{\}]+)\}";
        foreach (var module in list)
        {
            if (string.IsNullOrEmpty(module.SqlTemplate))
            {
                continue;
            }
            var sql = module.SqlTemplate;
            // 解析sql ,查询数据
            List<SugarParameter> parameter = new List<SugarParameter>();
            // 匹配自定义的变量 @[^\s]+\s?            
            foreach (Match match in Regex.Matches(sql, pattern))
            {
                var p = match.Groups[1].Value;
                if (!string.IsNullOrEmpty(p))
                {
                    // 判断是否@开头，统一整理成@开头的变量
                    if (!p.StartsWith("@"))
                    {
                        p = $"@{p}";
                    }

                    if (!parameter.Any(x => x.ParameterName == p))
                    {
                        parameter.Add(new SugarParameter(p, await _parameterManager.GetParameterValue(p)));
                    }

                    sql = Regex.Replace(sql, match.Groups[0].Value, p);
                }
            }

            // 查询数据
            var num = await _repository.Context.Ado.SqlQuerySingleAsync<int>(sql, parameter);
            if (num>0)
            {
                result.TryAdd(module.ModuleId, new ModuleRemindOutput
                {
                    num = num,
                    isDot = num > 99
                });
            }

            foreach (var item in result.Keys.ToArray())
            {
                //var parents = _repository.Context.Queryable<ModuleEntity>().ToParentList(it => it.ParentId, item);
                var parents = GetParentList(moduleList, item);
                foreach (var parent in parents)
                {
                    if (!result.ContainsKey(parent.Id))
                    {
                        result.Add(parent.Id, new ModuleRemindOutput { isDot = true });
                    }
                }
            }



        }
        return result;
    }


    #region Private Method
    /// <summary>
    /// 根据keyValue，获取所有上级菜单
    /// </summary>
    /// <param name="list"></param>
    /// <param name="keyValue"></param>
    /// <returns></returns>
    private List<ModuleEntity> GetParentList(List<ModuleEntity> list, string keyValue)
    {
        List<ModuleEntity> result = new List<ModuleEntity>();
        int i = 0;
        do
        {
            i++;
            var item = list.Find(x => x.Id == keyValue);
            if (item == null)
            {
                break;
            }
            result.Add(item);
            keyValue = item.ParentId;

            if (i>1000)
            {
                // 防止死循环
                break;
            }
        } while (true);

        return result;
    } 
    #endregion
}