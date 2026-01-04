using QT.Common.Configuration;
using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Core.Manager.Files;
using QT.Common.Core.Security;
using QT.Common.Extension;
using QT.Common.Models;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.WorkFlow.Entitys;
using QT.WorkFlow.Entitys.Dto.WorkFlowForm.SalesOrder;
using QT.WorkFlow.Entitys.Model.Item;
using QT.WorkFlow.Interfaces.Manager;
using QT.WorkFlow.Interfaces.Service;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Yitter.IdGenerator;

namespace QT.WorkFlow.WorkFlowForm;

/// <summary>
/// 销售订单



/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "WorkflowForm", Name = "SalesOrder", Order = 532)]
[Route("api/workflow/Form/[controller]")]
public class SalesOrderService : ISalesOrderService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<SalesOrderEntity> _sqlSugarRepository;
    private readonly ICacheManager _cacheManager;
    private readonly IFileManager _fileManager;
    private readonly IUserManager _userManager;
    private readonly IFlowTaskManager _flowTaskManager;
    private readonly ITenant _db;

    public SalesOrderService(
        ISqlSugarRepository<SalesOrderEntity> sqlSugarRepository,
        ICacheManager cacheManager,
        IFileManager fileManager,
        IUserManager userManager,
        IFlowTaskManager flowTaskManager,
        ISqlSugarClient context)
    {
        _sqlSugarRepository = sqlSugarRepository;
        _cacheManager = cacheManager;
        _fileManager = fileManager;
        _userManager = userManager;
        _flowTaskManager = flowTaskManager;
        _db = context.AsTenant();
    }

    #region GET

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键值</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var data = (await _sqlSugarRepository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<SalesOrderInfoOutput>();
        data.entryList = (await _sqlSugarRepository.Context.Queryable<SalesOrderEntryEntity>().Where(x => x.SalesOrderId == id).ToListAsync()).Adapt<List<EntryListItem>>();
        return data;
    }

    #endregion

    #region POST

    /// <summary>
    /// 保存.
    /// </summary>
    /// <param name="input">表单信息.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Save([FromBody] SalesOrderCrInput input)
    {
        var entity = input.Adapt<SalesOrderEntity>();
        var entityList = input.entryList.Adapt<List<SalesOrderEntryEntity>>();
        if (input.status == 1)
        {
            await Save(entity.Id, entity, entityList);
        }
        else
        {
            await Submit(entity.Id, entity, entityList, input.candidateList);
        }
    }

    /// <summary>
    /// 提交.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <param name="input">表单信息.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Submit(string id, [FromBody] SalesOrderUpInput input)
    {
        input.id = id;
        var entity = input.Adapt<SalesOrderEntity>();
        var entityList = input.entryList.Adapt<List<SalesOrderEntryEntity>>();
        if (input.status == 1)
        {
            await Save(entity.Id, entity, entityList);
        }
        else
        {
            await Submit(entity.Id, entity, entityList, input.candidateList);
        }

    }

    #endregion

    #region PrivateMethod

    /// <summary>
    /// 保存.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="entity"></param>
    /// <param name="itemList"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    private async Task Save(string id, SalesOrderEntity entity, List<SalesOrderEntryEntity> itemList, int type = 0)
    {
        try
        {
            _db.BeginTran();

            #region 表单信息
            await HandleForm(id, entity, itemList);
            #endregion

            #region 流程信息
            await _flowTaskManager.Save(id, entity.FlowId, entity.Id, entity.FlowTitle, entity.FlowUrgent, entity.BillNo, null, 1, type, true);
            #endregion

            _db.CommitTran();
        }
        catch (Exception ex)
        {
            _db.RollbackTran();
            throw;
        }
    }

    /// <summary>
    /// 提交.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="entity"></param>
    /// <param name="itemList"></param>
    /// <returns></returns>
    private async Task Submit(string id, SalesOrderEntity entity, List<SalesOrderEntryEntity> itemList, Dictionary<string, List<string>> candidateList)
    {
        try
        {
            _db.BeginTran();

            #region 表单信息

            await HandleForm(id, entity, itemList);

            #endregion

            #region 流程信息
            await _flowTaskManager.Submit(id, entity.FlowId, entity.Id, entity.FlowTitle, entity.FlowUrgent, entity.BillNo, entity.Adapt<SalesOrderUpInput>(), 0, 0, true, false, candidateList);
            #endregion

            _db.CommitTran();

        }
        catch (Exception ex)
        {
            _db.RollbackTran();
            throw;
        }
    }

    /// <summary>
    /// 表单操作.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="entity"></param>
    /// <param name="itemList"></param>
    /// <returns></returns>
    private async Task HandleForm(string id, SalesOrderEntity entity, List<SalesOrderEntryEntity> itemList)
    {
        if (string.IsNullOrEmpty(id))
        {
            entity.Id = YitIdHelper.NextId().ToString();
            foreach (var item in itemList)
            {
                item.Id = SnowflakeIdHelper.NextId();
                item.SalesOrderId = entity.Id;
                item.SortCode = itemList.IndexOf(item);
            }
            await _sqlSugarRepository.Context.Insertable(itemList).ExecuteCommandAsync();
            await _sqlSugarRepository.InsertAsync(entity);
            _cacheManager.Del(string.Format("{0}{1}_{2}", CommonConst.CACHEKEYBILLRULE, _userManager.TenantId, _userManager.UserId + "WF_SalesOrderNo"));
        }
        else
        {
            entity.Id = id;
            foreach (var item in itemList)
            {
                item.Id = SnowflakeIdHelper.NextId();
                item.SalesOrderId = entity.Id;
                item.SortCode = itemList.IndexOf(item);
            }

            await _sqlSugarRepository.Context.Deleteable<SalesOrderEntryEntity>(x => x.SalesOrderId == id).ExecuteCommandAsync();
            await _sqlSugarRepository.Context.Insertable(itemList).ExecuteCommandAsync();
            await _sqlSugarRepository.UpdateAsync(entity);
            foreach (var item in entity.FileJson.ToList<AnnexModel>())
            {
                if (item.IsNotEmptyOrNull() && item.FileType == "delete")
                {
                    await _fileManager.DeleteFile(Path.Combine(FileVariable.SystemFilePath , item.FileName));
                }
            }
        }
    }

    #endregion

    #region PublicMethod

    /// <summary>
    /// 工作流表单操作.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="obj"></param>
    /// <param name="type">0：事前审批，1：创建子流程.</param>
    /// <returns></returns>
    [NonAction]
    public async Task Save(string id, object obj, int type)
    {
        try
        {
            var input = obj.ToObject<SalesOrderUpInput>();
            var entity = input.Adapt<SalesOrderEntity>();
            var entityList = input.entryList.Adapt<List<SalesOrderEntryEntity>>();
            if (type == 0)
            {
                await this.HandleForm(id, entity, entityList);
            }
            else
            {
                entity.Id = id;
                foreach (var item in entityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.SalesOrderId = entity.Id;
                    item.SortCode = entityList.IndexOf(item);
                }
                await _sqlSugarRepository.Context.Insertable(entityList).ExecuteCommandAsync();
                await _sqlSugarRepository.InsertAsync(entity);
            }
        }
        catch (Exception e)
        {

            throw;
        }
    }

    #endregion
}