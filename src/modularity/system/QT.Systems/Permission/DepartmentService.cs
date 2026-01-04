using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Dto.Department;
using QT.Systems.Entitys.Dto.Organize;
using QT.Systems.Entitys.Dto.SysConfig;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.Permission;
using QT.Systems.Interfaces.System;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Polly;
using QT.Common.Extension;

namespace QT.Systems;

/// <summary>
/// 业务实现：部门管理.
/// </summary>
[ApiDescriptionSettings(Tag = "Permission", Name = "Organize", Order = 166)]
[Route("api/permission/[controller]")]
public class DepartmentService : IDepartmentService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 部门管理仓储.
    /// </summary>
    private readonly ISqlSugarRepository<OrganizeEntity> _repository;

    /// <summary>
    /// 系统配置.
    /// </summary>
    private readonly ISysConfigService _sysConfigService;

    /// <summary>
    /// 组织管理.
    /// </summary>
    private readonly IOrganizeService _organizeService;

    /// <summary>
    /// 第三方同步.
    /// </summary>
    private readonly ISynThirdInfoService _synThirdInfoService;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 初始化一个<see cref="DepartmentService"/>类型的新实例.
    /// </summary>
    public DepartmentService(
        ISqlSugarRepository<OrganizeEntity> repository,
        ISysConfigService sysConfigService,
        IOrganizeService organizeService,
        ISynThirdInfoService synThirdInfoService,
        IUserManager userManager,
        ISqlSugarClient context)
    {
        _repository = repository;
        _sysConfigService = sysConfigService;
        _organizeService = organizeService;
        _synThirdInfoService = synThirdInfoService;
        _userManager = userManager;
        _db = context.AsTenant();
    }

    #region GET

    /// <summary>
    /// 获取信息.
    /// </summary>
    /// <param name="companyId">公司主键.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("{companyId}/Department")]
    public async Task<dynamic> GetList(string companyId, [FromQuery] KeywordInput input)
    {
        List<DepartmentListOutput>? data = new List<DepartmentListOutput>();

        // 全部部门数据
        List<DepartmentListOutput>? departmentAllList = await _repository.Context.Queryable<OrganizeEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.ManagerId))
            .Where(a => a.ParentId == companyId && a.Category.Equals("department") && a.DeleteMark == null).OrderBy(a => a.SortCode)
            .OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .Select((a, b) => new DepartmentListOutput
            {
                id = a.Id,
                parentId = a.ParentId,
                fullName = a.FullName,
                enCode = a.EnCode,
                description = a.Description,
                enabledMark = a.EnabledMark,
                creatorTime = a.CreatorTime,
                manager = SqlFunc.MergeString(b.RealName, "/", b.Account),
                sortCode = a.SortCode
            }).ToListAsync();

        // 当前公司部门
        List<OrganizeEntity>? departmentList = await _repository.Context.Queryable<OrganizeEntity>().WhereIF(!string.IsNullOrEmpty(input.keyword), d => d.FullName.Contains(input.keyword) || d.EnCode.Contains(input.keyword))
            .Where(t => t.ParentId == companyId && t.Category.Equals("department") && t.DeleteMark == null)
            .OrderBy(a => a.SortCode)
            .OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .ToListAsync();
        departmentList.ForEach(item =>
        {
            item.ParentId = "0";
            data.AddRange(departmentAllList.TreeChildNode(item.Id, t => t.id, t => t.parentId));
        });
        return new { list = data.OrderBy(x => x.sortCode).ToList() };
    }

    /// <summary>
    /// 获取下拉框.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Department/Selector/{id}")]
    public async Task<dynamic> GetSelector(string id)
    {
        List<OrganizeEntity>? data = await _repository.AsQueryable().Where(t => t.DeleteMark == null).OrderBy(o => o.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).ToListAsync();
        if (!"0".Equals(id)) data.RemoveAll(it => it.Id == id);

        List<DepartmentSelectorOutput>? treeList = data.Adapt<List<DepartmentSelectorOutput>>();
        treeList.ForEach(item =>
        {
            if (item.type != null && item.type.Equals("company")) item.icon = "icon-qt icon-qt-tree-organization3";
        });
        return new { list = treeList.OrderBy(x => x.sortCode).ToList().ToTree("-1") };
    }

    /// <summary>
    /// 获取信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpGet("Department/{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        OrganizeEntity? entity = await _repository.SingleAsync(d => d.Id == id);
        return entity.Adapt<DepartmentInfoOutput>();
    }

    #endregion

    #region POST

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("Department")]
    public async Task Create([FromBody] DepartmentCrInput input)
    {
        if (!_userManager.DataScope.Any(it => it.organizeId == input.parentId && it.Add) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);
        if (await _repository.Context.Queryable<OrganizeEntity>().AnyAsync(o => o.EnCode.Equals(input.enCode) && o.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D2014);
        if (await _repository.Context.Queryable<OrganizeEntity>().AnyAsync(o => o.ParentId == input.parentId && o.FullName == input.fullName && o.Category == "department" && o.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D2019);
        OrganizeEntity? entity = input.Adapt<OrganizeEntity>();
        entity.Category = "department";
        entity.Id = SnowflakeIdHelper.NextId();
        entity.EnabledMark = 1;
        entity.CreatorTime = DateTime.Now;
        entity.CreatorUserId = _userManager.UserId;

        #region  处理 上级ID列表 存储

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

        OrganizeEntity? newEntity = await _repository.Context.Insertable(entity).CallEntityMethod(m => m.Create()).ExecuteReturnEntityAsync();
        _ = newEntity ?? throw Oops.Oh(ErrorCode.D2015);

        #region 第三方同步

        try
        {
            SysConfigOutput? sysConfig = await _sysConfigService.GetInfo();
            List<OrganizeListOutput>? orgList = new List<OrganizeListOutput>();
            orgList.Add(entity.Adapt<OrganizeListOutput>());
            if (sysConfig.dingSynIsSynOrg)
                await _synThirdInfoService.SynDep(2, 2, sysConfig, orgList);
            if (sysConfig.qyhIsSynOrg)
                await _synThirdInfoService.SynDep(1, 2, sysConfig, orgList);
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
    [HttpDelete("Department/{id}")]
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
            .Where(x=> SqlFunc.Subqueryable<UserEntity>().Where(u=>u.Id == x.UserId && u.DeleteMark == null).Any())
            .AnyAsync())
            throw Oops.Oh(ErrorCode.D2004);

        // 该机构下有角色，则不能删
        if (await _repository.Context.Queryable<OrganizeRelationEntity>().Where(x => x.OrganizeId == id && x.ObjectType == "Role").AnyAsync())
            throw Oops.Oh(ErrorCode.D2020);
        OrganizeEntity? entity = await _repository.SingleAsync(o => o.Id == id && o.DeleteMark == null);
        _ = entity ?? throw Oops.Oh(ErrorCode.D2002);

        int isOK = await _repository.Context.Updateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();
        if (!(isOK > 0))
        {
            throw Oops.Oh(ErrorCode.D2017);
        }
        else
        {
            // 删除该组织和角色关联数据
            await _repository.Context.Deleteable<OrganizeRelationEntity>().Where(x => x.OrganizeId == id && x.ObjectType == "Role").ExecuteCommandAsync();
        }

        #region 第三方数据删除
        try
        {
            SysConfigOutput? sysConfig = await _sysConfigService.GetInfo();
            if (sysConfig.dingSynIsSynOrg) await _synThirdInfoService.DelSynData(2, 2, sysConfig, id);
            if (sysConfig.qyhIsSynOrg) await _synThirdInfoService.DelSynData(1, 2, sysConfig, id);
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
    [HttpDelete("Department/{way}/{id}")]
    public async Task DeleteWay(string id,int way)
    {
        if (!_userManager.DataScope.Any(it => it.organizeId == id && it.Delete == true) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        if (way == 0)
        {
            await Delete(id);
            return;
        }


        OrganizeEntity? entity = await _repository.SingleAsync(o => o.Id == id && o.DeleteMark == null);
        _ = entity ?? throw Oops.Oh(ErrorCode.D2002);

        //// 该机构下有机构，则不能删
        //if (await _repository.AnyAsync(o => o.ParentId.Equals(id) && o.DeleteMark == null))
        //    throw Oops.Oh(ErrorCode.D2005);

        //// 该机构下有岗位，则不能删
        //if (await _repository.Context.Queryable<PositionEntity>().AnyAsync(p => p.OrganizeId.Equals(id) && p.DeleteMark == null))
        //    throw Oops.Oh(ErrorCode.D2006);

        #region 用户和角色
        // 该机构下有用户，则不能删
        var userRelationList = await _repository.Context.Queryable<UserRelationEntity>()
            .Where(x => x.ObjectType == "Organize" && x.ObjectId == id)
            .Where(x => SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == x.UserId && u.DeleteMark == null).Any())
            .ToListAsync();

        // 该机构下有角色，则不能删
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


            await _repository.Context.Updateable(entity).CallEntityMethod(m => m.Delete())
            .UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId })
            .ExecuteCommandAsync();

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

        //int isOK = await _repository.Context.Updateable(entity).CallEntityMethod(m => m.Delete())
        //    .UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId })
        //    .ExecuteCommandAsync();
        //if (!(isOK > 0))
        //{
        //    throw Oops.Oh(ErrorCode.D2017);
        //}
        //else
        //{
        //    // 删除该组织和角色关联数据
        //    await _repository.Context.Deleteable<OrganizeRelationEntity>().Where(x => x.OrganizeId == id && x.ObjectType == "Role").ExecuteCommandAsync();
        //}

        #region 第三方数据删除
        try
        {
            SysConfigOutput? sysConfig = await _sysConfigService.GetInfo();
            if (sysConfig.dingSynIsSynOrg) await _synThirdInfoService.DelSynData(2, 2, sysConfig, id);
            if (sysConfig.qyhIsSynOrg) await _synThirdInfoService.DelSynData(1, 2, sysConfig, id);
        }
        catch (Exception)
        {
        }
        #endregion
    }

    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPut("Department/{id}")]
    public async Task Update(string id, [FromBody] DepartmentUpInput input)
    {
        OrganizeEntity? oldEntity = await _repository.SingleAsync(it => it.Id == id);
        if (oldEntity.ParentId != input.parentId && !_userManager.DataScope.Any(it => it.organizeId == oldEntity.ParentId && it.Edit) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        if (oldEntity.ParentId != input.parentId && !_userManager.DataScope.Any(it => it.organizeId == input.parentId && it.Edit) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        if (!_userManager.DataScope.Any(it => it.organizeId == id && it.Edit) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        if (input.parentId.Equals(id))
            throw Oops.Oh(ErrorCode.D2001);

        // 父id不能为自己的子节点
        List<string>? childIdListById = await _organizeService.GetChildIdListWithSelfById(id);
        if (childIdListById.Contains(input.parentId))
            throw Oops.Oh(ErrorCode.D2001);
        if (await _repository.AnyAsync(o => o.EnCode == input.enCode && o.Id != id && o.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D2014);
        if (await _repository.AnyAsync(o => o.ParentId == input.parentId && o.FullName == input.fullName && o.Id != id && o.Category == "department" && o.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D2019);
        OrganizeEntity? entity = input.Adapt<OrganizeEntity>();
        entity.LastModifyTime = DateTime.Now;
        entity.LastModifyUserId = _userManager.UserId;

        #region 处理 上级ID列表 存储
        if (string.IsNullOrWhiteSpace(oldEntity.OrganizeIdTree) || entity.ParentId != oldEntity.ParentId)
        {
            List<string>? idList = new List<string>();
            idList.Add(entity.Id);
            if (entity.ParentId != "-1")
            {
                List<string>? ids = _repository.Entities.ToParentList(it => it.ParentId, entity.ParentId).Select(x => x.Id).ToList();
                idList.AddRange(ids);
            }

            idList.Reverse();
            entity.OrganizeIdTree = string.Join(",", idList);

            // 如果上级结构 变动 ，需要更改所有包含 该组织的id 的结构
            if (entity.OrganizeIdTree != oldEntity.OrganizeIdTree)
            {
                List<OrganizeEntity>? oldEntityList = await _repository.Where(x => x.OrganizeIdTree.Contains(oldEntity.Id) && x.Id != oldEntity.Id).ToListAsync();
                oldEntityList.ForEach(item =>
                {
                    string? childList = item.OrganizeIdTree.Split(oldEntity.Id).LastOrDefault();
                    item.OrganizeIdTree = entity.OrganizeIdTree + childList;
                });

                await _repository.Context.Updateable(oldEntityList).UpdateColumns(x => x.OrganizeIdTree).ExecuteCommandAsync(); // 批量修改 父级组织
            }
        }
        #endregion

        int isOK = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
        if (!(isOK > 0)) throw Oops.Oh(ErrorCode.D2018);

        #region 第三方同步
        try
        {
            SysConfigOutput? sysConfig = await _sysConfigService.GetInfo();
            List<OrganizeListOutput>? orgList = new List<OrganizeListOutput>();
            entity.Category = "department";
            orgList.Add(entity.Adapt<OrganizeListOutput>());
            if (sysConfig.dingSynIsSynOrg) await _synThirdInfoService.SynDep(2, 2, sysConfig, orgList);

            if (sysConfig.qyhIsSynOrg) await _synThirdInfoService.SynDep(1, 2, sysConfig, orgList);

        }
        catch (Exception)
        {
        }
        #endregion
    }

    /// <summary>
    /// 更新状态.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpPut("Department/{id}/Actions/State")]
    public async Task UpdateState(string id)
    {
        if (!_userManager.DataScope.Any(it => it.organizeId == id && it.Edit == true) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);
        OrganizeEntity? entity = await _repository.FirstOrDefaultAsync(o => o.Id == id);
        _ = entity.EnabledMark == 1 ? 0 : 1;
        entity.LastModifyTime = DateTime.Now;
        entity.LastModifyUserId = _userManager.UserId;

        int isOk = await _repository.Context.Updateable(entity).UpdateColumns(o => new { o.EnabledMark, o.LastModifyTime, o.LastModifyUserId }).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D2016);
    }

    #endregion

    #region PublicMethod

    /// <summary>
    /// 获取部门列表(其他服务使用).
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<List<OrganizeEntity>> GetListAsync()
    {
        return await _repository.AsQueryable().Where(t => t.Category.Equals("department") && t.EnabledMark == 1 && t.DeleteMark == null).OrderBy(o => o.SortCode).ToListAsync();
    }

    /// <summary>
    /// 部门名称.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [NonAction]
    public string GetDepName(string id)
    {
        OrganizeEntity? entity = _repository.FirstOrDefault(x => x.Id == id && x.Category == "department" && x.EnabledMark == 1 && x.DeleteMark == null);
        return entity == null ? string.Empty : entity.FullName;
    }

    /// <summary>
    /// 公司名称.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [NonAction]
    public string GetComName(string id)
    {
        string? name = string.Empty;
        OrganizeEntity? entity = _repository.FirstOrDefault(x => x.Id == id && x.EnabledMark == 1 && x.DeleteMark == null);
        if (entity == null)
        {
            return name;
        }
        else
        {
            if (entity.Category == "company")
            {
                return entity.FullName;
            }
            else
            {
                OrganizeEntity? pEntity = _repository.FirstOrDefault(x => x.Id == entity.ParentId && x.EnabledMark == 1 && x.DeleteMark == null);
                return GetComName(pEntity.Id);
            }
        }
    }

    /// <summary>
    /// 公司结构名称树.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [NonAction]
    public string GetOrganizeNameTree(string id)
    {
        string? names = string.Empty;

        // 组织结构
        List<string>? olist = _repository.AsQueryable().ToParentList(it => it.ParentId, id).Select(x => x.FullName).ToList();
        olist.Reverse();
        names = string.Join("/", olist);

        return names;
    }

    /// <summary>
    /// 公司id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [NonAction]
    public string GetCompanyId(string id)
    {
        OrganizeEntity? entity = _repository.FirstOrDefault(x => x.Id == id && x.EnabledMark == 1 && x.DeleteMark == null);
        if (entity == null)
        {
            return string.Empty;
        }
        else
        {
            if (entity.Category == "company")
            {
                return entity.Id;
            }
            else
            {
                OrganizeEntity? pEntity = _repository.FirstOrDefault(x => x.Id == entity.ParentId && x.EnabledMark == 1 && x.DeleteMark == null);
                return GetCompanyId(pEntity.Id);
            }
        }
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