using Mapster;
using Microsoft.AspNetCore.Mvc;
using QT.Common;
using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Emp.Entitys;
using QT.Emp.Entitys.Dto.EmpDismissionEmployee;
using QT.Emp.Entitys.Dto.EmpEmployee;
using QT.Emp.Entitys.Dto.EmpTransferEmployee;
using QT.Emp.Interfaces;
using QT.Employee.Entitys.Dto.EmpChangeEmployee;
using QT.FriendlyException;
using QT.Systems.Entitys.Permission;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Emp;

/// <summary>
/// 业务实现：员工变更记录.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Employee, Tag = "Emp", Name = "EmpChangeEmployee", Order = 200)]
[Route("api/Emp/[controller]")]
public class EmpChangeEmployeeService : IEmpChangeEmployeeService,IDynamicApiController, IScoped
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<EmpChangeEmployeeEntity> _repository;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="EmpEmployeeService"/>类型的新实例.
    /// </summary>
    public EmpChangeEmployeeService(
        ISqlSugarRepository<EmpChangeEmployeeEntity> empEmployeeRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = empEmployeeRepository;
        _db = context.AsTenant();
        _userManager = userManager;
    }


    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] EmpChangeEmployeeCrInput input)
    {
        var entity = input.Adapt<EmpChangeEmployeeEntity>();
        entity.Id = SnowflakeIdHelper.NextId();

        var employee = await _repository.Context.Queryable<EmpEmployeeEntity>().InSingleAsync(entity.EmployeeId);
        _repository.Context.Tracking(employee);

        if (!string.IsNullOrEmpty(input.propertyJson))
        {
            var json = input.propertyJson.ToObject<Dictionary<string,object>>();
            foreach (var item in json)
            {
                var p = EntityHelper<EmpEmployeeEntity>.InstanceProperties.FirstOrDefault(x => x.Name.Equals(item.Key, StringComparison.OrdinalIgnoreCase));
                if (p!=null)
                {
                    p.SetValue(employee, Convert.ChangeType(item.Value, p.PropertyType));
                }
            }
        }

        await _db.TranExecute(async () =>
        {
            await _repository.Context.Insertable<EmpChangeEmployeeEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            await _repository.Context.Updateable<EmpEmployeeEntity>(employee).ExecuteCommandAsync();
        });
     
    }

    /// <summary>
    /// 获取员工变更记录
    /// </summary>
    /// <param name="id"></param>
    //[HttpGet("Actions/{id}/ChangeList")]
    [NonAction]
    public  async Task<List<EmpEmployeeChangeLogOutput>> ChangeList(string id)
    {
        List<EmpEmployeeChangeLogOutput> result = new List<EmpEmployeeChangeLogOutput>();

        var employee = await _repository.Context.Queryable<EmpEmployeeEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        var list = await _repository.AsQueryable().Where(x => x.EmployeeId == id).ToListAsync();

        foreach (var item in list)
        {
            var entity = new EmpEmployeeChangeLogOutput
            {
                time = item.OperateTime,
                operateTime = item.CreatorTime,
                description = "",
                tag = ""
            };
            result.Add(entity);
            switch (item.ChangeType)
            {
                case Employee.Entitys.Dto.EmpChangeEmployee.ChangeType.Common:
                    entity.tag = "日常更新";
                    break;
                case Employee.Entitys.Dto.EmpChangeEmployee.ChangeType.Salary:
                    entity.tag = "调薪";
                    var newItem = item.PropertyJson.ToObject<ChangeSalaryForm>();
                    //entity.description = $"工资由【{newItem.lastSalary}】调整为【{newItem.salary}】";
                    entity.description = $"工资调整为【{newItem.salary}】";
                    break;
                default:
                    break;
            }
        }

        var list1 = await _repository.Context.Queryable<EmpEntryEmployeeEntity>()
         .Where(w => w.Mobile == employee.Mobile && w.AuditTime != null)
         .Select(w => new EmpEmployeeChangeLogOutput
         {
             time = w.ConfrimJoinTime,
             operateTime = w.AuditTime,
             tag = "入职",
             description = "操作入职"
         }).ToListAsync();

        var list2 = await _repository.Context.Queryable<EmpTransferEmployeeEntity>()
            .Where(w => w.EmployeeId == id && w.AuditTime != null)
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
            .Where(w => w.EmployeeId == id && w.AuditTime != null)
            .Select(it => new EmpEmployeeChangeLogOutput
            {
                time = it.LastWorkDay,
                operateTime = it.AuditTime,
                description = it.Reason,
                tag = "离职"
            }).ToListAsync();

        list1.AddRange(list3);

        var list4 = await _repository.Context.Queryable<EmpProbationEmployeeEntity>()
            .Where(w => w.EmployeeId == id && w.AuditTime != null)
            .Select(it => new EmpEmployeeChangeLogOutput
            {
                time = it.RegularTime,
                operateTime = it.AuditTime,
                description = it.Remark,
                tag = "转正"
            }).ToListAsync();

        list1.AddRange(list4);

        var list5 = (await _repository.Context.Queryable<EmpInterviewEmployeeEntity>()
        .Where(w => w.Mobile == employee.Mobile && w.AuditTime != null)
        .ToListAsync())
        .Select(w => new EmpEmployeeChangeLogOutput
        {
            time = w.InterviewTime,
            operateTime = w.AuditTime,
            tag = "面试",
            description = $"面试{(w.Result == "1" ? "通过" : "不通过")}"
        }).ToList();

        list1.AddRange(list5);

        result.AddRange(list1);

        return result.OrderByDescending(x => x.time).OrderByDescending(x => x.operateTime).ToList(); ;
    }
}
