using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Emp.Entitys.Dto.EmpTransferEmployee;
using QT.Emp.Entitys;
using QT.Emp.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Emp.Entitys.Dto.EmpDismissionEmployee;
using QT.Systems.Entitys.Permission;
using QT.Common.Const;

namespace QT.Emp;

/// <summary>
/// 业务实现：调岗管理.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Employee, Tag = "Emp", Name = "EmpTransferEmployee", Order = 200)]
[Route("api/Emp/[controller]")]
public class EmpTransferEmployeeService : IEmpTransferEmployeeService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<EmpTransferEmployeeEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly ITenant _db;

    /// <summary>
    /// 初始化一个<see cref="EmpTransferEmployeeService"/>类型的新实例.
    /// </summary>
    public EmpTransferEmployeeService(
        ISqlSugarRepository<EmpTransferEmployeeEntity> empTransferEmployeeRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = empTransferEmployeeRepository;
        _userManager = userManager;
        _db = context.AsTenant();
    }

    /// <summary>
    /// 获取调岗管理.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<EmpTransferEmployeeInfoOutput>();
    }

    /// <summary>
    /// 获取调岗管理列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] EmpTransferEmployeeListQueryInput input)
    {
        List<DateTime> queryTransferTime = input.transferTime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startTransferTime = queryTransferTime?.First();
        DateTime? endTransferTime = queryTransferTime?.Last();
        var data = await _repository.Context.Queryable<EmpTransferEmployeeEntity>()
            .WhereIF(input.type == 0, it => it.AuditTime == null)
            .WhereIF(input.type == 1, it => it.AuditTime != null)
            .WhereIF(queryTransferTime != null, it => SqlFunc.Between(it.TransferTime, startTransferTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endTransferTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new EmpTransferEmployeeListOutput
            {
                id = it.Id,
                employeeId = it.EmployeeId,
                transferOrganizeId = it.TransferOrganizeId,
                transferPositionId = it.TransferPositionId,
                transferOrganizeIdName = SqlFunc.Subqueryable<OrganizeEntity>().Where(ddd => ddd.Id == it.TransferOrganizeId).Select(ddd => ddd.FullName),
                transferPositionIdName = SqlFunc.Subqueryable<PositionEntity>().Where(ddd => ddd.Id == it.TransferPositionId).Select(ddd => ddd.FullName),
                reason = it.Reason,
                transferTime = it.TransferTime,
                organizeIdName = SqlFunc.Subqueryable<OrganizeEntity>().Where(ddd => ddd.Id == it.OrganizeId).Select(ddd => ddd.FullName),
                positionIdName = SqlFunc.Subqueryable<PositionEntity>().Where(ddd => ddd.Id == it.PositionId).Select(ddd => ddd.FullName),
                employeeIdName = SqlFunc.Subqueryable<EmpEmployeeEntity>().Where(x => x.Id == it.EmployeeId).Select(x => x.Name),
                confrimJoinTime = SqlFunc.Subqueryable<EmpEmployeeEntity>().Where(x => x.Id == it.EmployeeId).Select(x => x.ConfrimJoinTime),
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<EmpTransferEmployeeListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建调岗管理.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] EmpTransferEmployeeCrInput input)
    {
        var entity = input.Adapt<EmpTransferEmployeeEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新调岗管理.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] EmpTransferEmployeeUpInput input)
    {
        var entity = input.Adapt<EmpTransferEmployeeEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除调岗管理.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<EmpTransferEmployeeEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }


    /// <summary>
    /// 审批（确认调岗）.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("Actions/{id}/Audit")]
    public async Task Audit(string id, [FromBody] EmpTransferEmployeeUpInput input)
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

        // 判断当前部门和岗位是否和调入的一样
        if (entity.PositionId == entity.TransferPositionId && entity.OrganizeId == entity.TransferOrganizeId)
        {
            throw Oops.Oh("当前部门、岗位，与调入部门、岗位一致！");
        }

        _repository.Context.Tracking(employee);
        employee.PositionId = entity.TransferPositionId;
        employee.OrganizeId = entity.TransferOrganizeId;
        await _db.TranExecute(async () =>
        {
            // 更新调岗记录
            var isOk = await _repository.Context.Updateable(entity).ExecuteCommandAsync();
            if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);

            // 更新员工档案
            await _repository.Context.Updateable<EmpEmployeeEntity>(employee).ExecuteCommandAsync();
        });


    }

    /// <summary>
    /// 员工档案调整岗位.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("Actions/EmployeeSubmit")]
    public async Task EmployeeSubmit([FromBody] EmpTransferEmployeeUpInput input)
    {
        await _db.TranExecute(async () =>
        {
            var entity = input.Adapt<EmpTransferEmployeeEntity>();
            entity.Id = SnowflakeIdHelper.NextId();
            var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();

            input.id = entity.Id;
            await Audit(entity.Id, input);
        });


    }
}