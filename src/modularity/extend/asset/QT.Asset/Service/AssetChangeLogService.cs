using Microsoft.AspNetCore.Mvc;
using QT.Asset.Dto.AssetChangeLog;
using QT.Asset.Entity;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Dtos.DataBase;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.DependencyInjection;
using QT.DynamicApiController;
using SqlSugar;
using System.Threading.Channels;

namespace QT.Asset;

/// <summary>
/// 资产变更日志服务
/// </summary>
[ApiDescriptionSettings("资产管理", Tag = "系统日志", Name = "AssetChangeLog", Order = 612)]
[Route("api/extend/asset/[controller]")]
public class AssetChangeLogService : /*QTBaseService<AssetChangeLogEntity, AssetChangeLogCrInput, AssetChangeLogUpInput, AssetChangeLogDto, AssetChangeLogListPageInput, AssetChangeLogListOutput>,*/ IDynamicApiController, IScoped
{
    private readonly ISqlSugarRepository<AssetChangeLogEntity> _repository;

    public AssetChangeLogService(
        ISqlSugarRepository<AssetChangeLogEntity> repository)
    {
        _repository = repository;
    }


    /// <summary>
    /// 记录资产变更日志
    /// </summary>
    /// <param name="assetId">资产id</param>
    /// <param name="changeFrom">变更来源类型</param>
    /// <param name="changes">变更字段</param>
    /// <param name="operatorId"></param>
    /// <param name="operatorName"></param>
    /// <param name="taskId"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    [NonAction]
    public async Task LogAssetChangesAsync(string assetId, List<FieldChangeDto> changes, string? operatorId = null, string? operatorName = null, string? taskId = null, string? reason = null)
    {
        if (!changes.IsAny())
        {
            return;
        }
        var logs = new List<AssetChangeLogEntity>();
        foreach (var item in changes)
        {
            logs.Add(new AssetChangeLogEntity
            {
                AssetId = assetId,
                ChangeFrom = item.tableName,
                TaskId = taskId ?? "",
                ChangeReason = reason ?? "",
                OperatorId = operatorId ?? "",
                OperatorName = operatorName ?? "",
                ChangeTime = DateTime.Now,
                FieldName = item.fieldName,
                NewValue = item.newValue,
                OldValue = item.oldValue,
                FieldTitle = item.description ?? ""
            });
        }
        if (logs.IsAny())
        {
            await _repository.Context.Insertable(logs).ExecuteCommandAsync();
        }

    }

    /// <summary>
    /// 记录资产变更日志
    /// </summary>
    /// <param name="assetId">资产id</param>
    /// <param name="changeFrom">变更来源类型</param>
    /// <param name="changes">变更字段</param>
    /// <param name="operatorId"></param>
    /// <param name="operatorName"></param>
    /// <param name="taskId"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    [NonAction]
    public async Task LogAssetChangesAsync(Dictionary<string, List<FieldChangeDto>> changesDict, string? operatorId = null, string? operatorName = null, string? taskId = null, string? reason = null)
    {
        if (!changesDict.IsAny())
        {
            return;
        }
        var logs = new List<AssetChangeLogEntity>();
        foreach (var c in changesDict)
        {
            var assetId = c.Key;
            foreach (var item in c.Value)
            {
                logs.Add(new AssetChangeLogEntity
                {
                    AssetId = assetId,
                    ChangeFrom = item.tableName,
                    TaskId = taskId ?? "",
                    ChangeReason = reason ?? "",
                    OperatorId = operatorId ?? "",
                    OperatorName = operatorName ?? "",
                    ChangeTime = DateTime.Now,
                    FieldName = item.fieldName,
                    NewValue = item.newValue,
                    OldValue = item.oldValue,
                    FieldTitle = item.description ?? ""
                });
            }
        }
        if (logs.IsAny())
        {
            await _repository.Context.Insertable(logs).ExecuteCommandAsync();
        }

    }

    /// <summary>
    /// 根据任务id，批量删除变更记录
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    [NonAction]
    public async Task RemoveAssetChangesAsync(string taskId)
    {
        if (taskId.IsNotEmptyOrNull())
        {
            await _repository.Context.Updateable<AssetChangeLogEntity>()
                .SetColumns(it => new AssetChangeLogEntity() { DeleteTime = DateTime.Now })
                .Where(it => it.TaskId == taskId)
                .ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 获取资产变更日志列表
    /// </summary>
    /// <param name="assetId"></param>
    /// <returns></returns>
    [HttpGet("{assetId}/list")]
    public async Task<List<AssetChangeLogDto>> GetAssetChangeLogList(string assetId)
    {
        return await _repository.Context.Queryable<AssetChangeLogEntity>()
            .Where(it => it.AssetId == assetId)
            .OrderBy(it => it.ChangeTime)
            .Select<AssetChangeLogDto>()
            .ToListAsync();
    }
}