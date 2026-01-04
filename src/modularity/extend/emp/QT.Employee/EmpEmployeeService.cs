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
using QT.Emp.Entitys.Dto.EmpEmployee;
using QT.Emp.Entitys.Dto.EmpEmployeeEdu;
using QT.Emp.Entitys.Dto.EmpEmployeeFamily;
using QT.Emp.Entitys.Dto.EmpEmployeeUrgent;
using QT.Emp.Entitys;
using QT.Emp.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Systems.Entitys.Permission;
using QT.Emp.Entitys.Dto.EmpEntryEmployee;
using QT.Emp.Entitys.Dto.EmpTransferEmployee;
using QT.Systems.Interfaces.Permission;
using QT.Systems.Entitys.Dto.User;
using QT.Common.Const;

namespace QT.Emp;

/// <summary>
/// 业务实现：员工档案.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Employee, Tag = "Emp", Name = "EmpEmployee", Order = 200)]
[Route("api/Emp/[controller]")]
public class EmpEmployeeService : IEmpEmployeeService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<EmpEmployeeEntity> _repository;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly IEmpChangeEmployeeService _empChangeEmployeeService;

    /// <summary>
    /// 初始化一个<see cref="EmpEmployeeService"/>类型的新实例.
    /// </summary>
    public EmpEmployeeService(
        ISqlSugarRepository<EmpEmployeeEntity> empEmployeeRepository,
        ISqlSugarClient context,
        IUserManager userManager,
        IEmpChangeEmployeeService empChangeEmployeeService)
    {
        _repository = empEmployeeRepository;
        _db = context.AsTenant();
        _userManager = userManager;
        _empChangeEmployeeService = empChangeEmployeeService;
    }

    /// <summary>
    /// 获取员工档案.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<EmpEmployeeInfoOutput>();

        var empEmployeeEduList = await _repository.Context.Queryable<EmpEmployeeEduEntity>().Where(w => w.EmployeeId == output.id).ToListAsync();
        output.empEmployeeEduList = empEmployeeEduList.Adapt<List<EmpEmployeeEduInfoOutput>>();

        var empEmployeeFamilyList = await _repository.Context.Queryable<EmpEmployeeFamilyEntity>().Where(w => w.EmployeeId == output.id).ToListAsync();
        output.empEmployeeFamilyList = empEmployeeFamilyList.Adapt<List<EmpEmployeeFamilyInfoOutput>>();

        var empEmployeeUrgentList = await _repository.Context.Queryable<EmpEmployeeUrgentEntity>().Where(w => w.EmployeeId == output.id).ToListAsync();
        output.empEmployeeUrgentList = empEmployeeUrgentList.Adapt<List<EmpEmployeeUrgentInfoOutput>>();

        output.empEmployeeChangeList = await _empChangeEmployeeService.ChangeList(id);

        return output;
    }

    /// <summary>
    /// 获取员工档案.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("Profile/{id}")]
    public async Task<dynamic> GetProfileInfo(string id)
    {
        var entity = await _repository.AsQueryable().ClearFilter().FirstAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);
        var output = entity.Adapt<EmpEmployeeInfoOutput>().Adapt<EmpEmployeeProfileInfoOutput>();

        output.organizeIdName = await _repository.Context.Queryable<OrganizeEntity>().Where(it => it.Id == output.organizeId).Select(it => it.FullName).FirstAsync();
        output.positionIdName = await _repository.Context.Queryable<PositionEntity>().Where(it => it.Id == output.positionId).Select(it => it.FullName).FirstAsync();

        if (entity.DeleteMark == 1)
        {
            output.isDismission = true;
        }
        if (entity.ConfrimJoinTime.HasValue)
        {
            output.onJobDays = (DateTime.Now - entity.ConfrimJoinTime.Value).Days;
        }
        if (entity.BirthTime.HasValue)
        {
            output.age = DateTime.Now.Year - entity.BirthTime.Value.Year;
        }

        var empEmployeeEduList = await _repository.Context.Queryable<EmpEmployeeEduEntity>().Where(w => w.EmployeeId == output.id).ToListAsync();
        output.empEmployeeEduList = empEmployeeEduList.Adapt<List<EmpEmployeeEduInfoOutput>>();

        var empEmployeeFamilyList = await _repository.Context.Queryable<EmpEmployeeFamilyEntity>().Where(w => w.EmployeeId == output.id).ToListAsync();
        output.empEmployeeFamilyList = empEmployeeFamilyList.Adapt<List<EmpEmployeeFamilyInfoOutput>>();

        var empEmployeeUrgentList = await _repository.Context.Queryable<EmpEmployeeUrgentEntity>().Where(w => w.EmployeeId == output.id).ToListAsync();
        output.empEmployeeUrgentList = empEmployeeUrgentList.Adapt<List<EmpEmployeeUrgentInfoOutput>>();

        output.empEmployeeChangeList = await _empChangeEmployeeService.ChangeList(id);
        return output;
    }

    /// <summary>
    /// 获取员工档案列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] EmpEmployeeListQueryInput input)
    {
        List<DateTime> queryConfrimJoinTime = input.confrimJoinTime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startConfrimJoinTime = queryConfrimJoinTime?.First();
        DateTime? endConfrimJoinTime = queryConfrimJoinTime?.Last();

        List<DateTime> queryPlanRegularTime = input.planRegularTime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startPlanRegularTime = queryPlanRegularTime?.First();
        DateTime? endPlanRegularTime = queryPlanRegularTime?.Last();

        DateTime? startBirthTime=null;
        if (!string.IsNullOrEmpty(input.birthMonth) && DateTime.TryParse($"{input.birthMonth}-01",out var b))
        {
            startBirthTime = b;
        } 
        DateTime? startJoinTime = null;
        if (!string.IsNullOrEmpty(input.confrimJoinMonth) && DateTime.TryParse($"{input.confrimJoinMonth}-01", out var j))
        {
            startJoinTime = j;
        }

        var data = await _repository.Context.Queryable<EmpEmployeeEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.organizeId), it => it.OrganizeId.Equals(input.organizeId))
            .WhereIF(queryConfrimJoinTime != null, it => SqlFunc.Between(it.ConfrimJoinTime, startConfrimJoinTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endConfrimJoinTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(queryPlanRegularTime != null, it => SqlFunc.Between(it.PlanRegularTime, startPlanRegularTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endPlanRegularTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                || it.OrganizeId.Contains(input.keyword)
                )
            .WhereIF(startBirthTime.HasValue, it => it.BirthTime.Value.Month == startBirthTime.Value.Month)
            .WhereIF(startJoinTime.HasValue, it => it.ConfrimJoinTime.Value.Month == startJoinTime.Value.Month && it.ConfrimJoinTime.Value.Year < DateTime.Now.Year)
            .WhereIF(!string.IsNullOrEmpty(input.employeeStatus),it=> it.EmployeeStatus == input.employeeStatus)
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new EmpEmployeeListOutput
            {
                id = it.Id,
                name = it.Name,
                organizeId = it.OrganizeId,
                positionId = it.PositionId,
                mobile = it.Mobile,
                confrimJoinTime = it.ConfrimJoinTime,
                employeeType = it.EmployeeType,
                birthTime = it.BirthTime,
                organizeIdName = SqlFunc.Subqueryable<OrganizeEntity>().Where(ddd=>ddd.Id == it.OrganizeId).Select(ddd=>ddd.FullName),
                positionIdName = SqlFunc.Subqueryable<PositionEntity>().Where(ddd => ddd.Id == it.PositionId).Select(ddd => ddd.FullName),
                planRegularTime=it.PlanRegularTime,
                probationPeriodType=it.ProbationPeriodType
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<EmpEmployeeListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建员工档案.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] EmpEmployeeCrInput input)
    {
        var entity = input.Adapt<EmpEmployeeEntity>();
        entity.Id = SnowflakeIdHelper.NextId();

        // 判断手机号码是否存在
        if (await _repository.AnyAsync(it => it.Mobile == entity.Mobile))
        {
            throw Oops.Oh($"员工已存在！");
        }

        // 判断入职管理是否有相同手机号的待入职记录
        if (await _repository.Context.Queryable<EmpEntryEmployeeEntity>().AnyAsync(it => it.Mobile == entity.Mobile && it.AuditTime == null))
        {
            throw Oops.Oh($"[{entity.Name}]在待入职列表中");
        }

        // 判断是否已经有删除的记录
        var delEntity = await _repository.Context.Queryable<EmpEmployeeEntity>().ClearFilter().SingleAsync(it => it.DeleteMark == 1 && it.Mobile == entity.Mobile);
        if (delEntity !=null)
        {
            delEntity.DeleteMark = null;
            delEntity.DeleteTime = null;
            delEntity.DeleteUserId = string.Empty;
            await _repository.Context.Updateable(delEntity).UpdateColumns("DeleteMark", "DeleteTime", "DeleteUserId").ExecuteCommandAsync();
            var upInput = input.Adapt<EmpEmployeeUpInput>();
            upInput.id = delEntity.Id;
            await Update(delEntity.Id, upInput);
            return;
        }

        try
        {
            // 开启事务
            _db.BeginTran();

            var newEntity = await _repository.Context.Insertable<EmpEmployeeEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            var empEmployeeEduEntityList = input.empEmployeeEduList.Adapt<List<EmpEmployeeEduEntity>>();
            if(empEmployeeEduEntityList != null)
            {
                foreach (var item in empEmployeeEduEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.EmployeeId = newEntity.Id;
                }

                await _repository.Context.Insertable<EmpEmployeeEduEntity>(empEmployeeEduEntityList).ExecuteCommandAsync();
            }

            var empEmployeeFamilyEntityList = input.empEmployeeFamilyList.Adapt<List<EmpEmployeeFamilyEntity>>();
            if(empEmployeeFamilyEntityList != null)
            {
                foreach (var item in empEmployeeFamilyEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.EmployeeId = newEntity.Id;
                }

                await _repository.Context.Insertable<EmpEmployeeFamilyEntity>(empEmployeeFamilyEntityList).ExecuteCommandAsync();
            }

            var empEmployeeUrgentEntityList = input.empEmployeeUrgentList.Adapt<List<EmpEmployeeUrgentEntity>>();
            if(empEmployeeUrgentEntityList != null)
            {
                foreach (var item in empEmployeeUrgentEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.EmployeeId = newEntity.Id;
                }

                await _repository.Context.Insertable<EmpEmployeeUrgentEntity>(empEmployeeUrgentEntityList).ExecuteCommandAsync();
            }

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
    /// 获取员工档案无分页列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    private async Task<dynamic> GetNoPagingList([FromQuery] EmpEmployeeListQueryInput input)
    {
        List<DateTime> queryConfrimJoinTime = input.confrimJoinTime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startConfrimJoinTime = queryConfrimJoinTime?.First();
        DateTime? endConfrimJoinTime = queryConfrimJoinTime?.Last();
        return await _repository.Context.Queryable<EmpEmployeeEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.organizeId), it => it.OrganizeId.Equals(input.organizeId))
            .WhereIF(queryConfrimJoinTime != null, it => SqlFunc.Between(it.ConfrimJoinTime, startConfrimJoinTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endConfrimJoinTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                || it.OrganizeId.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new EmpEmployeeListOutput
            {
                id = it.Id,
                name = it.Name,
                organizeId = it.OrganizeId,
                positionId = it.PositionId,
                mobile = it.Mobile,
                confrimJoinTime = it.ConfrimJoinTime,
                employeeType = it.EmployeeType,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToListAsync();
    }

    /// <summary>
    /// 导出员工档案.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Actions/Export")]
    public async Task<dynamic> Export([FromQuery] EmpEmployeeListQueryInput input)
    {
        var exportData = new List<EmpEmployeeListOutput>();
        if (input.dataType == 0)
            exportData = Clay.Object(await GetList(input)).Solidify<PageResult<EmpEmployeeListOutput>>().list;
        else
            exportData = await GetNoPagingList(input);
        List<ParamsModel> paramList = "[{\"value\":\"姓名\",\"field\":\"name\"},{\"value\":\"部门\",\"field\":\"organizeId\"},{\"value\":\"岗位\",\"field\":\"positionId\"},{\"value\":\"入职时间\",\"field\":\"confrimJoinTime\"},{\"value\":\"员工类型\",\"field\":\"employeeType\"},{\"value\":\"手机号码\",\"field\":\"mobile\"},]".ToList<ParamsModel>();
        ExcelConfig excelconfig = new ExcelConfig();
        excelconfig.FileName = "员工档案.xls";
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
        ExcelExportHelper<EmpEmployeeListOutput>.Export(exportData, excelconfig, addPath);
        var fileName = _userManager.UserId + "|" + addPath + "|xls";
        return new
        {
            name = excelconfig.FileName,
            url = "/api/File/Download?encryption=" + DESCEncryption.Encrypt(fileName, "QT")
        };
    }

    /// <summary>
    /// 批量删除员工档案.
    /// </summary>
    /// <param name="ids">主键数组.</param>
    /// <returns></returns>
    [HttpPost("batchRemove")]
    public async Task BatchRemove([FromBody] List<string> ids)
    {
        var entitys = await _repository.Context.Queryable<EmpEmployeeEntity>().In(it => it.Id, ids).ToListAsync();
        if (entitys.Count > 0)
        {
            try
            {
                // 开启事务
                _db.BeginTran();

                // 批量删除员工档案
                await _repository.Context.Deleteable<EmpEmployeeEntity>().In(it => it.Id,ids).ExecuteCommandAsync();

                // 清空员工学历表数据
                await _repository.Context.Deleteable<EmpEmployeeEduEntity>().In(u => u.EmployeeId, entitys.Select(s => s.Id).ToArray()).ExecuteCommandAsync();

                // 清空员工家庭信息表数据
                await _repository.Context.Deleteable<EmpEmployeeFamilyEntity>().In(u => u.EmployeeId, entitys.Select(s => s.Id).ToArray()).ExecuteCommandAsync();

                // 清空员工紧急联系人表数据
                await _repository.Context.Deleteable<EmpEmployeeUrgentEntity>().In(u => u.EmployeeId, entitys.Select(s => s.Id).ToArray()).ExecuteCommandAsync();

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
    }

    /// <summary>
    /// 更新员工档案.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] EmpEmployeeUpInput input)
    {
        //var entity = input.Adapt<EmpEmployeeEntity>(dbEntity);

        // 判断手机号码是否存在
        if (await _repository.AnyAsync(it => it.Mobile == input.mobile && it.Id != id))
        {
            throw Oops.Oh($"员工已存在！");
        }
        try
        {
            // 开启事务
            _db.BeginTran();

            var entity = await _repository.SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);

            _repository.Context.Tracking(entity);

            input.Adapt(entity);

            await _repository.Context.Updateable<EmpEmployeeEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).EnableDiffLogEvent().ExecuteCommandAsync();

            await _repository.Context.CUDSaveAsnyc<EmpEmployeeEduEntity,EmpEmployeeEduUpInput>(it => it.EmployeeId == entity.Id, input.empEmployeeEduList, it => it.EmployeeId = entity.Id);


            // 清空员工家庭信息原有数据
            await _repository.Context.CUDSaveAsnyc<EmpEmployeeFamilyEntity,EmpEmployeeFamilyUpInput>(it => it.EmployeeId == entity.Id, input.empEmployeeFamilyList, it => it.EmployeeId = entity.Id);

            // 清空员工紧急联系人原有数据
            await _repository.Context.CUDSaveAsnyc<EmpEmployeeUrgentEntity, EmpEmployeeUrgentUpInput>(it => it.EmployeeId == entity.Id, input.empEmployeeUrgentList, it => it.EmployeeId = entity.Id);

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
    /// 删除员工档案.
    /// 硬删除，离职才软删除.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        // 逻辑删除
        var entity = await _repository.Context.Queryable<EmpEmployeeEntity>().SingleAsync(it => it.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);

        _repository.Context.Tracking(entity);
        entity.Delete();
        await _repository.Context.Updateable(entity).EnableDiffLogEvent().ExecuteCommandAsync();

        //if (!await _repository.Context.Queryable<EmpEmployeeEntity>().AnyAsync(it => it.Id == id))
        //{
        //    throw Oops.Oh(ErrorCode.COM1005);
        //}

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        //    var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        //    await _repository.Context.Deleteable<EmpEmployeeEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

        //    // 清空员工学历表数据
        //    await _repository.Context.Deleteable<EmpEmployeeEduEntity>().Where(it => it.EmployeeId.Equals(entity.Id)).ExecuteCommandAsync();

        //    // 清空员工家庭信息表数据
        //    await _repository.Context.Deleteable<EmpEmployeeFamilyEntity>().Where(it => it.EmployeeId.Equals(entity.Id)).ExecuteCommandAsync();

        //    // 清空员工紧急联系人表数据
        //    await _repository.Context.Deleteable<EmpEmployeeUrgentEntity>().Where(it => it.EmployeeId.Equals(entity.Id)).ExecuteCommandAsync();

        //    // 关闭事务
        //    _db.CommitTran();
        //}
        //catch (Exception)
        //{
        //    // 回滚事务
        //    _db.RollbackTran();

        //    throw Oops.Oh(ErrorCode.COM1002);
        //}
    }

    /// <summary>
    /// 办理转正.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("Actions/{id}/Probation")]
    public async Task Probation(string id, [FromBody] EmpProbationEmployeeUpInput input)
    {
        // 1、更新员工档案的实际转正日期
        //2、写入转正记录

        var entity = await _repository.AsQueryable().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        entity.RegularTime = input.regularTime;
        entity.EmployeeStatus = "1"; // 状态更新为1

        var probationEntity = new EmpProbationEmployeeEntity
        {
            ConfrimJoinTime = entity.ConfrimJoinTime,
            OrganizeId = entity.OrganizeId,
            PositionId = entity.PositionId,
            RegularTime = entity.RegularTime,
            EmployeeId = entity.Id,
            Remark = input.remark,
            Id = SnowflakeIdHelper.NextId(),
        };

        await _db.TranExecute(async() =>
        {
            // 1、更新员工档案的实际转正日期
            var isOk = await _repository.Context.Updateable(entity).UpdateColumns("RegularTime", "EmployeeStatus").ExecuteCommandAsync();
            if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);

            //2、写入转正记录
            isOk = await _repository.Context.Insertable<EmpProbationEmployeeEntity>(probationEntity).IgnoreColumns(true).ExecuteCommandAsync();
            if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
        });
    }

    /// <summary>
    /// 创建用户账号
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("Actions/{id}/CreateAccount")]
    public async Task CreateAccount(string id, [FromServices] IUsersService usersService)
    {
        var entity = await _repository.AsQueryable().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);
        if (string.IsNullOrEmpty(entity.Mobile))
        {
            throw Oops.Oh("手机号码不能为空！");
        }
        // 创建客户账号
        await usersService.InnerCreate(new UserInCrInput
        {
            id = SnowflakeIdHelper.NextId(),
            account = entity.Mobile,
            realName = entity.Name,
            password = CommonConst.DEFAULTPASSWORD,
            mobilePhone = entity.Mobile,
            gender = entity.SexType == "1"? 1:(entity.SexType == "2"? 2:3),
            organizeId = entity.OrganizeId,
            positionId = entity.PositionId,
        });
    }


    [Obsolete]
    private async Task LoadChangeList(EmpEmployeeInfoOutput output)
    {
        var list1 = await _repository.Context.Queryable<EmpEntryEmployeeEntity>()
          .Where(w => w.Id == output.id && w.AuditTime != null).Select(w => new EmpEmployeeChangeLogOutput
          {
              time = w.ConfrimJoinTime,
              operateTime = w.AuditTime,
              tag = "入职",
              description = "操作入职"
          }).ToListAsync();

        var list2 = await _repository.Context.Queryable<EmpTransferEmployeeEntity>()
            .Where(w => w.EmployeeId == output.id && w.AuditTime != null)
            .Select(it => new EmpTransferEmployeeListOutput
            {
                confrimJoinTime = it.AuditTime,
                //reason = it.Reason,
                transferOrganizeIdName = SqlFunc.Subqueryable<OrganizeEntity>().Where(ddd => ddd.Id == it.TransferOrganizeId).Select(ddd => ddd.FullName),
                transferPositionIdName = SqlFunc.Subqueryable<PositionEntity>().Where(ddd => ddd.Id == it.TransferPositionId).Select(ddd => ddd.FullName),
                //reason = it.Reason,
                transferTime = it.TransferTime,
                organizeIdName = SqlFunc.Subqueryable<OrganizeEntity>().Where(ddd => ddd.Id == it.OrganizeId).Select(ddd => ddd.FullName),
                positionIdName = SqlFunc.Subqueryable<PositionEntity>().Where(ddd => ddd.Id == it.PositionId).Select(ddd => ddd.FullName),
            }).ToListAsync();

        list1.AddRange(list2.Select(x =>
        {
            List<string> strings = new List<string>();
            if (!string.IsNullOrEmpty(x.transferOrganizeIdName) && x.transferOrganizeIdName != x.organizeIdName)
            {
                strings.Add($"部门调整为[{x.transferOrganizeIdName}]");
            }
            if (!string.IsNullOrEmpty(x.transferPositionIdName) && x.transferPositionIdName != x.positionIdName)
            {
                strings.Add($"岗位调整为[{x.transferPositionIdName}]");
            }


            return new EmpEmployeeChangeLogOutput
            {
                operateTime = x.confrimJoinTime,
                time = x.transferTime,
                tag = "调岗",
                description = string.Join(",", strings)
            };
        }));

        var list3 = await _repository.Context.Queryable<EmpDismissionEmployeeEntity>()
            .Where(w => w.EmployeeId == output.id && w.AuditTime != null)
            .Select(it => new EmpEmployeeChangeLogOutput
            {
                time = it.LastWorkDay,
                operateTime = it.AuditTime,
                description = it.Reason,
                tag = "离职"
            }).ToListAsync();

        list1.AddRange(list3);

        var list4 = await _repository.Context.Queryable<EmpProbationEmployeeEntity>()
            .Where(w => w.EmployeeId == output.id && w.AuditTime != null)
            .Select(it => new EmpEmployeeChangeLogOutput
            {
                time = it.RegularTime,
                operateTime = it.AuditTime,
                description = it.Remark,
                tag = "转正"
            }).ToListAsync();

        list1.AddRange(list4);

        output.empEmployeeChangeList = list1.OrderByDescending(x => x.time).OrderByDescending(x => x.operateTime).ToList();

    }
}