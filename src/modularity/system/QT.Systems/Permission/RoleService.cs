using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Models.User;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Dto.Role;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.Permission;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using NPOI.SS.Formula.PTG;

namespace QT.Systems;

/// <summary>
/// 业务实现：角色信息.
/// </summary>
[ApiDescriptionSettings(Tag = "Permission", Name = "Role", Order = 167)]
[Route("api/permission/[controller]")]
public class RoleService : IRoleService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<RoleEntity> _repository;

    /// <summary>
    /// 操作权限服务.
    /// </summary>
    private readonly IAuthorizeService _authorizeService;

    /// <summary>
    /// 缓存管理器.
    /// </summary>
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 初始化一个<see cref="RoleService"/>类型的新实例.
    /// </summary>
    public RoleService(
        ISqlSugarRepository<RoleEntity> repository,
        IAuthorizeService authorizeService,
        ICacheManager cacheManager,
        IUserManager userManager,
        ISqlSugarClient context)
    {
        _repository = repository;
        _authorizeService = authorizeService;
        _cacheManager = cacheManager;
        _userManager = userManager;
        _db = context.AsTenant();
    }

    #region GET

    /// <summary>
    /// 获取列表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] RoleListInput input)
    {
        List<OrganizeRelationEntity>? oIdList = new List<OrganizeRelationEntity>();

        // 获取组织角色关联 列表
        if (input.organizeId != "0")
        {
            oIdList = await _repository.Context.Queryable<OrganizeRelationEntity>()
                .WhereIF(input.organizeId != "0", x => x.OrganizeId == input.organizeId && x.ObjectType == "Role")
                .Select(x => new OrganizeRelationEntity { ObjectId = x.ObjectId, OrganizeId = x.OrganizeId }).ToListAsync();
        }

        SqlSugarPagedList<RoleListOutput>? list = await _repository.Context.Queryable<RoleEntity>()
            .WhereIF(input.organizeId == "0", a => a.GlobalMark == 1)
            .WhereIF(input.organizeId.IsNotEmptyOrNull() && input.organizeId != "0", a => oIdList.Select(x => x.ObjectId).Contains(a.Id))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), a => a.FullName.Contains(input.keyword) || a.EnCode.Contains(input.keyword))
            .Where(a => a.DeleteMark == null)
            .Select((a) => new RoleListOutput
            {
                id = a.Id,
                parentId = a.Type,
                type = SqlFunc.IIF(a.GlobalMark == 1, "全局", "组织"),
                enCode = a.EnCode,
                fullName = a.FullName,
                description = a.Description,
                enabledMark = a.EnabledMark,
                creatorTime = a.CreatorTime,
                sortCode = a.SortCode
            }).MergeTable().OrderBy(a => a.sortCode).OrderBy(a => a.creatorTime, OrderByType.Desc).ToPagedListAsync(input.currentPage, input.pageSize);

        #region 处理 角色 多组织

        // 获取组织角色关联 列表
        if (input.organizeId != "0")
        {
            oIdList = await _repository.Context.Queryable<OrganizeRelationEntity>().Select(x => new OrganizeRelationEntity { ObjectId = x.ObjectId, OrganizeId = x.OrganizeId }).ToListAsync();
            List<OrganizeEntity>? orgList = _repository.Context.Queryable<OrganizeEntity>().Where(x => x.DeleteMark == null).OrderBy(x => x.CreatorTime).ToList();
            foreach (RoleListOutput? item in list.list)
            {
                List<string>? roleOrgList = oIdList.Where(x => x.ObjectId == item.id).Select(x => x.OrganizeId).ToList(); // 获取角色组织集合
                List<string>? tree = orgList.Where(x => roleOrgList.Contains(x.Id)).Select(x => x.OrganizeIdTree).ToList(); // 获取组织树

                List<string>? infoList = new List<string>();

                tree.ForEach(treeItem =>
                {
                    if (treeItem.IsNotEmptyOrNull())
                    {
                        var orgNameList = new List<string>();
                        treeItem.Split(",").ToList().ForEach(it =>
                        {
                            var org = orgList.Find(x => x.Id == it);
                            if (org != null) orgNameList.Add(org.FullName);
                        });
                        infoList.Add(string.Join("/", orgNameList));
                    }
                });

                item.organizeInfo = string.Join(" ; ", infoList);
            }
        }

        #endregion
        return PageResult<RoleListOutput>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 获取下拉框(类型+角色).
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<dynamic> GetSelector()
    {
        // 获取所有组织 对应 的 角色id集合
        var ridList = await _repository.Context.Queryable<OrganizeRelationEntity>().Where(x => x.ObjectType == "Role")
            .Select(x => new { x.ObjectId, x.OrganizeId }).ToListAsync();

        // 获取 全局角色 和 组织角色
        List<RoleListOutput>? roleList = await _repository.Entities.Where(a => a.DeleteMark == null).Where(a => a.GlobalMark == 1 || ridList.Select(x => x.ObjectId).Contains(a.Id))
            .Select(a => new RoleListOutput
            {
                id = a.Id,
                parentId = a.GlobalMark.ToString(),
                fullName = a.FullName,
                enabledMark = a.EnabledMark,
                creatorTime = a.CreatorTime,
                type = "role",
                icon = "icon-qt icon-qt-global-role",
                sortCode = 0
            }).MergeTable().OrderBy(a => a.sortCode).OrderBy(a => a.creatorTime, OrderByType.Desc).ToListAsync();

        for (int i = 0; i < roleList.Count; i++) roleList[i].onlyId = "role_" + i;

        // 处理 组织角色
        roleList.Where(x => ridList.Select(x => x.ObjectId).Contains(x.id)).ToList().ForEach(item =>
        {
            var oolist = ridList.Where(x => x.ObjectId == item.id).ToList();

            for (int i = 0; i < oolist.Count; i++)
            {
                if (i == 0)
                {
                    item.parentId = oolist.FirstOrDefault().OrganizeId;
                }
                else
                {
                    // 该角色属于多个组织
                    RoleListOutput? newItem = item.ToObject<RoleListOutput>();
                    newItem.parentId = oolist[i].OrganizeId;
                    roleList.Add(newItem);
                }
            }
        });

        // 设置 全局 根目录
        List<RoleListOutput>? treeList = new List<RoleListOutput>() { new RoleListOutput() { id = "1", type = string.Empty, parentId = "-1", enCode = string.Empty, fullName = "全局", num = roleList.Count(x => x.parentId == "1") } };

        // 获取所有组织
        List<OrganizeEntity>? allOrgList = await _repository.Context.Queryable<OrganizeEntity>().ToListAsync();

        List<RoleListOutput>? organizeList = allOrgList.Select(x => new RoleListOutput()
        {
            id = x.Id,
            type = string.Empty,
            parentId = x.ParentId,
            organizeInfo = x.OrganizeIdTree,
            icon = x.Category == "company" ? "icon-qt icon-qt-tree-organization3" : "icon-qt icon-qt-tree-department1",
            fullName = x.FullName,
            sortCode=1
        }).ToList();
        treeList.AddRange(organizeList);

        for (int i = 0; i < treeList.Count; i++)
        {
            treeList[i].onlyId = "organizeList_" + i;
            treeList[i].num = roleList.Count(x => x.parentId == treeList[i].id);
        }

        treeList.Where(x => x.num > 0).ToList().ForEach(item =>
        {
            if (item.organizeInfo.IsNotEmptyOrNull())
            {
                treeList.Where(x => !x.id.Equals("1") && x.organizeInfo.IsNotEmptyOrNull()).Where(x => item.organizeInfo.Contains(x.id)).ToList().ForEach(it =>
                {
                    if (it != null && it.num < 1)
                        it.num = item.num;
                });
            }
        });

        var res = treeList.Where(x => x.num > 0).Union(roleList).OrderBy(x => x.sortCode).ToList().ToTree("-1");
        return new { list = OrderbyTree(res) };
    }

    /// <summary>
    /// 获取下拉框，有分级管理编辑权限(类型+角色).
    /// </summary>
    /// <returns></returns>
    [HttpGet("SelectorByPermission")]
    public async Task<dynamic> GetSelectorByPermission()
    {
        // 获取当前用户所属组织
        var user = await _userManager.GetUserInfo();

        // 获取分级管理组织
        var scopeOrg = user.dataScope.Where(x => x.Edit).Select(x => x.organizeId).Distinct().ToList();

        var ridList = await _repository.Context.Queryable<OrganizeRelationEntity>().Where(x => x.ObjectType == "Role")
            .WhereIF(!user.isAdministrator, x => scopeOrg.Contains(x.OrganizeId))
            .Select(x => new { x.ObjectId, x.OrganizeId }).ToListAsync();

        // 获取 全局角色 和 组织角色
        List<RoleListOutput>? roleList = await _repository.Entities.Where(a => a.DeleteMark == null)
            .WhereIF(!user.isAdministrator, a => a.GlobalMark == 0 && ridList.Select(x => x.ObjectId).Contains(a.Id))
            .WhereIF(user.isAdministrator, a => a.GlobalMark == 1 || ridList.Select(x => x.ObjectId).Contains(a.Id))
            .Select(a => new RoleListOutput
            {
                id = a.Id,
                parentId = a.GlobalMark.ToString(),
                fullName = a.FullName,
                enabledMark = a.EnabledMark,
                creatorTime = a.CreatorTime,
                type = "role",
                icon = "icon-qt icon-qt-global-role",
                sortCode = 0
            }).MergeTable().OrderBy(a => a.sortCode).OrderBy(a => a.creatorTime, OrderByType.Desc).ToListAsync();

        for (int i = 0; i < roleList.Count; i++) roleList[i].onlyId = "role_" + i;

        // 处理 组织角色
        roleList.Where(x => ridList.Select(x => x.ObjectId).Contains(x.id)).ToList().ForEach(item =>
        {
            var oolist = ridList.Where(x => x.ObjectId == item.id).ToList();

            for (int i = 0; i < oolist.Count; i++)
            {
                if (i == 0)
                {
                    item.parentId = oolist.FirstOrDefault().OrganizeId;
                }
                else
                {
                    // 该角色属于多个组织
                    RoleListOutput? newItem = item.ToObject<RoleListOutput>();
                    newItem.parentId = oolist[i].OrganizeId;
                    roleList.Add(newItem);
                }
            }
        });

        // 设置 全局  根目录
        List<RoleListOutput>? treeList = new List<RoleListOutput>();
        if (user.isAdministrator) treeList.Add(new RoleListOutput() { id = "1", type = string.Empty, parentId = "-1", enCode = string.Empty, fullName = "全局", num = roleList.Count(x => x.parentId == "1") });

        // 获取所有组织
        List<OrganizeEntity>? allOrgList = await _repository.Context.Queryable<OrganizeEntity>().Where(x => x.DeleteMark == null).ToListAsync();

        if (!user.isAdministrator)
        {
            var orgIdList = new List<string>();
            allOrgList.Where(x => scopeOrg.Contains(x.Id)).ToList().ForEach(item =>
            {
                orgIdList.AddRange(item.OrganizeIdTree.Split(","));
            });
            allOrgList = allOrgList.Where(x => orgIdList.Contains(x.Id)).ToList();
        }

        List<RoleListOutput>? organizeList = allOrgList.Select(x => new RoleListOutput()
        {
            id = x.Id,
            type = string.Empty,
            parentId = x.ParentId,
            organizeInfo = x.OrganizeIdTree,
            sortCode = 11,
            icon = x.Category == "company" ? "icon-qt icon-qt-tree-organization3" : "icon-qt icon-qt-tree-department1",
            fullName = x.FullName
        }).ToList();
        treeList.AddRange(organizeList);

        for (int i = 0; i < treeList.Count; i++)
        {
            treeList[i].onlyId = "organizeList_" + i;
            treeList[i].num = roleList.Count(x => x.parentId == treeList[i].id);
        }

        treeList.Where(x => x.num > 0).ToList().ForEach(item =>
        {
            if (item.organizeInfo.IsNotEmptyOrNull())
            {
                treeList.Where(x => !x.id.Equals("1") && x.organizeInfo.IsNotEmptyOrNull()).Where(x => item.organizeInfo.Contains(x.id)).ToList().ForEach(it =>
                {
                    if (it != null && it.num < 1)
                        it.num = item.num;
                });
            }
        });

        var res = treeList.Where(x => x.num > 0).Union(roleList).OrderBy(x => x.sortCode).ToList().ToTree("-1");
        return new { list = OrderbyTree(res) };
    }

    /// <summary>
    /// 获取信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        RoleEntity? entity = await _repository.FirstOrDefaultAsync(r => r.Id == id);
        RoleInfoOutput? output = entity.Adapt<RoleInfoOutput>();
        output.organizeIdsTree = new List<List<string>>();

        List<OrganizeRelationEntity>? oIds = await _repository.Context.Queryable<OrganizeRelationEntity>().Where(x => x.ObjectId == id).ToListAsync();
        List<OrganizeEntity>? oList = await _repository.Context.Queryable<OrganizeEntity>().Where(x => oIds.Select(o => o.OrganizeId).Contains(x.Id)).ToListAsync();

        oList.ForEach(item =>
        {
            if (item.OrganizeIdTree.IsNullOrEmpty()) item.OrganizeIdTree = item.Id;
            List<string>? idList = item.OrganizeIdTree?.Split(",").ToList();
            output.organizeIdsTree.Add(idList);
        });

        return output;
    }

    #endregion

    #region POST

    /// <summary>
    /// 获取角色列表 根据组织Id集合.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("getListByOrgIds")]
    public async Task<dynamic> GetListByOrgIds([FromBody] RoleListInput input)
    {
        // 获取所有组织 对应 的 角色id集合
        var ridList = await _repository.Context.Queryable<OrganizeRelationEntity>()
            .Where(x => x.ObjectType == "Role" && input.organizeIds.Contains(x.OrganizeId))
            .Select(x => new { x.ObjectId, x.OrganizeId }).ToListAsync();

        // 获取 全局角色 和 组织角色
        List<RoleListOutput>? roleList = await _repository.Context.Queryable<RoleEntity>()
            .Where(a => a.DeleteMark == null && a.EnabledMark == 1).Where(a => a.GlobalMark == 1 || ridList.Select(x => x.ObjectId).Contains(a.Id))
            .Select(a => new RoleListOutput
            {
                id = a.Id,
                parentId = a.GlobalMark.ToString(),
                fullName = a.FullName,
                enabledMark = a.EnabledMark,
                creatorTime = a.CreatorTime,
                sortCode = a.SortCode
            }).MergeTable().OrderBy(a => a.sortCode).OrderBy(a => a.creatorTime, OrderByType.Desc).ToListAsync();

        for (int i = 0; i < roleList.Count; i++) roleList[i].onlyId = "role_" + i;

        // 处理 组织角色
        roleList.Where(x => ridList.Select(x => x.ObjectId).Contains(x.id)).ToList().ForEach(item =>
        {
            var oolist = ridList.Where(x => x.ObjectId == item.id).ToList();

            for (int i = 0; i < oolist.Count; i++)
            {
                if (i == 0)
                {
                    item.parentId = oolist.FirstOrDefault().OrganizeId;
                }
                else
                {
                    // 该角色属于多个组织
                    RoleListOutput? newItem = item.ToObject<RoleListOutput>();
                    newItem.parentId = oolist[i].OrganizeId;
                    roleList.Add(newItem);
                }
            }
        });

        List<RoleListOutput>? treeList = new List<RoleListOutput>() { new RoleListOutput() { id = "1", type = string.Empty, parentId = "0", enCode = string.Empty, fullName = "全局", num = roleList.Count(x => x.parentId == "1") } };

        // 获取所有组织
        List<OrganizeEntity>? allOrgList = await _repository.Context.Queryable<OrganizeEntity>().OrderBy(x => x.CreatorTime, OrderByType.Asc).ToListAsync();
        allOrgList.ForEach(item =>
        {
            if (item.OrganizeIdTree == null || item.OrganizeIdTree == string.Empty) item.OrganizeIdTree = item.Id;
        });

        List<RoleListOutput>? organizeList = allOrgList.Where(x => input.organizeIds.Contains(x.Id)).Select(x => new RoleListOutput()
        {
            id = x.Id,
            type = string.Empty,
            parentId = "0",
            enCode = string.Empty,
            fullName = string.Join("/", allOrgList.Where(all => x.OrganizeIdTree.Split(",").Contains(all.Id)).Select(x => x.FullName)),
            num = roleList.Count(x => x.parentId == x.id)
        }).ToList();
        treeList.AddRange(organizeList);

        for (int i = 0; i < treeList.Count; i++) treeList[i].onlyId = "organizeList_" + i;

        return new { list = treeList.Union(roleList).OrderBy(x => x.sortCode).ToList().ToTree("0") };
    }

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    public async Task Create([FromBody] RoleCrInput input)
    {
        // 全局角色 只能超管才能变更
        if (input.globalMark == 1 && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1612);

        #region 分级权限验证

        List<string?>? orgIdList = input.organizeIdsTree.Select(x => x.LastOrDefault()).ToList();
        if (!_userManager.DataScope.Any(it => orgIdList.Contains(it.organizeId) && it.Add) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);
        #endregion

        if (await _repository.AnyAsync(r => r.EnCode == input.enCode && r.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D1600);
        if (await _repository.AnyAsync(r => r.FullName == input.fullName && r.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D1601);

        RoleEntity? entity = input.Adapt<RoleEntity>();

        try
        {
            // 开启事务
            _db.BeginTran();

            // 删除除了门户外的相关权限
            await _repository.Context.Insertable(entity).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();

            #region 组织角色关系

            if (input.globalMark == 0)
            {
                List<OrganizeRelationEntity>? oreList = new List<OrganizeRelationEntity>();
                input.organizeIdsTree.ForEach(item =>
                {
                    string? id = item.LastOrDefault();
                    if (id.IsNotEmptyOrNull())
                    {
                        OrganizeRelationEntity? oreEntity = new OrganizeRelationEntity();
                        oreEntity.ObjectType = "Role";
                        oreEntity.CreatorUserId = _userManager.UserId;
                        oreEntity.ObjectId = entity.Id;
                        oreEntity.OrganizeId = id;
                        oreList.Add(oreEntity);
                    }
                });

                await _repository.Context.Insertable(oreList).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync(); // 插入关系数据

            }

            #endregion

            await DelRole(string.Format("{0}_{1}", _userManager.TenantId, _userManager.UserId));

            _db.CommitTran();
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw Oops.Oh(ErrorCode.D1602);
        }
    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        RoleEntity? entity = await _repository.FirstOrDefaultAsync(r => r.Id == id && r.DeleteMark == null);
        _ = entity ?? throw Oops.Oh(ErrorCode.D1608);

        // 全局角色 只能超管才能变更
        if (entity.GlobalMark == 1 && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1612);

        #region 分级权限验证

        // 旧数据
        List<string>? orgIdList = await _repository.Context.Queryable<OrganizeRelationEntity>().Where(x => x.ObjectId == id && x.ObjectType == "Role").Select(x => x.OrganizeId).ToListAsync();
        if (!_userManager.DataScope.Any(it => orgIdList.Contains(it.organizeId) && it.Delete) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        #endregion

        // 角色下有数据权限不能删
        List<string>? items = await _authorizeService.GetAuthorizeItemIds(entity.Id, "resource");
        if (items.Count > 0)
            throw Oops.Oh(ErrorCode.D1603);

        // 角色下有表单不能删
        items = await _authorizeService.GetAuthorizeItemIds(entity.Id, "form");
        if (items.Count > 0)
            throw Oops.Oh(ErrorCode.D1606);

        // 角色下有列不能删除
        items = await _authorizeService.GetAuthorizeItemIds(entity.Id, "column");
        if (items.Count > 0)
            throw Oops.Oh(ErrorCode.D1605);

        // 角色下有按钮不能删除
        items = await _authorizeService.GetAuthorizeItemIds(entity.Id, "button");
        if (items.Count > 0)
            throw Oops.Oh(ErrorCode.D1604);

        // 角色下有菜单不能删
        items = await _authorizeService.GetAuthorizeItemIds(entity.Id, "module");
        if (items.Count > 0)
            throw Oops.Oh(ErrorCode.D1606);

        // 角色下有用户不能删
        if (await _repository.Context.Queryable<UserRelationEntity>().AnyAsync(u => u.ObjectType == "Role" && u.ObjectId == id))
            throw Oops.Oh(ErrorCode.D1607);

        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.Context.Updateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();

            // 删除角色和组织关联数据
            await _repository.Context.Deleteable<OrganizeRelationEntity>().Where(x => x.ObjectType == "Role" && x.ObjectId == id).ExecuteCommandAsync();

            await DelRole(string.Format("{0}_{1}", _userManager.TenantId, _userManager.UserId));

            _db.CommitTran();
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw Oops.Oh(ErrorCode.D1602);
        }
    }

    /// <summary>
    /// 根据选择的方式删除.
    /// </summary>
    /// <param name="way">0：检查引用状态，1：删除子表数据，2：保留子表数据</param>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpDelete("{way}/{id}")]
    public async Task DeleteWay(string id,int way)
    {
        RoleEntity? entity = await _repository.FirstOrDefaultAsync(r => r.Id == id && r.DeleteMark == null);
        _ = entity ?? throw Oops.Oh(ErrorCode.D1608);

        // 全局角色 只能超管才能变更
        if (entity.GlobalMark == 1 && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1612);

        #region 分级权限验证

        // 旧数据
        List<string>? orgIdList = await _repository.Context.Queryable<OrganizeRelationEntity>().Where(x => x.ObjectId == id && x.ObjectType == "Role").Select(x => x.OrganizeId).ToListAsync();
        if (!_userManager.DataScope.Any(it => orgIdList.Contains(it.organizeId) && it.Delete) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        #endregion

        if (way == 0)
        {
            await Delete(id);
            return;
        }

        //// 角色下有用户不能删
        //if (await _repository.Context.Queryable<UserRelationEntity>().AnyAsync(u => u.ObjectType == "Role" && u.ObjectId == id))
        //    throw Oops.Oh(ErrorCode.D1607);
        var userRelationEntity = await _repository.Context.Queryable<UserRelationEntity>().Where(u => u.ObjectType == "Role" && u.ObjectId == id).ToListAsync();

        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.Context.Updateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();

            if (userRelationEntity.IsAny())
            {
                await _repository.Context.Deleteable<UserRelationEntity>(userRelationEntity).ExecuteCommandAsync();

                if (way == 1)
                {
                    // 删除用户
                    var userList = userRelationEntity.Select(it => it.UserId).ToArray();

                    await _repository.Context.Updateable<UserEntity>().SetColumns(it => new UserEntity()
                    {
                        DeleteMark = 1,
                        DeleteTime = SqlFunc.GetDate(),
                        DeleteUserId = _userManager.UserId,
                    }).Where(it => userList.Contains(it.Id) && it.DeleteMark == null && it.IsAdministrator == 0).ExecuteCommandAsync();
                }
            }

            string[] itemList = new string[] { "resource", "form", "column", "button", "module" };
            await _repository.Context.Deleteable<AuthorizeEntity>().Where(a => a.ObjectId == entity.Id && itemList.Contains(a.ItemType )).ExecuteCommandAsync();

            // 删除角色和组织关联数据
            await _repository.Context.Deleteable<OrganizeRelationEntity>().Where(x => x.ObjectType == "Role" && x.ObjectId == id).ExecuteCommandAsync();

            await DelRole(string.Format("{0}_{1}", _userManager.TenantId, _userManager.UserId));

            _db.CommitTran();
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw Oops.Oh(ErrorCode.D1602);
        }
    }

    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="id">主键</param>
    /// <param name="input">参数</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] RoleUpInput input)
    {
        RoleEntity? oldRole = await _repository.AsQueryable().InSingleAsync(input.id);

        // 全局角色 只能超管才能变更
        if (oldRole.GlobalMark == 1 && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1612);

        #region 分级权限验证

        // 旧数据
        List<string>? orgIdList = await _repository.Context.Queryable<OrganizeRelationEntity>().Where(x => x.ObjectId == id && x.ObjectType == "Role").Select(x => x.OrganizeId).ToListAsync();
        if (!_userManager.DataScope.Any(it => orgIdList.Contains(it.organizeId) && it.Edit) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        // 新数据
        orgIdList = input.organizeIdsTree.Select(x => x.LastOrDefault()).ToList();
        if (!_userManager.DataScope.Any(it => orgIdList.Contains(it.organizeId) && it.Edit) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        #endregion

        if (await _repository.AnyAsync(r => r.EnCode == input.enCode && r.DeleteMark == null && r.Id != id))
            throw Oops.Oh(ErrorCode.D1600);
        if (await _repository.AnyAsync(r => r.FullName == input.fullName && r.DeleteMark == null && r.Id != id))
            throw Oops.Oh(ErrorCode.D1601);

        #region 如果变更组织，该角色下已存在成员，则不允许修改

        if (oldRole.GlobalMark == 0)
        {
            // 查找该角色下的所有所属组织id
            List<string>? orgRoleList = await _repository.Context.Queryable<OrganizeRelationEntity>().Where(x => x.ObjectType == "Role" && x.ObjectId == id).Select(x => x.OrganizeId).ToListAsync();

            // 查找该角色下的所有成员id
            List<string>? roleUserList = await _repository.Context.Queryable<UserRelationEntity>().Where(x => x.ObjectType == "Role" && x.ObjectId == id).Select(x => x.UserId).ToListAsync();

            // 获取带有角色成员的组织集合
            var orgUserCountList = await _repository.Context.Queryable<UserRelationEntity>().Where(x => x.ObjectType == "Organize" && roleUserList.Contains(x.UserId))
                .GroupBy(it => new { it.ObjectId })
                .Having(x => SqlFunc.AggregateCount(x.UserId) > 0)
                .Select(x => new { x.ObjectId, UCount = SqlFunc.AggregateCount(x.UserId) })
                .ToListAsync();
            List<string>? oldList = orgRoleList.Intersect(orgUserCountList.Select(x => x.ObjectId)).ToList(); // 将两个组织List交集
            List<string?>? newList = input.organizeIdsTree.Select(x => x.LastOrDefault()).ToList();

            if (oldList.Except(newList).Any()) throw Oops.Oh(ErrorCode.D1613);
        }

        // 全局改成组织
        if (oldRole.GlobalMark == 1 && input.globalMark == 0 && _repository.Context.Queryable<UserRelationEntity>().Where(x => x.ObjectType == "Role" && x.ObjectId == id).Any())
        {
            throw Oops.Oh(ErrorCode.D1615);
        }

        #endregion

        RoleEntity? entity = input.Adapt<RoleEntity>();
        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();

            #region 组织角色关系

            await _repository.Context.Deleteable<OrganizeRelationEntity>().Where(x => x.ObjectType == "Role" && x.ObjectId == entity.Id).ExecuteCommandAsync(); // 删除原数据
            if (input.globalMark == 0)
            {
                List<OrganizeRelationEntity>? oreList = new List<OrganizeRelationEntity>();
                input.organizeIdsTree.ForEach(item =>
                {
                    string? id = item.LastOrDefault();
                    if (id.IsNotEmptyOrNull())
                    {
                        OrganizeRelationEntity? oreEntity = new OrganizeRelationEntity();
                        oreEntity.ObjectType = "Role";
                        oreEntity.CreatorUserId = _userManager.UserId;
                        oreEntity.ObjectId = entity.Id;
                        oreEntity.OrganizeId = id;
                        oreList.Add(oreEntity);
                    }
                });

                await _repository.Context.Insertable(oreList).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync(); // 插入关系数据
            }

            #endregion

            await DelRole(string.Format("{0}_{1}", _userManager.TenantId, _userManager.UserId));

            _db.CommitTran();
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw Oops.Oh(ErrorCode.D1610);
        }
    }

    /// <summary>
    /// 更新状态.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/State")]
    public async Task UpdateState(string id)
    {
        if (!await _repository.AnyAsync(r => r.Id == id && r.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D1608);

        // 只能超管才能变更
        if (!_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1612);

        int isOk = await _repository.Context.Updateable<RoleEntity>().SetColumns(it => new RoleEntity()
        {
            EnabledMark = SqlFunc.IIF(it.EnabledMark == 1, 0, 1),
            LastModifyUserId = _userManager.UserId,
            LastModifyTime = SqlFunc.GetDate()
        }).Where(it => it.Id == id).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D1610);

        await DelRole(string.Format("{0}_{1}", _userManager.TenantId, _userManager.UserId));
    }

    #endregion

    #region PrivateMethod

    /// <summary>
    /// 删除角色缓存.
    /// </summary>
    /// <param name="userId">适配多租户模式(userId:tenantId_userId).</param>
    /// <returns></returns>
    private async Task<bool> DelRole(string userId)
    {
        string? cacheKey = string.Format("{0}{1}", CommonConst.CACHEKEYROLE, userId);
        await _cacheManager.DelAsync(cacheKey);
        return await Task.FromResult(true);
    }

    /// <summary>
    /// 递归排序 树形 List.
    /// </summary>
    /// <param name="list">.</param>
    /// <returns></returns>
    private List<RoleListOutput> OrderbyTree(List<RoleListOutput> list)
    {
        list.ForEach(item =>
        {
            var cList = item.children.ToObject<List<RoleListOutput>>();
            if (cList != null)
            {
                cList = cList.OrderBy(x => x.sortCode).ToList();
                item.children = cList.ToObject<List<object>>();
                if (cList.Any()) OrderbyTree(cList);
            }
        });

        return list;
    }
    #endregion
}