using Microsoft.AspNetCore.Mvc;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DynamicApiController;
using QT.SDMS.Entitys.Dto.Marketer;
using QT.SDMS.Entitys.Entity;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.Permission;
using SqlSugar;

namespace QT.SDMS;

/// <summary>
/// 业务实现：营销人员.
/// </summary>
[ApiDescriptionSettings("售电系统", Tag = "销售人员管理", Name = "Marketer", Order = 300)]
[Route("api/sdms/marketer")]
public class MarketerService : QTBaseService<MarketerEntity, MarketerCrInput, MarketerUpInput, MarketerOutput, MarketerListQueryInput, MarketerListOutput>, IDynamicApiController
{
    private readonly IUsersCurrentService _usersCurrentService;

    public MarketerService(ISqlSugarRepository<MarketerEntity> repository, ISqlSugarClient context, IUserManager userManager, IUsersCurrentService usersCurrentService) : base(repository, context, userManager)
    {
        _usersCurrentService = usersCurrentService;
    }

    protected override async Task BeforeCreate(MarketerCrInput input, MarketerEntity entity)
    {
        await base.BeforeCreate(input, entity);
    }

    public override async Task<MarketerOutput> GetInfo(string id)
    {
        var data = await base.GetInfo(id);
        //data.items = await _repository.Context.Queryable<MaintenanceRecordEntity>()
        //    .LeftJoin<MaterialEntity>((x, a) => x.Mid == a.Id)
        //    .Where(x => x.FId == id)
        //    .Select<MaintenanceRecordListOutput>((x, a) => new MaintenanceRecordListOutput
        //    {
        //        midName = a.Name,
        //        midCode = a.Code,
        //        midSpec = a.Spec,
        //        midUnit = a.Unit                 
        //    }, true).ToListAsync();

        data.miniProgramQRCode = await _usersCurrentService.GetMiniProgramQRCode(data.userId);

        return data;
    }

    protected override async Task<SqlSugarPagedList<MarketerListOutput>> GetPageList([FromQuery] MarketerListQueryInput input)
    {
        var data = await _repository.Context.Queryable<MarketerEntity>()
            .WhereIF(input.pid.IsNotEmptyOrNull(), it => it.ManagerId == input.pid)
            //.WhereIF(!string.IsNullOrEmpty(input.keyword), it => SqlFunc.Subqueryable<UserEntity>().Where(dd=>dd.Id == it.UserId && dd.RealName.Contains(input.keyword)).Any()
            //|| SqlFunc.Subqueryable<UserEntity>().Where(dd => dd.Id == it.ManagerId && dd.RealName.Contains(input.keyword)).Any())
            .Select<MarketerListOutput>(it => new MarketerListOutput
            {
                userIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == it.UserId).Select(ddd => ddd.RealName) ?? "",
                managerIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == it.ManagerId).Select(ddd => ddd.RealName) ?? ""
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);

        var tree = data.list.ToList();
        if (input.keyword.IsNotEmptyOrNull())
        {
            tree = tree.TreeWhere(x => x.userIdName.Contains(input.keyword) || x.managerIdName.Contains(input.keyword), x => x.userId, x => x.managerId);
        }
        var parentId = input.pid ?? "0";
        data.list = tree.ToTree(x => x.userId, parentId);
        return data;
    }

    public override Task<PageResult<MarketerListOutput>> GetList([FromQuery] MarketerListQueryInput input)
    {
        input.pageSize = 9999;
        return base.GetList(input);
    }

    /// <summary>
    /// 获取下拉框.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="category">菜单分类（参数有Web,App），默认显示所有分类.</param>
    /// <returns></returns>
    [HttpGet("selector/{id}")]
    public async Task<dynamic> GetSelector(string id)
    {
        var treeList = await _repository.AsQueryable()
            .InnerJoin<UserEntity>((a, b) => a.UserId == b.Id)
            .Select((a, b) => new MarketerTreeListOutput
            {
                id = b.Id,
                fullName = b.RealName,
                parentId = a.ManagerId
            })
            .ToListAsync();

        if (!id.Equals("0"))
            treeList.RemoveAll(x => x.id == id);
        return new { list = treeList.ToTree("-1") };
    }

    /// <summary>
    /// 获取菜单列表（下拉框）.
    /// </summary>
    /// <param name="category">菜单分类（参数有Web,App）.</param>
    /// <returns></returns>
    [HttpGet("selector/All")]
    public async Task<dynamic> GetSelectorAll()
    {
        var treeList = await _repository.AsQueryable()
             .InnerJoin<UserEntity>((a, b) => a.UserId == b.Id)
             .Select((a, b) => new MarketerTreeListOutput
             {
                 id = b.Id,
                 fullName = b.RealName,
                 parentId = a.ManagerId
             })
             .ToListAsync();
        //if (!string.IsNullOrEmpty(category))
        //    data = data.FindAll(x => x.Category == category); 
        return new { list = treeList.ToTree("-1") };
    }
}