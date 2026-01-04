using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Logistics.Entitys.Dto.LogEnterprise;
using QT.Logistics.Entitys.Dto.LogEnterpriseAuditrecord;
using QT.Logistics.Entitys;
using QT.Logistics.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Logistics.Entitys.Dto.LogEnterpriseAttachment;
using QT.Systems.Entitys.Permission;
using QT.Logistics.Entitys.Dto.LogStoreroom;
using QT.Systems.Interfaces.System;
using QT.Logistics.Entitys.Dto.LogEnterpriseFinancial;
using QT.Logistics.Entitys.Dto.LogEnterpriseSupplyProduct;
using QT.Logistics.Entitys.Dto.LogEnterpriseProduct;

namespace QT.Logistics;

/// <summary>
/// 业务实现：入驻商家.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "入驻商家管理", Name = "LogEnterprise", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogEnterpriseService : ILogEnterpriseService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogEnterpriseEntity> _repository;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly IDictionaryDataService _dictionaryDataService;

    /// <summary>
    /// 初始化一个<see cref="LogEnterpriseService"/>类型的新实例.
    /// </summary>
    public LogEnterpriseService(
        ISqlSugarRepository<LogEnterpriseEntity> logEnterpriseRepository,
        ISqlSugarClient context,
        IUserManager userManager,
        IDictionaryDataService dictionaryDataService)
    {
        _repository = logEnterpriseRepository;
        _db = context.AsTenant();
        _userManager = userManager;
        _dictionaryDataService = dictionaryDataService;
    }

    #region 增删改查
    /// <summary>
    /// 获取入驻商家.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LogEnterpriseInfoOutput>();

        var logEnterpriseAuditrecordList = await _repository.Context.Queryable<LogEnterpriseAuditrecordEntity>().Where(w => w.EId == output.id).ToListAsync();
        output.logEnterpriseAuditrecordList = logEnterpriseAuditrecordList.Adapt<List<LogEnterpriseAuditrecordInfoOutput>>();

        var logEnterpriseAttachmentList = await _repository.Context.Queryable<LogEnterpriseAttachmentEntity>().Where(w => w.EId == output.id).ToListAsync();
        output.logEnterpriseAttachmentList = logEnterpriseAttachmentList.Adapt<List<LogEnterpriseAttachmentInfoOutput>>();

        output.logEnterpriseFinancialList = await _repository.Context.Queryable<LogEnterpriseFinancialEntity>().Where(w => w.EId == output.id).Select<LogEnterpriseFinancialListOutput>().ToListAsync();

        output.logEnterpriseSupplyProductList = await _repository.Context.Queryable<LogEnterpriseSupplyProductEntity>()
            .Where(it => it.EId == id)
            .Select(it => new LogEnterpriseSupplyProductListOutput
            {
                id = it.Id,
                tid = it.Tid,
                name = it.Name,
                firstChar = it.FirstChar,
                producer = it.Producer,
                remark = it.Remark,
                storage = it.Storage,
                retention = it.Retention,
                state = it.State ?? 0,
                tidName = SqlFunc.Subqueryable<LogEnterpriseProducttypeEntity>().Where(x => x.Id == it.Tid).Select(x => x.Name)
            })
            .ToListAsync();

        output.logEnterpriseProductList = await _repository.Context.Queryable<LogEnterpriseProductEntity>()
            .Where(it => it.EId == id)
            .Select(it => new LogEnterpriseProductListOutput
            {
                id = it.Id,
                tid = it.Tid,
                name = it.Name,
                firstChar = it.FirstChar,
                producer = it.Producer,
                remark = it.Remark,
                storage = it.Storage,
                retention = it.Retention,
                state = it.State ?? 0,
                tidName = SqlFunc.Subqueryable<LogEnterpriseProducttypeEntity>().Where(x => x.Id == it.Tid).Select(x => x.Name)
            })
            .ToListAsync();

        return output;
    }

    /// <summary>
    /// 获取入驻商家列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogEnterpriseListQueryInput input)
    {
        var data = await _repository.Context.Queryable<LogEnterpriseEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.phone), it => it.Phone.Contains(input.phone))
            .WhereIF(!string.IsNullOrEmpty(input.leader), it => it.Leader.Contains(input.leader))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                || it.Phone.Contains(input.keyword)
                || it.AdminId.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogEnterpriseListOutput
            {
                id = it.Id,
                name = it.Name,
                phone = it.Phone,
                adminId = it.AdminId,
                status = it.Status ?? 0,
                leader = it.Leader,
                adminIdName = SqlFunc.Subqueryable<UserEntity>().Where(x => x.Id == it.AdminId).Select(x => x.RealName),
                account = SqlFunc.Subqueryable<UserEntity>().Where(x => x.Id == it.AdminId).Select(x => x.Account)
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogEnterpriseListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建入驻商家.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogEnterpriseCrInput input)
    {
        var entity = input.Adapt<LogEnterpriseEntity>();
        entity.Id = SnowflakeIdHelper.NextId();

        // 判断名称是否重复
        if (await _repository.Where(it => it.Name == entity.Name).AnyAsync())
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }

        try
        {
            // 开启事务
            _db.BeginTran();

            var newEntity = await _repository.Context.Insertable<LogEnterpriseEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            #region 订单附件
            var logEnterpriseAttachmentList = input.logEnterpriseAttachmentList.Adapt<List<LogEnterpriseAttachmentEntity>>();
            if (logEnterpriseAttachmentList != null)
            {
                foreach (var item in logEnterpriseAttachmentList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.EId = newEntity.Id;
                    item.UploadTime = DateTime.Now;
                }
                await _repository.Context.Insertable<LogEnterpriseAttachmentEntity>(logEnterpriseAttachmentList).ExecuteCommandAsync();
            }
            #endregion

            #region 审批记录
            //var logEnterpriseAuditrecordEntityList = input.logEnterpriseAuditrecordList.Adapt<List<LogEnterpriseAuditrecordEntity>>();
            //if (logEnterpriseAuditrecordEntityList != null)
            //{
            //    foreach (var item in logEnterpriseAuditrecordEntityList)
            //    {
            //        item.Id = SnowflakeIdHelper.NextId();
            //        item.EId = newEntity.Id;
            //    }

            //    await _repository.Context.Insertable<LogEnterpriseAuditrecordEntity>(logEnterpriseAuditrecordEntityList).ExecuteCommandAsync();
            //}
            #endregion

            // 清空缓存
            _repository.Context.DataCache.RemoveDataCache(nameof(LogEnterpriseEntity));

            // 关闭事务
            _db.CommitTran();
        }
        catch (Exception)
        {
            // 回滚事务
            _db.RollbackTran();

            throw Oops.Oh(ErrorCode.COM1000);
        }
    }

    /// <summary>
    /// 更新入驻商家.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogEnterpriseUpInput input)
    {
        var entity = input.Adapt<LogEnterpriseEntity>();
        // 判断名称是否重复
        if (await _repository.Where(it => it.Name == entity.Name && it.Id != entity.Id).AnyAsync())
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }
        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.Context.Updateable<LogEnterpriseEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

            //// 清空入驻商家审批记录原有数据
            //await _repository.Context.CUDSaveAsnyc<LogEnterpriseAuditrecordEntity>(it => it.EId == entity.Id, input.logEnterpriseAuditrecordList, it => it.EId = entity.Id);

            // 清空入驻商家附件记录原有数据
            await _repository.Context.CUDSaveAsnyc<LogEnterpriseAttachmentEntity>(it => it.EId == entity.Id, input.logEnterpriseAttachmentList, it => it.EId = entity.Id);


            // 清空缓存
            _repository.Context.DataCache.RemoveDataCache(nameof(LogEnterpriseEntity));

            //// 清空入驻商家审批记录原有数据
            //await _repository.Context.Deleteable<LogEnterpriseAuditrecordEntity>().Where(it => it.EId == entity.Id).ExecuteCommandAsync();

            //// 新增入驻商家审批记录新数据
            //var logEnterpriseAuditrecordEntityList = input.logEnterpriseAuditrecordList.Adapt<List<LogEnterpriseAuditrecordEntity>>();
            //if (logEnterpriseAuditrecordEntityList != null)
            //{
            //    foreach (var item in logEnterpriseAuditrecordEntityList)
            //    {
            //        item.Id ??= SnowflakeIdHelper.NextId();
            //        item.EId = entity.Id;
            //    }

            //    await _repository.Context.Insertable<LogEnterpriseAuditrecordEntity>(logEnterpriseAuditrecordEntityList).ExecuteCommandAsync();
            //}

            //// 清空入驻商家附件记录原有数据
            //await _repository.Context.Deleteable<LogEnterpriseAttachmentEntity>().Where(it => it.EId == entity.Id).ExecuteCommandAsync();

            //// 新增入驻商家附件记录新数据
            //var logEnterpriseAttachmentList = input.logEnterpriseAttachmentList.Adapt<List<LogEnterpriseAttachmentEntity>>();
            //if (logEnterpriseAttachmentList != null)
            //{
            //    foreach (var item in logEnterpriseAttachmentList)
            //    {
            //        item.Id ??= SnowflakeIdHelper.NextId();
            //        item.EId = entity.Id;
            //    }

            //    await _repository.Context.Insertable<LogEnterpriseAttachmentEntity>(logEnterpriseAttachmentList).ExecuteCommandAsync();
            //}

            // 关闭事务
            _db.CommitTran();
        }
        catch (Exception)
        {
            // 回滚事务
            _db.RollbackTran();
            throw Oops.Oh(ErrorCode.COM1001);
        }
    }

    /// <summary>
    /// 删除入驻商家.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        if (!await _repository.Context.Queryable<LogEnterpriseEntity>().AnyAsync(it => it.Id == id))
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }

        try
        {
            // 开启事务
            _db.BeginTran();

            var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
            await _repository.Context.Deleteable<LogEnterpriseEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

            // 清空入驻商家审批记录表数据
            await _repository.Context.Deleteable<LogEnterpriseAuditrecordEntity>().Where(it => it.EId.Equals(entity.Id)).ExecuteCommandAsync();

            // 清空缓存
            _repository.Context.DataCache.RemoveDataCache(nameof(LogEnterpriseEntity));

            // 关闭事务
            _db.CommitTran();
        }
        catch (Exception)
        {
            // 回滚事务
            _db.RollbackTran();

            throw Oops.Oh(ErrorCode.COM1002);
        }
    } 
    #endregion


    /// <summary>
    /// 下拉选择
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/Selector")]
    public async Task<dynamic> Selector()
    {
        var data = await _repository.Where(it => it.Status == 1).Select(it => new { id = it.Id, name = it.Name }).ToListAsync();

        return new { list = data };
    }

    /// <summary>
    /// 审批
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("Audit/{id}")]
    [SqlSugarUnitOfWork]
    public async Task Audit(string id, [FromBody]LogEnterpriseAuditrecordInput input)
    {
        var entity = await _repository.AsQueryable().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        LogEnterpriseAuditrecordEntity logEnterpriseAuditrecordEntity = new LogEnterpriseAuditrecordEntity()
        {
            Id = SnowflakeIdHelper.NextId(),
            AuditTime = DateTime.Now,
            AuditUserId = _userManager.UserId,
            EId = entity.Id,
            Remark = $"【{(input.status == 0 ? "不通过" : "通过")}】{input.remark}",
        };
        entity.Status = input.status;
        await _repository.Context.Updateable<LogEnterpriseEntity>(entity).UpdateColumns(it=> it.Status).ExecuteCommandAsync();
        await _repository.Context.Insertable<LogEnterpriseAuditrecordEntity>(logEnterpriseAuditrecordEntity).ExecuteCommandAsync();
    }


    /// <summary>
    /// 获取入驻商家.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("PropertyJson/{id}")]
    public async Task<List<LogEnterpriseProperty>> GetPropertyJsonInfo(string id)
    {
        List<LogEnterpriseProperty> list = new List<LogEnterpriseProperty>();
        var entity = (await _repository.FirstOrDefaultAsync(x => x.Id == id)) ?? throw Oops.Oh(ErrorCode.COM1005);
        var options = await _dictionaryDataService.GetList("LogEnterpriseProperty");
        if (entity.PropertyJson.IsNotEmptyOrNull())
        {
            list = entity.PropertyJson.ToObject<List<LogEnterpriseProperty>>();
        }

        foreach (var item in options)
        {
            if (!list.Any(it => it.prop == item.EnCode))
            {
                list.Add(new LogEnterpriseProperty
                {
                    prop = item.EnCode,
                    enable = false,
                    title = item.FullName,
                    value = ""
                });
            }
        }

        return list;
    }

    /// <summary>
    /// 保存商家信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpPost("PropertyJson/{id}")]
    public async Task PropertyJsonInfo(string id, [FromBody] List<LogEnterpriseProperty> input)
    {
        List<LogEnterpriseProperty> list = new List<LogEnterpriseProperty>();
        var entity = (await _repository.FirstOrDefaultAsync(x => x.Id == id)) ?? throw Oops.Oh(ErrorCode.COM1005);

        // 检查属性是否重复
        if (input.GroupBy(it => it.prop).Where(it => it.Count() > 1).Any())
        {
            throw Oops.Oh("属性重复，请检查后再保存！");
        }

        entity.PropertyJson = input.ToJsonString();

        await _repository.Context.Updateable<LogEnterpriseEntity>(entity).UpdateColumns(it => it.PropertyJson).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取当前用户绑定的商家.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("CurrentUser")]
    public async Task<LogEnterpriseInfoOutput> GetInfoByCurrentUser()
    {
        var entity = await _repository.FirstOrDefaultAsync(it => it.AdminId == _userManager.UserId) ?? throw Oops.Oh(ErrorCode.COM1005);

        return entity.Adapt<LogEnterpriseInfoOutput>();
    }
}