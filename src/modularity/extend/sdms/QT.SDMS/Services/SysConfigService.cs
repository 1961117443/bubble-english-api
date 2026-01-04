using Microsoft.AspNetCore.Mvc;
using QT.Common;
using QT.Common.Core.Security;
using QT.Common.Options;
using QT.Common.Security;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Logging.Attributes;
using QT.SDMS.Entitys.Dto.SysConfig;
using QT.Systems.Entitys.Dto.SysConfig;
using QT.Systems.Entitys.System;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.SDMS;

/// <summary>
/// 业务实现：营销管理-用户订单（我的部门）.
/// </summary>
[ApiDescriptionSettings("售电系统", Tag = "销售配置", Name = "SysConfig", Order = 300)]
[Route("api/sdms/sysconfig")]
public class SysConfigService: IDynamicApiController
{
    private readonly ISqlSugarRepository<SysConfigEntity> _sysConfigRepository;

    public SysConfigService(ISqlSugarRepository<SysConfigEntity> sysConfigRepository)
    {
        _sysConfigRepository = sysConfigRepository;
    }



    /// <summary>
    /// 获取系统配置.
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<SdmsConfigOutput> GetInfo()
    {
        var propertyList = EntityHelper<SdmsConfigOutput>.InstanceProperties.Select(p => p.Name).ToArray();
        var array = new Dictionary<string, string>();
        var data = await _sysConfigRepository.Where(x => propertyList.Contains(x.Key) && x.Category.Equals("SdmsConfig")).ToListAsync();
        foreach (var item in data)
        {
            array.Add(item.Key, item.Value);
        }
        return array.ToObject<SdmsConfigOutput>();

    }

    /// <summary>
    /// 更新系统配置.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPut]
    [SqlSugarUnitOfWork]
    [OperateLog("售电系统-销售配置", "更改销售提点")]
    public async Task Update([FromBody] SdmsConfigOutput input)
    {
        var configDic = input.ToObject<Dictionary<string, object>>();
        var entitys = new List<SysConfigEntity>();
        foreach (var item in configDic.Keys)
        {
            if (configDic[item] != null)
            {

                SysConfigEntity sysConfigEntity = new SysConfigEntity();
                sysConfigEntity.Id = SnowflakeIdHelper.NextId();
                sysConfigEntity.Key = item;
                sysConfigEntity.Value = configDic[item].ToString();
                sysConfigEntity.Category = "SdmsConfig";
                entitys.Add(sysConfigEntity);
            }
        }

        await _sysConfigRepository.DeleteAsync(x => x.Category.Equals("SdmsConfig"));
        await _sysConfigRepository.InsertAsync(entitys);
    }
}
