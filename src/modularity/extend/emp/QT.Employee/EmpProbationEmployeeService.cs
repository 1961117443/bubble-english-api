using Microsoft.AspNetCore.Mvc;
using QT.Common.Filter;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Emp.Entitys.Dto.EmpDismissionEmployee;
using QT.Emp.Entitys;
using QT.Emp.Interfaces;
using QT.Systems.Entitys.Permission;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QT.Common.Core.Manager;
using QT.Common.Security;
using QT.Common.Extension;
using QT.Emp.Entitys.Dto.EmpProbationEmployee;
using QT.Common.Const;

namespace QT.Emp;

/// <summary>
/// 业务实现：转正管理.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Employee, Tag = "Emp", Name = "EmpProbationEmployee", Order = 200)]
[Route("api/Emp/[controller]")]
public class EmpProbationEmployeeService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<EmpProbationEmployeeEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly ITenant _db;

    /// <summary>
    /// 初始化一个<see cref="EmpDismissionEmployeeService"/>类型的新实例.
    /// </summary>
    public EmpProbationEmployeeService(
        ISqlSugarRepository<EmpProbationEmployeeEntity> repository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = repository;
        _userManager = userManager;
        _db = context.AsTenant();
    }

    /// <summary>
    /// 获取离职管理列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] EmpProbationEmployeeListQueryInput input)
    {
        List<DateTime> queryRregularTime = input.regularTime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startRregularTime = queryRregularTime?.First();
        DateTime? endRregularTime = queryRregularTime?.Last();
        var data = await _repository.Context.Queryable<EmpProbationEmployeeEntity>()
            //.WhereIF(input.type == 0, it => it.AuditTime == null)
            //.WhereIF(input.type == 1, it => it.AuditTime != null)
            .WhereIF(queryRregularTime != null, it => SqlFunc.Between(it.RegularTime, startRregularTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endRregularTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new EmpProbationEmployeeListOutput
            {
                id = it.Id,
                employeeId = it.EmployeeId,
                regularTime = it.RegularTime,
                remark = it.Remark,
                organizeIdName = SqlFunc.Subqueryable<OrganizeEntity>().Where(ddd => ddd.Id == it.OrganizeId).Select(ddd => ddd.FullName),
                positionIdName = SqlFunc.Subqueryable<PositionEntity>().Where(ddd => ddd.Id == it.PositionId).Select(ddd => ddd.FullName),
                employeeIdName = SqlFunc.Subqueryable<EmpEmployeeEntity>().Where(x => x.Id == it.EmployeeId).Select(x => x.Name),
                confrimJoinTime = SqlFunc.Subqueryable<EmpEmployeeEntity>().Where(x => x.Id == it.EmployeeId).Select(x => x.ConfrimJoinTime),
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<EmpProbationEmployeeListOutput>.SqlSugarPageResult(data);
    }
}
