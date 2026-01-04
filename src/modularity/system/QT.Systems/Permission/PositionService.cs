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
using QT.Systems.Entitys.Dto.Position;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.Permission;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.Systems;

/// <summary>
/// 业务实现：岗位管理
.0


/// 日 期：2021.06.07.
/// </summary>
[ApiDescriptionSettings(Tag = "Permission", Name = "Position", Order = 162)]
[Route("api/Permission/[controller]")]
public class PositionService : IPositionService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<PositionEntity> _repository;

    /// <summary>
    /// 组织管理.
    /// </summary>
    private readonly IOrganizeService _organizeService;

    /// <summary>
    /// 缓存管理器.
    /// </summary>
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 用户管理器.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 初始化一个<see cref="PositionService"/>类型的新实例.
    /// </summary>
    public PositionService(
        ISqlSugarRepository<PositionEntity> repository,
        IOrganizeService organizeService,
        ICacheManager cacheManager,
        IUserManager userManager,
        ISqlSugarClient context)
    {
        _repository = repository;
        _organizeService = organizeService;
        _cacheManager = cacheManager;
        _userManager = userManager;
        _db = context.AsTenant();
    }

    #region GET

    /// <summary>
    /// 获取列表 根据organizeId.
    /// </summary>
    /// <param name="organizeId">参数.</param>
    /// <returns></returns>
    [HttpGet("getList/{organizeId}")]
    public async Task<dynamic> GetListByOrganizeId(string organizeId)
    {
        List<string>? oid = new List<string>();
        if (!string.IsNullOrWhiteSpace(organizeId))
        {
            // 获取组织下的所有组织 id 集合
            List<OrganizeEntity>? oentity = await _repository.Context.Queryable<OrganizeEntity>().ToChildListAsync(x => x.ParentId, organizeId);
            oid = oentity.Select(x => x.Id).ToList();
        }

        List<PositionListOutput>? data = await _repository.Context.Queryable<PositionEntity, OrganizeEntity, DictionaryDataEntity>(
            (a, b, c) => new JoinQueryInfos(JoinType.Left, b.Id == a.OrganizeId, JoinType.Left, a.Type == c.EnCode && c.DictionaryTypeId == "dae93f2fd7cd4df999d32f8750fa6a1e"))

            // 组织机构
            .WhereIF(!string.IsNullOrWhiteSpace(organizeId), a => oid.Contains(a.OrganizeId))
            .Where(a => a.DeleteMark == null).OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).OrderBy(a => a.LastModifyTime, OrderByType.Desc)
            .Select((a, b, c) => new PositionListOutput
            {
                id = a.Id,
                fullName = a.FullName,
                enCode = a.EnCode,
                type = c.FullName,
                department = b.FullName,
                enabledMark = a.EnabledMark,
                creatorTime = a.CreatorTime,
                description = a.Description,
                sortCode = a.SortCode
            }).ToListAsync();
        return data.OrderBy(x => x.sortCode).ToList();
    }

    /// <summary>
    /// 获取列表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] PositionListQuery input)
    {
        if (input.organizeId == "0")
        {
            UserInfoModel? user = await _userManager.GetUserInfo();
            input.organizeId = user.organizeId;
        }

        SqlSugarPagedList<PositionListOutput>? data = await _repository.Context.Queryable<PositionEntity, OrganizeEntity, DictionaryDataEntity>(
            (a, b, c) => new JoinQueryInfos(JoinType.Left, b.Id == a.OrganizeId, JoinType.Left, a.Type == c.EnCode && c.DictionaryTypeId == "dae93f2fd7cd4df999d32f8750fa6a1e"))

            // 组织机构
            .WhereIF(!string.IsNullOrWhiteSpace(input.organizeId), a => a.OrganizeId == input.organizeId)

            // 关键字（名称、编码）
            .WhereIF(!input.keyword.IsNullOrEmpty(), a => a.FullName.Contains(input.keyword) || a.EnCode.Contains(input.keyword))
            .Where(a => a.DeleteMark == null).OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).OrderBy(a => a.LastModifyTime, OrderByType.Desc)
            .Select((a, b, c) => new PositionListOutput
            {
                id = a.Id,
                fullName = a.FullName,
                enCode = a.EnCode,
                type = c.FullName,
                department = b.FullName,
                enabledMark = a.EnabledMark,
                creatorTime = a.CreatorTime,
                description = a.Description,
                organizeId = b.OrganizeIdTree,
                sortCode = a.SortCode
            }).ToPagedListAsync(input.currentPage, input.pageSize);

        #region 处理岗位所属组织树

        List<OrganizeEntity>? orgList = await _repository.Context.Queryable<OrganizeEntity>().Where(x => x.DeleteMark == null).OrderBy(x => x.CreatorTime).ToListAsync();

        foreach (PositionListOutput? item in data.list)
        {
            if (item.organizeId.IsNotEmptyOrNull())
            {
                List<string>? tree = orgList.Where(x => item.organizeId.Split(",").Contains(x.Id)).Select(x => x.FullName).ToList();
                item.department = string.Join("/", tree);
            }
        }

        #endregion

        return PageResult<PositionListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 获取列表.
    /// </summary>
    /// <returns></returns>
    [HttpGet("All")]
    public async Task<dynamic> GetList()
    {
        List<PositionListOutput>? data = await _repository.Context.Queryable<PositionEntity, OrganizeEntity, DictionaryDataEntity>((a, b, c) => new JoinQueryInfos(JoinType.Left, b.Id == a.OrganizeId, JoinType.Left, a.Type == c.EnCode && c.DictionaryTypeId == "dae93f2fd7cd4df999d32f8750fa6a1e"))
            .Where(a => a.DeleteMark == null && a.EnabledMark == 1).OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).OrderBy(a => a.LastModifyTime, OrderByType.Desc)
            .Select((a, b, c) => new PositionListOutput
            {
                id = a.Id,
                fullName = a.FullName,
                enCode = a.EnCode,
                type = c.FullName,
                department = b.FullName,
                enabledMark = a.EnabledMark,
                creatorTime = a.CreatorTime,
                description = a.Description,
                sortCode = a.SortCode
            }).ToListAsync();
        return new { list = data.OrderBy(x => x.sortCode).ToList() };
    }

    /// <summary>
    /// 获取下拉框（公司+部门+岗位）.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<dynamic> GetSelector()
    {
        List<OrganizeEntity>? organizeList = await _organizeService.GetListAsync();
        List<PositionEntity>? positionList = await _repository.AsQueryable().Where(t => t.EnabledMark == 1 && t.DeleteMark == null)
            .OrderBy(o => o.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).OrderBy(a => a.LastModifyTime, OrderByType.Desc).ToListAsync();
        List<PositionSelectorOutput>? treeList = new List<PositionSelectorOutput>();

        organizeList.ForEach(item =>
        {
            string? icon = string.Empty;
            if (item.Category.Equals("department"))
                icon = "icon-qt icon-qt-tree-department1";
            else
                icon = "icon-qt icon-qt-tree-organization3";
            treeList.Add(
                new PositionSelectorOutput
                {
                    id = item.Id,
                    parentId = item.ParentId,
                    fullName = item.FullName,
                    enabledMark = item.EnabledMark,
                    icon = icon,
                    type = item.Category,
                    organizeInfo = item.OrganizeIdTree,
                    num = positionList.Count(x => x.OrganizeId == item.Id),
                    sortCode = 1
                });
        });

        treeList.Where(x => x.num > 0 && x.organizeInfo.IsNotEmptyOrNull()).ToList().ForEach(item =>
        {
            treeList.Where(x => x.organizeInfo.IsNotEmptyOrNull()).Where(x => item.organizeInfo.Contains(x.id)).ToList().ForEach(it =>
            {
                if (it != null && it.num < 1) it.num = item.num;
            });
        });

        positionList.ForEach(item =>
        {
            treeList.Add(
                new PositionSelectorOutput
                {
                    id = item.Id,
                    parentId = item.OrganizeId,
                    fullName = item.FullName,
                    enabledMark = item.EnabledMark,
                    icon = "icon-qt icon-qt-tree-position1",
                    type = "position",
                    num = 1,
                    sortCode = 0
                });
        });

        var res = treeList.Where(x => x.num > 0).OrderBy(x => x.sortCode).ToList().ToTree("-1");

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
        PositionEntity? entity = await _repository.SingleAsync(p => p.Id == id);
        return entity.Adapt<PositionInfoOutput>();
    }

    #endregion

    #region POST

    /// <summary>
    /// 获取岗位列表 根据组织Id集合.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("getListByOrgIds")]
    public async Task<dynamic> GetListByOrgIds([FromBody] PositionListQuery input)
    {
        List<PositionListOutput>? data = await _repository.Context.Queryable<PositionEntity, OrganizeEntity, DictionaryDataEntity>(
            (a, b, c) => new JoinQueryInfos(JoinType.Left, b.Id == a.OrganizeId, JoinType.Left, a.Type == c.EnCode && c.DictionaryTypeId == "dae93f2fd7cd4df999d32f8750fa6a1e"))
            .Where(a => input.organizeIds.Contains(a.OrganizeId) && a.DeleteMark == null && a.EnabledMark == 1).OrderBy(a => a.SortCode)
            .Select((a, b, c) => new PositionListOutput
            {
                id = a.Id,
                parentId = b.Id,
                fullName = a.FullName,
                enabledMark = a.EnabledMark,
                creatorTime = a.CreatorTime,
                sortCode = a.SortCode,
                isLeaf = true
            }).ToListAsync();

        // 获取所有组织
        List<OrganizeEntity>? allOrgList = await _repository.Context.Queryable<OrganizeEntity>().OrderBy(x => x.CreatorTime, OrderByType.Asc).ToListAsync();
        allOrgList.ForEach(item =>
        {
            item.ParentId = "0";
            if (item.OrganizeIdTree == null || item.OrganizeIdTree.Equals(string.Empty)) item.OrganizeIdTree = item.Id;
        });

        List<PositionListOutput>? organizeList = allOrgList.Where(x => input.organizeIds.Contains(x.Id)).Select(x => new PositionListOutput()
        {
            id = x.Id,
            parentId = "0",
            fullName = string.Join("/", allOrgList.Where(all => x.OrganizeIdTree.Split(",").Contains(all.Id)).Select(x => x.FullName)),
            num = data.Count(x => x.parentId == x.id)
        }).ToList();

        return new { list = organizeList.Union(data).OrderBy(x => x.sortCode).ToList().ToTree("0") };
    }

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] PositionCrInput input)
    {
        UserInfoModel? user = await _userManager.GetUserInfo();
        if (!_userManager.DataScope.Any(it => it.organizeId == input.organizeId && it.Add == true) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);
        if (await _repository.AnyAsync(p => p.OrganizeId == input.organizeId && p.FullName == input.fullName && p.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D6005);
        if (await _repository.AnyAsync(p => p.OrganizeId == input.organizeId && p.EnCode == input.enCode && p.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D6000);
        PositionEntity? entity = input.Adapt<PositionEntity>();
        int isOk = await _repository.Context.Insertable(entity).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D6001);
        await DelPosition(string.Format("{0}_{1}", _userManager.TenantId, _userManager.UserId));
    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        PositionEntity? entity = await _repository.SingleAsync(p => p.Id == id && p.DeleteMark == null);
        if (!_userManager.DataScope.Any(it => it.organizeId == entity.OrganizeId && it.Delete == true) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        // 岗位下有用户不能删
        if (await _repository.Context.Queryable<UserRelationEntity>().AnyAsync(u => u.ObjectType == "Position" && u.ObjectId == id))
            throw Oops.Oh(ErrorCode.D6007);

        int isOk = await _repository.Context.Updateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();
        if (!(isOk > 0))
            throw Oops.Oh(ErrorCode.D6002);
        await DelPosition(string.Format("{0}_{1}", _userManager.TenantId, _userManager.UserId));
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
        PositionEntity? entity = await _repository.SingleAsync(p => p.Id == id && p.DeleteMark == null);
        if (!_userManager.DataScope.Any(it => it.organizeId == entity.OrganizeId && it.Delete == true) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        if (way == 0)
        {
            await Delete(id);
            return;
        }

        try
        {
            _db.BeginTran();

            var userRelationEntity = await _repository.Context.Queryable<UserRelationEntity>().Where(u => u.ObjectType == "Position" && u.ObjectId == id).ToListAsync();

            await _repository.Context.Updateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();

            if (userRelationEntity.IsAny())
            {
                // 删除该岗位和用户关联数据
                await _repository.Context.Deleteable<UserRelationEntity>(userRelationEntity).ExecuteCommandAsync();

                if (way == 1)
                {
                    var userList = userRelationEntity.Select(it => it.UserId).ToArray();

                    await _repository.Context.Updateable<UserEntity>().SetColumns(it => new UserEntity()
                    {
                        DeleteMark = 1,
                        DeleteTime = SqlFunc.GetDate(),
                        DeleteUserId = _userManager.UserId,
                    }).Where(it => userList.Contains(it.Id) && it.DeleteMark == null && it.IsAdministrator == 0).ExecuteCommandAsync();

                }
            }

            //// 岗位下有用户不能删
            //if (await _repository.Context.Queryable<UserRelationEntity>().AnyAsync(u => u.ObjectType == "Position" && u.ObjectId == id))
            //    throw Oops.Oh(ErrorCode.D6007);

            //int isOk = await _repository.Context.Updateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();
            //if (!(isOk > 0))
            //    throw Oops.Oh(ErrorCode.D6002);


            await DelPosition(string.Format("{0}_{1}", _userManager.TenantId, _userManager.UserId));

            _db.CommitTran();
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw Oops.Oh(ErrorCode.D6002);
        }
    }

    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] PositionUpInput input)
    {
        PositionEntity? oldEntity = await _repository.SingleAsync(it => it.Id == id);
        if (oldEntity.OrganizeId != input.organizeId && !_userManager.DataScope.Any(it => it.organizeId == oldEntity.OrganizeId && it.Edit == true) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);
        if (!_userManager.DataScope.Any(it => it.organizeId == input.organizeId && it.Edit == true) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);
        if (await _repository.AnyAsync(p => p.OrganizeId == input.organizeId && p.FullName == input.fullName && p.DeleteMark == null && p.Id != id))
            throw Oops.Oh(ErrorCode.D6005);
        if (await _repository.AnyAsync(p => p.OrganizeId == input.organizeId && p.EnCode == input.enCode && p.DeleteMark == null && p.Id != id))
            throw Oops.Oh(ErrorCode.D6000);

        // 如果变更组织，该岗位下已存在成员，则不允许修改
        if (input.organizeId != oldEntity.OrganizeId)
        {
            if (await _repository.Context.Queryable<UserRelationEntity>().AnyAsync(u => u.ObjectType == "Position" && u.ObjectId == id))
                throw Oops.Oh(ErrorCode.D6008);
        }

        PositionEntity? entity = input.Adapt<PositionEntity>();
        int isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
        if (!(isOk > 0))
            throw Oops.Oh(ErrorCode.D6003);
        await DelPosition(string.Format("{0}_{1}", _userManager.TenantId, _userManager.UserId));
    }

    /// <summary>
    /// 更新状态.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/State")]
    public async Task UpdateState(string id)
    {
        UserInfoModel? user = await _userManager.GetUserInfo();
        if (!_userManager.DataScope.Any(it => it.organizeId == id && it.Add == true) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);
        if (!await _repository.AnyAsync(r => r.Id == id && r.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D6006);

        int isOk = await _repository.Context.Updateable<PositionEntity>().UpdateColumns(it => new PositionEntity()
        {
            EnabledMark = SqlFunc.IIF(it.EnabledMark == 1, 0, 1),
            LastModifyUserId = _userManager.UserId,
            LastModifyTime = SqlFunc.GetDate()
        }).Where(it => it.Id == id).ExecuteCommandAsync();
        if (!(isOk > 0))
            throw Oops.Oh(ErrorCode.D6004);
        await DelPosition(string.Format("{0}_{1}", _userManager.TenantId, _userManager.UserId));
    }

    #endregion

    #region PublicMethod

    /// <summary>
    /// 获取信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<PositionEntity> GetInfoById(string id)
    {
        return await _repository.SingleAsync(p => p.Id == id);
    }

    /// <summary>
    /// 获取岗位列表.
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<List<PositionEntity>> GetListAsync()
    {
        return await _repository.AsQueryable().Where(u => u.DeleteMark == null).ToListAsync();
    }

    /// <summary>
    /// 名称.
    /// </summary>
    /// <param name="ids">岗位ID组</param>
    /// <returns></returns>
    [NonAction]
    public string GetName(string ids)
    {
        if (ids.IsNullOrEmpty())
            return string.Empty;
        List<string>? idList = ids.Split(",").ToList();
        List<string>? nameList = new List<string>();
        List<PositionEntity>? roleList = _repository.AsQueryable().Where(x => x.DeleteMark == null && x.EnabledMark == 1).ToList();
        foreach (string item in idList)
        {
            var info = roleList.Find(x => x.Id == item);
            if (info != null && info.FullName.IsNotEmptyOrNull())
            {
                nameList.Add(info.FullName);
            }
        }

        return string.Join(",", nameList);
    }

    #endregion

    #region PrivateMethod

    /// <summary>
    /// 删除岗位缓存.
    /// </summary>
    /// <param name="userId">适配多租户模式(userId:tenantId_userId).</param>
    /// <returns></returns>
    private async Task<bool> DelPosition(string userId)
    {
        string? cacheKey = string.Format("{0}{1}", CommonConst.CACHEKEYPOSITION, userId);
        await _cacheManager.DelAsync(cacheKey);
        return await Task.FromResult(true);
    }

    /// <summary>
    /// 递归排序 树形 List.
    /// </summary>
    /// <param name="list">.</param>
    /// <returns></returns>
    private List<PositionSelectorOutput> OrderbyTree(List<PositionSelectorOutput> list)
    {
        list.ForEach(item =>
        {
            var cList = item.children.ToObject<List<PositionSelectorOutput>>();
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