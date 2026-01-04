using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.JsonSerialization;
using QT.Systems.Entitys.Dto.Organize;
using QT.Systems.Entitys.Dto.SysConfig;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.Permission;
using QT.Systems.Interfaces.System;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Yitter.IdGenerator;
using QT.Common.Extension;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Crypto;

namespace QT.Systems;

/// <summary>
/// 机构管理
/// 组织架构：公司》部门》岗位》用户
/// </summary>
[ApiDescriptionSettings(Tag = "Permission", Name = "Organize", Order = 165)]
[Route("api/permission/[controller]")]
public class OrganizeService : IOrganizeService, IDynamicApiController, IScoped
{
    /// <summary>
    /// 基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<OrganizeEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 系统配置.
    /// </summary>
    private readonly ISysConfigService _sysConfigService;

    /// <summary>
    /// 第三方同步.
    /// </summary>
    private readonly ISynThirdInfoService _synThirdInfoService;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    private List<OrganizeEntity> _organizeEntities;

    /// <summary>
    /// 初始化一个<see cref="OrganizeService"/>类型的新实例.
    /// </summary>
    public OrganizeService(
        ISqlSugarRepository<OrganizeEntity> repository,
        ISysConfigService sysConfigService,
        ISynThirdInfoService synThirdInfoService,
        IUserManager userManager,
        ISqlSugarClient context)
    {
        _repository = repository;
        _sysConfigService = sysConfigService;
        _synThirdInfoService = synThirdInfoService;
        _userManager = userManager;
        _db = context.AsTenant();
    }

    #region GET

