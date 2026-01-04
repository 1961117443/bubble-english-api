using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Emp.Entitys.Dto.EmpDismissionEmployee;
using QT.Emp.Entitys;
using QT.Emp.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Systems.Entitys.Permission;
using QT.Emp.Entitys.Dto.EmpTransferEmployee;
using QT.Common.Const;

namespace QT.Emp;

/// <summary>
/// 业务实现：离职管理.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Employee, Tag = "Emp", Name = "EmpDismissionEmployee", Order = 200)]
[Route("api/Emp/[controller]")]
public class EmpDismissionEmployeeService : IEmpDismissionEmployeeService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<EmpDismissionEmployeeEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly ITenant _db;

    /// <summary>
    /// 初始化一个<see cref="EmpDismissionEmployeeService"/>类型的新实例.
    /// </summary>
    public EmpDismissionEmployeeService(
        ISqlSugarRepository<EmpDismissionEmployeeEntity> empDismissionEmployeeRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = empDismissionEmployeeRepository;
        _userManager = userManager;
        _db = context.AsTenant();
    }

    /// <summary>
    /// 获取离职管理.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<EmpDismissionEmployeeInfoOutput>();
    }

    /// <summary>
    /// 获取离职管理列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] EmpDismissionEmployeeListQueryInput input)
    {
        List<DateTime> queryLastWorkDay = input.lastWorkDay?.Split(',').ToObject<List<DateTime>>();
        DateTime? startLastWorkDay = queryLastWorkDay?.First();
        DateTime? endLastWorkDay = queryLastWorkDay?.Last();
        var data = await _repository.Context.Queryable<EmpDismissionEmployeeEntity>()
            .WhereIF(input.type == 0, it=>it.AuditTime == null)
            .WhereIF(input.type == 1, it => it.AuditTime !=null)
            .WhereIF(queryLastWorkDay != null, it => SqlFunc.Between(it.LastWorkDay, startLastWorkDay.ParseToDateTime("yyyy-MM-dd 00:00:00"), endLastWorkDay.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new EmpDismissionEmployeeListOutput
            {
                id = it.Id,
                employeeId = it.EmployeeId,
                lastWorkDay = it.LastWorkDay,
                reason = it.Reason,
                organizeIdName = SqlFunc.Subqueryable<OrganizeEntity>().Where(ddd => ddd.Id == it.OrganizeId).Select(ddd => ddd.FullName),
                positionIdName = SqlFunc.Subqueryable<PositionEntity>().Where(ddd => ddd.Id == it.PositionId).Select(ddd => ddd.FullName),
                employeeIdName = SqlFunc.Subqueryable<EmpEmployeeEntity>().Where(x=>x.Id == it.EmployeeId).Select(x=>x.Name),
                confrimJoinTime = SqlFunc.Subqueryable<EmpEmployeeEntity>().Where(x => x.Id == it.EmployeeId).Select(x => x.ConfrimJoinTime),
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<EmpDismissionEmployeeListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建离职管理.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task<string> Create([FromBody] EmpDismissionEmployeeCrInput input)
    {
        var entity = input.Adapt<EmpDismissionEmployeeEntity>();

        // 判断员工是否正在离职
        if (await _repository.AnyAsync(x => x.EmployeeId == entity.EmployeeId && !x.AuditTime.HasValue))
        {
            throw Oops.Oh($"已经为员工办理过离职，处于待离职状态。");
        }

        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);

        return entity.Id;
    }

    /// <summary>
    /// 更新离职管理.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] EmpDismissionEmployeeUpInput input)
    {
        var entity = input.Adapt<EmpDismissionEmployeeEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 审批（确认离职）.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("Actions/{id}/Audit")]
    public async Task Audit(string id, [FromBody] EmpDismissionEmployeeUpInput input)
    {
        var entity = await _repository.AsQueryable().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        var employee = await _repository.Context.Queryable<EmpEmployeeEntity>().InSingleAsync(entity.EmployeeId) ?? throw Oops.Oh(ErrorCode.COM1005);

        _repository.Context.Tracking(entity);
        input.Adapt(entity);
        entity.AuditTime = DateTime.Now;
        entity.AuditUserId = _userManager.UserId;
        // 锁定当前部门和岗位
        entity.PositionId = employee.PositionId;
        entity.OrganizeId = employee.OrganizeId;

        _repository.Context.Tracking(employee);
        employee.Delete();
        await _db.TranExecute(async () =>
        {
            // 更新离职记录
            var isOk = await _repository.Context.Updateable(entity).ExecuteCommandAsync();
            if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);

            // 删除员工档案
            await _repository.Context.Updateable<EmpEmployeeEntity>(employee).ExecuteCommandAsync();
        });


    }

    /// <summary>
    /// 删除离职管理.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<EmpDismissionEmployeeEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 员工档案办理离职.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("Actions/EmployeeSubmit")]
    public async Task EmployeeSubmit([FromBody] EmpDismissionEmployeeUpInput input)
    {
        await _db.TranExecute(async () =>
        {
            input.id = await Create(input);

            await Audit(input.id, input);
        });
    }
}