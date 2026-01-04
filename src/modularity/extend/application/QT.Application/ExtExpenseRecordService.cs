using QT.Application.Entitys;
using QT.Application.Entitys.Dto.ExtExpenseDetail;
using QT.Application.Entitys.Dto.ExtExpenseRecord;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.LinqBuilder;
using QT.Extend.Entitys.Dto.FinancialRecord;
using QT.Systems.Interfaces.System;
using QT.WorkFlow.Entitys.Entity;
using QT.WorkFlow.Interfaces.Manager;
using QT.WorkFlow.Interfaces.Repository;
using System.Linq.Expressions;

namespace QT.Application;

/// <summary>
/// 业务实现：报销单.
/// </summary>
[ApiDescriptionSettings("乾通ERP.V2", Tag = "报销单", Name = "ExtExpenseRecord", Order = 300)]
[Route("api/extend/[controller]")]
public class ExtExpenseRecordService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ExtExpenseRecordEntity> _repository;

    /// <summary>
    /// 流程任务管理.
    /// </summary>
    private readonly IFlowTaskManager _flowTaskManager;

    /// <summary>
    /// 流程任务仓储.
    /// </summary>
    private readonly IFlowTaskRepository _flowTaskRepository;
    private readonly IBillRullService _billRullService;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;
    /// <summary>
    /// 初始化一个<see cref="ExtExpenseRecordService"/>类型的新实例.
    /// </summary>
    public ExtExpenseRecordService(
        ISqlSugarRepository<ExtExpenseRecordEntity> extExpenseRecordRepository,
        ISqlSugarClient context,
        IUserManager userManager,
        IFlowTaskManager flowTaskManager,
        IFlowTaskRepository flowTaskRepository,
        IBillRullService billRullService)
    {
        _repository = extExpenseRecordRepository;
        _flowTaskManager = flowTaskManager;
        _flowTaskRepository = flowTaskRepository;
        _billRullService = billRullService;
        _db = context.AsTenant();
        _userManager = userManager;
    }

    /// <summary>
    /// 获取报销单.
    /// </summary>
    /// <param name="id">参数.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<ExtExpenseRecordInfoOutput>();

        var extExpenseDetailList = await _repository.Context.Queryable<ExtExpenseDetailEntity>().Where(w => w.Fid == output.id).ToListAsync();
        output.extExpenseDetailList = extExpenseDetailList.Adapt<List<ExtExpenseDetailInfoOutput>>();

        return output;
    }

    [HttpGet("")]
    public async Task<PageResult<ExtExpenseRecordListOutput>> GetList([FromQuery] ExtExpenseRecordListQueryInput input)
    {
        List<DateTime> billDateRange = input.billDateRange?.Split(',').ToObject<List<DateTime>>();
        DateTime? startDate = billDateRange?.First();
        DateTime? endDate = billDateRange?.Last();

        Expression<Func<ExtExpenseRecordEntity,  bool>> labelWhere = null;
        if (input.label.IsNotEmptyOrNull())
        {
            foreach (var label in input.label.Split(",", true))
            {
                if (labelWhere == null)
                {
                    labelWhere = (x) => QTSqlFunc.FIND_IN_SET(label, x.Label);
                }
                else
                {
                    
                    labelWhere = labelWhere.Or((x) => QTSqlFunc.FIND_IN_SET(label, x.Label));
                }
            }
        }

        var data = await _repository.Context.Queryable<ExtExpenseRecordEntity, FlowTaskEntity>((x, b) => new JoinQueryInfos(JoinType.Left, x.Id == b.Id))
            .Where((x, b) => x.CreatorUserId == _userManager.UserId)
           //.WhereIF(input.category.HasValue, x => x.Category == input.category)
           .WhereIF(input.keyword.IsNotEmptyOrNull(),(x,b)=>x.BillNo.Contains(input.keyword))
           .WhereIF(billDateRange != null, (x,b) => SqlFunc.Between(x.BillDate, startDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(labelWhere != null, labelWhere)
           .Select<ExtExpenseRecordListOutput>((x, b) => new ExtExpenseRecordListOutput
           {
               id = x.Id,
               billDate = x.BillDate,
               amount = x.Amount,
               billNo = x.BillNo,
               flowId = x.FlowId,
               flowState = b.Status,
               remark = x.Remark,
               label = x.Label,
           })
           .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<ExtExpenseRecordListOutput>.SqlSugarPagedList(data);

    }

    /// <summary>
    /// 新建报销单.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    [SqlSugarUnitOfWork]
    public async Task Create([FromBody] ExtExpenseRecordCrInput input)
    {
        switch (input.flowState)
        {
            case 0:
                await FlowSubmit(null, input.Adapt<ExtExpenseRecordUpInput>());
                break;
            case 1:
                await FlowSave(null, input.Adapt<ExtExpenseRecordUpInput>());
                break;
        }
    }

    /// <summary>
    /// 更新报销单.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Update(string id, [FromBody] ExtExpenseRecordUpInput input)
    {
        switch (input.flowState)
        {
            case 0:
                await FlowSubmit(id, input);
                break;
            case 1:
                await FlowSave(id, input);
                break;
        }
    }

    /// <summary>
    /// 删除报销单.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Delete(string id)
    {
        var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id)) ?? throw Oops.Oh(ErrorCode.COM1005);
        await _repository.Context.Updateable<ExtExpenseRecordEntity>(entity).CallEntityMethod(m => m.Delete()).ExecuteCommandAsync();

        // 清空报销明细表数据
        await _repository.Context.Updateable<ExtExpenseDetailEntity>().SetColumns(it => new ExtExpenseDetailEntity()
        {
            DeleteMark = 1,
            DeleteUserId = _userManager.UserId,
            DeleteTime = SqlFunc.GetDate()
        }).Where(it => it.Fid == id).ExecuteCommandAsync();


        await _repository.Context.Updateable<FlowTaskEntity>().SetColumns(it => new FlowTaskEntity()
        {
            DeleteMark = 1,
            DeleteUserId = _userManager.UserId,
            DeleteTime = SqlFunc.GetDate()
        }).Where(it => it.Id == id).ExecuteCommandAsync();
    }

    /// <summary>
    /// 流程保存.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">表单信息.</param>
    /// <returns></returns>
    private async Task FlowSave(string id, ExtExpenseRecordUpInput input)
    {
        await HandleForm(id, input);

        await _flowTaskManager.Save(id, input.flowId, input.id, _userManager.User.RealName + "的报销单", 1, null, input, 1, 0, true);
    }

    /// <summary>
    /// 提交审核.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">流程信息.</param>
    /// <returns></returns>
    private async Task FlowSubmit(string id, ExtExpenseRecordUpInput input)
    {
        await HandleForm(id, input);

        await _flowTaskManager.Submit(id, input.flowId, input.id, _userManager.User.RealName + "的报销单", 1, null, input, 0, 0, true, false, input.candidateList);

    }

    /// <summary>
    /// 表单操作.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">表单数据.</param>
    /// <returns></returns>
    private async Task HandleForm(string id, ExtExpenseRecordUpInput input)
    {
        var entity = input.Adapt<ExtExpenseRecordEntity>();

        var extExpenseDetailEntityList = input.extExpenseDetailList.Adapt<List<ExtExpenseDetailEntity>>();

        if (string.IsNullOrEmpty(id))
        {
            entity.Id = SnowflakeIdHelper.NextId();
            entity.BillNo = await _billRullService.GetBillNumber("ExpenseRecord");
            entity.Amount = extExpenseDetailEntityList.Sum(x => x.Amount);
            await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
            if (extExpenseDetailEntityList != null)
            {
                foreach (var item in extExpenseDetailEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Fid = entity.Id;
                }

                await _repository.Context.Insertable<ExtExpenseDetailEntity>(extExpenseDetailEntityList).ExecuteCommandAsync();
            }

        }
        else
        {
            entity.Id = id;
            entity.Amount = extExpenseDetailEntityList.Sum(x => x.Amount);
            await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

            // 清空报销明细原有数据
            await _repository.Context.Deleteable<ExtExpenseDetailEntity>().Where(it => it.Fid == entity.Id).ExecuteCommandAsync();

            // 新增报销明细新数据
            if (extExpenseDetailEntityList != null)
            {
                foreach (var item in extExpenseDetailEntityList)
                {
                    item.Id ??= SnowflakeIdHelper.NextId();
                    item.Fid = entity.Id;
                }

                await _repository.Context.Insertable<ExtExpenseDetailEntity>(extExpenseDetailEntityList).ExecuteCommandAsync();
            }

        }

        input.id = entity.Id;
    }

    /// <summary>
    /// 工作流表单操作.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="obj"></param>
    /// <param name="type">0：事前审批，1：创建子流程</param>
    /// <returns></returns>
    [NonAction]
    public async Task Save(string id, object obj, int type)
    {
        try
        {
            var input = obj.ToObject<ExtExpenseRecordUpInput>();
            var entity = input.Adapt<ExtExpenseRecordEntity>();

            var extExpenseDetailEntityList = input.extExpenseDetailList.Adapt<List<ExtExpenseDetailEntity>>();

            if (type == 0)
            {
                await HandleForm(id, input);
            }
            else
            {
                entity.Id = id;
                await _repository.InsertAsync(entity);

                if (extExpenseDetailEntityList != null)
                {
                    foreach (var item in extExpenseDetailEntityList)
                    {
                        item.Id = SnowflakeIdHelper.NextId();
                        item.Fid = entity.Id;
                    }

                    await _repository.Context.Insertable<ExtExpenseDetailEntity>(extExpenseDetailEntityList).ExecuteCommandAsync();
                }

            }
        }
        catch (Exception e)
        {
            throw;
        }
    }
}