    /// <summary>
    /// 获取机构列表.
    /// </summary>
    /// <param name="input">关键字参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] KeywordInput input)
    {
        List<OrganizeEntity>? data = await _repository.AsQueryable().Where(t => t.Category.Equals("company") && t.DeleteMark == null).OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).ToListAsync();
        if (!string.IsNullOrEmpty(input.keyword))
            data = data.TreeWhere(t => t.FullName.Contains(input.keyword) || t.EnCode.Contains(input.keyword), t => t.Id, t => t.ParentId);
        List<OrganizeListOutput>? treeList = data.Adapt<List<OrganizeListOutput>>();
        return new { list = treeList.OrderBy(x => x.sortCode).ToList().ToTree("-1") };
    }

    /// <summary>
    /// 获取下拉框.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector/{id}")]
    public async Task<dynamic> GetSelector(string id)
    {
        List<OrganizeEntity>? data = await _repository.AsQueryable().Where(t => t.Category.Equals("company") && t.DeleteMark == null).OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).ToListAsync();
        if (!"0".Equals(id))
        {
            OrganizeEntity? info = data.Find(it => it.Id == id);
            data.Remove(info);
        }

        List<OrganizeListOutput>? treeList = data.Adapt<List<OrganizeListOutput>>();
        treeList.ForEach(item =>
        {
            if (item != null && item.category.Equals("company"))
            {
                item.icon = "icon-qt icon-qt-tree-organization3";
            }
        });
        return new { list = treeList.OrderBy(x => x.sortCode).ToList().ToTree("-1") };
    }

    /// <summary>
    /// 获取树形.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Tree")]
    public async Task<dynamic> GetTree()
    {
        List<OrganizeEntity>? data = await _repository.AsQueryable().Where(t => t.Category.Equals("company") && t.DeleteMark == null).OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).ToListAsync();
        List<OrganizeTreeOutput>? treeList = data.Adapt<List<OrganizeTreeOutput>>();
        return new { list = treeList.OrderBy(x => x.sortCode).ToList().ToTree("-1") };
    }

    /// <summary>
    /// 获取信息.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        OrganizeEntity? entity = await _repository.SingleAsync(p => p.Id == id);
        if (entity.PropertyJson.IsNullOrEmpty())
        {
            entity.PropertyJson = "{}";
        }
        return entity.Adapt<OrganizeInfoOutput>();
    }

    /// <summary>
    /// 获取下拉框.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector/Company")]
    public async Task<dynamic> GetSelector()
    {
        List<OrganizeEntity>? data = await _repository.AsQueryable().Where(t => t.Category.Equals("company") && t.DeleteMark == null)
            .OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).ToListAsync();

        return new { list = data.Select(it => new { id = it.Id, name = it.FullName }).ToList() };
    }

    #endregion

    #region POST

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] OrganizeCrInput input)
    {
        if (!_userManager.DataScope.Any(it => it.organizeId == input.parentId && it.Add) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);
        if (await _repository.AnyAsync(o => o.EnCode == input.enCode && o.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D2008);
        if (await _repository.AnyAsync(o => o.ParentId == input.parentId && o.FullName == input.fullName && o.Category == "company" && o.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D2009);
        OrganizeEntity? entity = input.Adapt<OrganizeEntity>();
        entity.Id = YitIdHelper.NextId().ToString();
        entity.EnabledMark = input.enabledMark;
        entity.Category = "company";
        entity.PropertyJson = JSON.Serialize(input.propertyJson);

        #region 处理 上级ID列表 存储

        List<string>? idList = new List<string>();
        idList.Add(entity.Id);
        if (entity.ParentId != "-1")
        {
            List<string>? ids = _repository.Context.Queryable<OrganizeEntity>().ToParentList(it => it.ParentId, entity.ParentId).Select(x => x.Id).ToList();
            idList.AddRange(ids);
        }

        idList.Reverse();
        entity.OrganizeIdTree = string.Join(",", idList);
        #endregion

        OrganizeEntity? isOk = await _repository.Context.Insertable(entity).CallEntityMethod(m => m.Create()).ExecuteReturnEntityAsync();
        _ = isOk ?? throw Oops.Oh(ErrorCode.D2012);

        #region 第三方同步

        try
        {
            SysConfigOutput? sysConfig = await _sysConfigService.GetInfo();
            List<OrganizeListOutput>? orgList = new List<OrganizeListOutput>();
            orgList.Add(entity.Adapt<OrganizeListOutput>());
            if (sysConfig.dingSynIsSynOrg)
                await _synThirdInfoService.SynDep(2, 1, sysConfig, orgList);
            if (sysConfig.qyhIsSynOrg)
                await _synThirdInfoService.SynDep(1, 1, sysConfig, orgList);
        }
        catch (Exception)
        {
        }

        #endregion
    }

    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <param name="input">参数.</param>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] OrganizeUpInput input)
    {
        OrganizeEntity? oldEntity = await _repository.SingleAsync(it => it.Id == id);
        if (oldEntity.ParentId != input.parentId && !_userManager.DataScope.Any(it => it.organizeId == oldEntity.ParentId && it.Edit) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        if (oldEntity.ParentId != input.parentId && !_userManager.DataScope.Any(it => it.organizeId == input.parentId && it.Edit) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        if (!_userManager.DataScope.Any(it => it.organizeId == id && it.Edit == true) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        if (input.parentId.Equals(id))
            throw Oops.Oh(ErrorCode.D2001);
        if (input.parentId.Equals("-1") && !oldEntity.ParentId.Equals("-1") && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        // 父id不能为自己的子节点
        List<string>? childIdListById = await GetChildIdListWithSelfById(id);
        if (childIdListById.Contains(input.parentId))
            throw Oops.Oh(ErrorCode.D2001);
        if (await _repository.AnyAsync(o => o.EnCode == input.enCode && o.Id != id && o.DeleteMark == null && o.Id != id))
            throw Oops.Oh(ErrorCode.D2008);
        if (await _repository.AnyAsync(o => o.ParentId == input.parentId && o.FullName == input.fullName && o.Id != id && o.DeleteMark == null && o.Id != id))
            throw Oops.Oh(ErrorCode.D2009);
        OrganizeEntity? entity = input.Adapt<OrganizeEntity>();
        entity.LastModifyTime = DateTime.Now;
        entity.LastModifyUserId = _userManager.UserId;
        entity.PropertyJson = JSON.Serialize(input.propertyJson);

        try
        {
            // 开启事务
            _db.BeginTran();

            #region 处理 上级ID列表 存储

            if (string.IsNullOrWhiteSpace(oldEntity.OrganizeIdTree) || entity.ParentId != oldEntity.ParentId)
            {
                List<string>? idList = new List<string>();
                idList.Add(entity.Id);
                if (entity.ParentId != "-1")
                {
                    List<string>? ids = _repository.Context.Queryable<OrganizeEntity>().ToParentList(it => it.ParentId, entity.ParentId).Select(x => x.Id).ToList();
                    idList.AddRange(ids);
                }

                idList.Reverse();
                entity.OrganizeIdTree = string.Join(",", idList);

                // 如果上级结构 变动 ，需要更改所有包含 该组织的id 的结构
                if (entity.OrganizeIdTree != oldEntity.OrganizeIdTree)
                {
                    List<OrganizeEntity>? oldEntityList = await _repository.ToListAsync(x => x.OrganizeIdTree.Contains(oldEntity.Id) && x.Id != oldEntity.Id);
                    oldEntityList.ForEach(item =>
                    {
                        string? childList = item.OrganizeIdTree.Split(oldEntity.Id).LastOrDefault();
                        item.OrganizeIdTree = entity.OrganizeIdTree + childList;
                    });

                    await _repository.Context.Updateable(oldEntityList).UpdateColumns(x => x.OrganizeIdTree).ExecuteCommandAsync(); // 批量修改 父级组织
                }
            }

            #endregion

            await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();

            _db.CommitTran();
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw Oops.Oh(ErrorCode.D2010);
        }

        #region 第三方同步

        try
        {
            SysConfigOutput? sysConfig = await _sysConfigService.GetInfo();
            List<OrganizeListOutput>? orgList = new List<OrganizeListOutput>();
            entity.Category = "company";
            orgList.Add(entity.Adapt<OrganizeListOutput>());
            if (sysConfig.dingSynIsSynOrg) await _synThirdInfoService.SynDep(2, 1, sysConfig, orgList);
            if (sysConfig.qyhIsSynOrg) await _synThirdInfoService.SynDep(1, 1, sysConfig, orgList);
        }
        catch (Exception)
        {
        }

        #endregion
    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        if (!_userManager.DataScope.Any(it => it.organizeId == id && it.Delete == true) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        // 该机构下有机构，则不能删
        if (await _repository.AnyAsync(o => o.ParentId.Equals(id) && o.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D2005);

        // 该机构下有岗位，则不能删
        if (await _repository.Context.Queryable<PositionEntity>().AnyAsync(p => p.OrganizeId.Equals(id) && p.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D2006);

        // 该机构下有用户，则不能删
        if (await _repository.Context.Queryable<UserRelationEntity>()
            .Where(x => x.ObjectType == "Organize" && x.ObjectId == id)
            .Where(x => SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == x.UserId && u.DeleteMark == null).Any())
            .AnyAsync())
            throw Oops.Oh(ErrorCode.D2004);

        // 该机构下有角色，则不能删
        if (await _repository.Context.Queryable<OrganizeRelationEntity>().Where(x => x.OrganizeId == id && x.ObjectType == "Role").AnyAsync())
            throw Oops.Oh(ErrorCode.D2020);

        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.Context.Updateable<OrganizeEntity>().SetColumns(it => new OrganizeEntity()
            {
                DeleteMark = 1,
                DeleteTime = SqlFunc.GetDate(),
                DeleteUserId = _userManager.UserId,
            }).Where(it => it.Id == id && it.DeleteMark == null).ExecuteCommandAsync();

            // 删除该组织和角色关联数据
            await _repository.Context.Deleteable<OrganizeRelationEntity>().Where(x => x.OrganizeId == id && x.ObjectType == "Role").ExecuteCommandAsync();

            _db.CommitTran();
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw Oops.Oh(ErrorCode.D2012);
        }

        #region 第三方同步
        try
        {
            SysConfigOutput? sysConfig = await _sysConfigService.GetInfo();
            if (sysConfig.dingSynIsSynOrg)
                await _synThirdInfoService.DelSynData(2, 1, sysConfig, id);

            if (sysConfig.qyhIsSynOrg)
                await _synThirdInfoService.DelSynData(1, 1, sysConfig, id);

        }
        catch (Exception)
        {
        }
        #endregion
    }

    /// <summary>
    /// 根据选择的方式删除.
    /// </summary>
    /// <param name="way">0：检查引用状态，1：删除子表数据，2：保留子表数据</param>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpDelete("{way}/{id}")]
    public async Task DeleteWay(int way, string id)
    {
        if (!_userManager.DataScope.Any(it => it.organizeId == id && it.Delete == true) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        if (way == 0)
        {
            await Delete(id);
            return;
        }

        #region 用户和角色
        var userRelationList = await _repository.Context.Queryable<UserRelationEntity>()
           .Where(x => x.ObjectType == "Organize" && x.ObjectId == id)
           .Where(x => SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == x.UserId && u.DeleteMark == null).Any())
           .ToListAsync();

        var organizeRelationList = await _repository.Context.Queryable<OrganizeRelationEntity>().Where(x => x.OrganizeId == id && x.ObjectType == "Role").ToListAsync();
        #endregion

        try
        {
            // 开启事务
            _db.BeginTran();

            // 强制删除的情况：所有都删掉
            if (way == 1)
            {
                var organizeTreeList = await _repository.Context.Queryable<OrganizeEntity>().Where(it => it.DeleteMark == null).ToChildListAsync(it => it.ParentId, id);

                var organizeList = organizeTreeList.Select(it => it.Id).ToList() ?? new List<string>();
                //organizeList.Add(id);
                var positionList = await _repository.Context.Queryable<PositionEntity>().Where(p => (organizeList.Contains(p.OrganizeId)) && p.DeleteMark == null).ToListAsync();

                await RelationDelete(organizeList, positionList);
            }
            else if (way == 2)
            {
                // 保留的情况：保留岗位数据、组织数据
                await _repository.Context.Updateable<OrganizeEntity>().SetColumns(it => new OrganizeEntity()
                {
                    ParentId = ""
                }).Where(it => it.ParentId.Equals(id) && it.DeleteMark == null).ExecuteCommandAsync();

                await _repository.Context.Updateable<PositionEntity>().SetColumns(it => new PositionEntity()
                {
                    OrganizeId = ""
                }).Where(it => it.OrganizeId.Equals(id) && it.DeleteMark == null).ExecuteCommandAsync();
            }



            await _repository.Context.Updateable<OrganizeEntity>().SetColumns(it => new OrganizeEntity()
            {
                DeleteMark = 1,
                DeleteTime = SqlFunc.GetDate(),
                DeleteUserId = _userManager.UserId,
            }).Where(it => it.Id == id && it.DeleteMark == null).ExecuteCommandAsync();

            // 删除该组织和用户关联的数据
            if (userRelationList.IsAny())
            {
                await _repository.Context.Deleteable<UserRelationEntity>(userRelationList).ExecuteCommandAsync();

                if (way == 1)
                {
                    var userList = userRelationList.Select(it => it.UserId).ToArray();

                    await _repository.Context.Updateable<UserEntity>().SetColumns(it => new UserEntity()
                    {
                        DeleteMark = 1,
                        DeleteTime = SqlFunc.GetDate(),
                        DeleteUserId = _userManager.UserId,
                    }).Where(it => userList.Contains(it.Id) && it.DeleteMark == null && it.IsAdministrator == 0).ExecuteCommandAsync();

                }
            }

            // 删除该组织和角色关联数据
            if (organizeRelationList.IsAny())
            {
                await _repository.Context.Deleteable<OrganizeRelationEntity>(organizeRelationList).ExecuteCommandAsync();

                if (way == 1)
                {
                    var roleList = organizeRelationList.Select(it => it.ObjectId).ToArray();

                    await _repository.Context.Updateable<RoleEntity>().SetColumns(it => new RoleEntity()
                    {
                        DeleteMark = 1,
                        DeleteTime = SqlFunc.GetDate(),
                        DeleteUserId = _userManager.UserId,
                    }).Where(it => roleList.Contains(it.Id) && it.DeleteMark == null).ExecuteCommandAsync();

                }
            }

            _db.CommitTran();
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw Oops.Oh(ErrorCode.D2012);
        }

        #region 第三方同步
        try
        {
            SysConfigOutput? sysConfig = await _sysConfigService.GetInfo();
            if (sysConfig.dingSynIsSynOrg)
                await _synThirdInfoService.DelSynData(2, 1, sysConfig, id);

            if (sysConfig.qyhIsSynOrg)
                await _synThirdInfoService.DelSynData(1, 1, sysConfig, id);

        }
        catch (Exception)
        {
        }
        #endregion
    }

    /// <summary>
    /// 更新状态.
    /// </summary>
    /// <param name="id">主键</param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/State")]
    public async Task UpdateState(string id)
    {
        if (!_userManager.DataScope.Any(it => it.organizeId == id && it.Edit == true) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        if (!await _repository.AnyAsync(u => u.Id == id && u.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D2002);

        int isOk = await _repository.Context.Updateable<OrganizeEntity>().SetColumns(it => new OrganizeEntity()
        {
            EnabledMark = SqlFunc.IIF(it.EnabledMark == 1, 0, 1),
            LastModifyUserId = _userManager.UserId,
            LastModifyTime = SqlFunc.GetDate()
        }).Where(it => it.Id == id).ExecuteCommandAsync();

        if (!(isOk > 0))
            throw Oops.Oh(ErrorCode.D2011);
    }

    #endregion

    #region PublicMethod

    /// <summary>
    /// 是否机构主管.
    /// </summary>
    /// <param name="userId">用户ID.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<bool> GetIsManagerByUserId(string userId)
    {
        return await _repository.AnyAsync(o => o.EnabledMark == 1 && o.DeleteMark == null && o.ManagerId == userId);
    }

    /// <summary>
    /// 获取机构列表(其他服务使用).
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<List<OrganizeEntity>> GetListAsync()
    {
        return await _repository.AsQueryable().Where(t => t.EnabledMark == 1 && t.DeleteMark == null).OrderBy(o => o.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).ToListAsync();
    }

    /// <summary>
    /// 获取公司列表(其他服务使用).
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<List<OrganizeEntity>> GetCompanyListAsync()
    {
        return await _repository.AsQueryable().Where(t => t.Category.Equals("company") && t.EnabledMark == 1 && t.DeleteMark == null).OrderBy(o => o.SortCode).ToListAsync();
    }

    /// <summary>
    /// 下属机构.
    /// </summary>
    /// <param name="organizeId">机构ID.</param>
    /// <param name="isAdmin">是否管理员.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<string[]> GetSubsidiary(string organizeId, bool isAdmin)
    {
        List<OrganizeEntity>? data = await _repository.AsQueryable().Where(o => o.DeleteMark == null && o.EnabledMark == 1).OrderBy(o => o.SortCode).ToListAsync();
        if (!isAdmin)
            data = data.TreeChildNode(organizeId, t => t.Id, t => t.ParentId);
        return data.Select(m => m.Id).ToArray();
    }

    /// <summary>
    /// 下属机构.
    /// </summary>
    /// <param name="organizeId">机构ID.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<string>> GetSubsidiary(string organizeId)
    {
        List<OrganizeEntity>? data = await _repository.AsQueryable().Where(o => o.DeleteMark == null && o.EnabledMark == 1).OrderBy(o => o.SortCode).ToListAsync();
        data = data.TreeChildNode(organizeId, t => t.Id, t => t.ParentId);
        return data.Select(m => m.Id).ToList();
    }

    /// <summary>
    /// 根据节点Id获取所有子节点Id集合，包含自己.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<string>> GetChildIdListWithSelfById(string id)
    {
        List<string>? childIdList = await _repository.AsQueryable().Where(u => u.ParentId.Contains(id) && u.DeleteMark == null).Select(u => u.Id).ToListAsync();
        childIdList.Add(id);
        return childIdList;
    }

    /// <summary>
    /// 获取机构成员列表.
    /// </summary>
    /// <param name="organizeId">机构ID.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<OrganizeMemberListOutput>> GetOrganizeMemberList(string organizeId)
    {
        List<OrganizeMemberListOutput>? output = new List<OrganizeMemberListOutput>();
        if (organizeId.Equals("0"))
        {
            List<OrganizeEntity>? data = await _repository.AsQueryable().Where(o => o.ParentId.Equals("-1") && o.DeleteMark == null && o.EnabledMark == 1).OrderBy(o => o.SortCode).ToListAsync();
            data.ForEach(o =>
            {
                output.Add(new OrganizeMemberListOutput
                {
                    id = o.Id,
                    fullName = o.FullName,
                    enabledMark = o.EnabledMark,
                    type = o.Category,
                    icon = "icon-qt icon-qt-tree-organization3",
                    hasChildren = true,
                    isLeaf = false
                });
            });
        }
        else
        {
            List<UserEntity>? userList = await _repository.Context.Queryable<UserEntity>().Where(u => SqlFunc.ToString(u.OrganizeId).Equals(organizeId) && u.EnabledMark > 0 && u.DeleteMark == null).OrderBy(o => o.SortCode).ToListAsync();
            userList.ForEach(u =>
            {
                output.Add(new OrganizeMemberListOutput()
                {
                    id = u.Id,
                    fullName = u.RealName + "/" + u.Account,
                    enabledMark = u.EnabledMark,
                    type = "user",
                    icon = "icon-qt icon-qt-tree-user2",
                    hasChildren = false,
                    isLeaf = true
                });
            });
            List<OrganizeEntity>? departmentList = await _repository.AsQueryable().Where(o => o.ParentId.Equals(organizeId) && o.DeleteMark == null && o.EnabledMark == 1).OrderBy(o => o.SortCode).ToListAsync();
            departmentList.ForEach(o =>
            {
                output.Add(new OrganizeMemberListOutput()
                {
                    id = o.Id,
                    fullName = o.FullName,
                    enabledMark = o.EnabledMark,
                    type = o.Category,
                    icon = "icon-qt icon-qt-tree-department1",
                    hasChildren = true,
                    isLeaf = false
                });
            });
        }

        return output;
    }

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="Id"></param>
    /// <returns></returns>
    public async Task<OrganizeEntity> GetInfoById(string Id)
    {
        return await _repository.SingleAsync(p => p.Id == Id);
    }

    #endregion

    /// <summary>
    /// 删除引用数据
    /// </summary>
    /// <returns></returns>
    private async Task RelationDelete(List<string> organizeList, List<PositionEntity> positionList)
    {
        if (organizeList.IsAny())
        {
            // 逻辑删除所有的组织
            await _repository.Context.Updateable<OrganizeEntity>().SetColumns(it => new OrganizeEntity()
            {
                DeleteMark = 1,
                DeleteTime = SqlFunc.GetDate(),
                DeleteUserId = _userManager.UserId,
            }).Where(it => organizeList.Contains(it.Id) && it.DeleteMark == null).ExecuteCommandAsync();
        }

        if (positionList.IsAny())
        {
            var idList = positionList.Select(it => it.Id).ToList();
            // 逻辑删除所有的岗位
            await _repository.Context.Updateable<PositionEntity>().SetColumns(it => new PositionEntity()
            {
                DeleteMark = 1,
                DeleteTime = SqlFunc.GetDate(),
                DeleteUserId = _userManager.UserId,
            }).Where(it => idList.Contains(it.Id) && it.DeleteMark == null).ExecuteCommandAsync();
        }
    }
}