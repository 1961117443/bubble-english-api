using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.ClayObject;
using QT.Common.Configuration;
using QT.Common.Models.NPOI;
using QT.DataEncryption;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Emp.Entitys.Dto.EmpEntryEmployee;
using QT.Emp.Entitys;
using QT.Emp.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Systems.Entitys.Permission;
using NPOI.OpenXmlFormats;
using QT.Common.Const;

namespace QT.Emp;

/// <summary>
/// 业务实现：入职管理.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Employee, Tag = "Emp", Name = "EmpEntryEmployee", Order = 200)]
[Route("api/Emp/[controller]")]
public class EmpEntryEmployeeService : IEmpEntryEmployeeService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<EmpEntryEmployeeEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly ITenant _db;

    /// <summary>
    /// 初始化一个<see cref="EmpEntryEmployeeService"/>类型的新实例.
    /// </summary>
    public EmpEntryEmployeeService(
        ISqlSugarRepository<EmpEntryEmployeeEntity> empEntryEmployeeRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = empEntryEmployeeRepository;
        _userManager = userManager;
        _db = context.AsTenant();
    }

    /// <summary>
    /// 获取入职管理.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<EmpEntryEmployeeInfoOutput>();
    }

    /// <summary>
    /// 获取入职管理列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] EmpEntryEmployeeListQueryInput input)
    {
        List<DateTime> queryConfrimJoinTime = input.confrimJoinTime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startConfrimJoinTime = queryConfrimJoinTime?.First();
        DateTime? endConfrimJoinTime = queryConfrimJoinTime?.Last();
        var data = await _repository.Context.Queryable<EmpEntryEmployeeEntity>()
            .WhereIF(input.type == 0, it => it.AuditTime == null)
            .WhereIF(input.type == 1, it => it.AuditTime != null)
            //.Where(it=> SqlFunc.Subqueryable<EmpEmployeeEntity>().Where(x=>x.Mobile == it.Mobile).NotAny())
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.mobile), it => it.Mobile.Contains(input.mobile))
            .WhereIF(queryConfrimJoinTime != null, it => SqlFunc.Between(it.ConfrimJoinTime, startConfrimJoinTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endConfrimJoinTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                || it.Mobile.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new EmpEntryEmployeeListOutput
            {
                id = it.Id,
                name = it.Name,
                mobile = it.Mobile,
                organizeId = it.OrganizeId,
                positionId = it.PositionId,
                organizeIdName = SqlFunc.Subqueryable<OrganizeEntity>().Where(ddd => ddd.Id == it.OrganizeId).Select(ddd => ddd.FullName),
                positionIdName = SqlFunc.Subqueryable<PositionEntity>().Where(ddd => ddd.Id == it.PositionId).Select(ddd => ddd.FullName),
                confrimJoinTime = it.ConfrimJoinTime,
                employeeStatus = it.EmployeeStatus,
                employeeType = it.EmployeeType,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);

        foreach (var item in data.list)
        {
            if (item.confrimJoinTime.HasValue)
            {
                item.remainDays = (DateTime.Now - item.confrimJoinTime.Value).Days;
            }
        }
        return PageResult<EmpEntryEmployeeListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建入职管理.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] EmpEntryEmployeeCrInput input)
    {
        var entity = input.Adapt<EmpEntryEmployeeEntity>();

        // 判断手机号码是否存在
        if (await _repository.Context.Queryable<EmpEmployeeEntity>().AnyAsync(x => x.Mobile == entity.Mobile))
        {
            throw Oops.Oh("员工已存在！");
        }

        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 获取入职管理无分页列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    private async Task<dynamic> GetNoPagingList([FromQuery] EmpEntryEmployeeListQueryInput input)
    {
        List<DateTime> queryConfrimJoinTime = input.confrimJoinTime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startConfrimJoinTime = queryConfrimJoinTime?.First();
        DateTime? endConfrimJoinTime = queryConfrimJoinTime?.Last();
        return await _repository.Context.Queryable<EmpEntryEmployeeEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.mobile), it => it.Mobile.Contains(input.mobile))
            .WhereIF(queryConfrimJoinTime != null, it => SqlFunc.Between(it.ConfrimJoinTime, startConfrimJoinTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endConfrimJoinTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                || it.Mobile.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new EmpEntryEmployeeListOutput
            {
                id = it.Id,
                name = it.Name,
                mobile = it.Mobile,
                organizeId = it.OrganizeId,
                positionId = it.PositionId,
                confrimJoinTime = it.ConfrimJoinTime,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToListAsync();
    }

    /// <summary>
    /// 导出入职管理.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Actions/Export")]
    public async Task<dynamic> Export([FromQuery] EmpEntryEmployeeListQueryInput input)
    {
        var exportData = new List<EmpEntryEmployeeListOutput>();
        if (input.dataType == 0)
            exportData = Clay.Object(await GetList(input)).Solidify<PageResult<EmpEntryEmployeeListOutput>>().list;
        else
            exportData = await GetNoPagingList(input);
        List<ParamsModel> paramList = "[{\"value\":\"姓名\",\"field\":\"name\"},{\"value\":\"预计入职时间\",\"field\":\"confrimJoinTime\"},{\"value\":\"部门\",\"field\":\"organizeId\"},{\"value\":\"岗位\",\"field\":\"positionId\"},{\"value\":\"手机号码\",\"field\":\"mobile\"},]".ToList<ParamsModel>();
        ExcelConfig excelconfig = new ExcelConfig();
        excelconfig.FileName = "入职管理.xls";
        excelconfig.HeadFont = "微软雅黑";
        excelconfig.HeadPoint = 10;
        excelconfig.IsAllSizeColumn = true;
        excelconfig.ColumnModel = new List<ExcelColumnModel>();
        foreach (var item in input.selectKey.Split(',').ToList())
        {
            var isExist = paramList.Find(p => p.field == item);
            if (isExist != null)
                excelconfig.ColumnModel.Add(new ExcelColumnModel() { Column = isExist.field, ExcelColumn = isExist.value });
        }

        var addPath = FileVariable.TemporaryFilePath + excelconfig.FileName;
        ExcelExportHelper<EmpEntryEmployeeListOutput>.Export(exportData, excelconfig, addPath);
        var fileName = _userManager.UserId + "|" + addPath + "|xls";
        return new
        {
            name = excelconfig.FileName,
            url = "/api/File/Download?encryption=" + DESCEncryption.Encrypt(fileName, "QT")
        };
    }

    /// <summary>
    /// 更新入职管理.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] EmpEntryEmployeeUpInput input)
    {
        var entity = input.Adapt<EmpEntryEmployeeEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(nameof(EmpEntryEmployeeEntity.Mobile)).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 确认入职.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("Actions/{id}/Entry")]
    public async Task Entry(string id, [FromBody] EmpEntryEmployeeEntryInput input)
    {
        var entity = await _repository.AsQueryable().Where(x=>x.Id == id && x.AuditTime == null).FirstAsync() ?? throw Oops.Oh(ErrorCode.COM1005);

        _repository.Context.Tracking(entity);
        try
        {
            _db.BeginTran();
            input.Adapt(entity);
            // 更新入职记录
            // 更新审核状态
            entity.AuditTime = DateTime.Now;
            entity.AuditUserId = _userManager.UserId;

            var isOk = await _repository.Context.Updateable(entity).ExecuteCommandAsync();
            if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);

            // 生成员工档案
            // 根据手机号码判断是否有员工记录
            var employee = await _repository.Context.Queryable<EmpEmployeeEntity>().ClearFilter().Where(x => x.Mobile == entity.Mobile).FirstAsync();
            if (employee == null)
            {
                employee = input.Adapt<EmpEmployeeEntity>();
                isOk = await _repository.Context.Insertable<EmpEmployeeEntity>(employee).IgnoreColumns(true).ExecuteCommandAsync();
                if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
            }
            else
            {
                _repository.Context.Tracking(employee);
                input.Adapt(employee);
                employee.DeleteMark = null;
                employee.DeleteTime = null;
                employee.DeleteUserId = string.Empty;
                isOk = await _repository.Context.Updateable<EmpEmployeeEntity>(employee).ExecuteCommandAsync();
                if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
            }

            _db.CommitTran();
        }
        catch (Exception ex)
        {
            // 回滚事务
            _db.RollbackTran();
            if (ex is AppFriendlyException appFriendlyException)
            {
                throw appFriendlyException;
            }
            throw Oops.Oh(ErrorCode.COM1001);
        }
        
    }

    /// <summary>
    /// 删除入职管理.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<EmpEntryEmployeeEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
}