using QT.Common.Enum;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Extend.Entitys.Dto.ProjectGantt;
using QT.Extend.Entitys.Dto.Team;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.System;

namespace QT.Extend;

/// <summary>
/// 业务实现：团队管理
/// </summary>
[ApiDescriptionSettings(Tag = "Extend", Name = "Team", Order = 162)]
[Route("api/extend/[controller]")]
public class TeamService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<TeamEntity> _repository;

    /// <summary>
    /// 用户管理器.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly IDictionaryDataService _dictionaryDataService;

    /// <summary>
    /// 初始化一个<see cref="TeamService"/>类型的新实例.
    /// </summary>
    public TeamService(
        ISqlSugarRepository<TeamEntity> repository,
        IUserManager userManager,
        IDictionaryDataService dictionaryDataService)
    {
        _repository = repository;
        _userManager = userManager;
        _dictionaryDataService = dictionaryDataService;
    }

    #region GET

    /// <summary>
    /// 获取列表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] PageInputBase input, [FromQuery]string userId)
    {
        //var dictionary = await _dictionaryDataService.GetList("groupType");
        SqlSugarPagedList<TeamListOutput>? data = await _repository.Context.Queryable<TeamEntity, DictionaryDataEntity>(
            (a, b) => new JoinQueryInfos(JoinType.Left, a.Type == b.Id /*&& b.EnCode == "groupType"*/))

            // 关键字（名称、编码）
            .WhereIF(input.keyword.IsNotEmptyOrNull(), a => a.FullName.Contains(input.keyword) || a.EnCode.Contains(input.keyword))
                .WhereIF(userId.IsNotEmptyOrNull(), a => QTSqlFunc.FIND_IN_SET(userId, a.ManagerIds) || QTSqlFunc.FIND_IN_SET(userId, a.MemberIds))
            .Where(a => a.DeleteMark == null).OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).OrderBy(a => a.LastModifyTime, OrderByType.Desc)
            .Select((a, b) => new TeamListOutput
            {
                id = a.Id,
                fullName = a.FullName,
                enCode = a.EnCode,
                type = b.FullName,
                enabledMark = a.EnabledMark,
                creatorTime = a.CreatorTime,
                description = a.Description,
                sortCode = a.SortCode,
                managerIds = a.ManagerIds,
                memberIds = a.MemberIds,
            }).ToPagedListAsync(input.currentPage, input.pageSize);

        await GetManagersInfo(data.list);

        return PageResult<TeamListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 获取信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        TeamEntity? entity = await _repository.SingleAsync(p => p.Id == id);
        return entity.Adapt<TeamUpInput>();
    }

    /// <summary>
    /// 获取下拉框.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<dynamic> GetSelector()
    {
        // 获取所有分组数据
        List<TeamEntity>? TeamList = await _repository.AsQueryable()
            .Where(t => t.EnabledMark == 1 && t.DeleteMark == null)
            .OrderBy(o => o.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).OrderBy(a => a.LastModifyTime, OrderByType.Desc).ToListAsync();

        // 获取所有分组类型(字典)
        //List<DictionaryDataEntity>? typeList = await _repository.Context.Queryable<DictionaryDataEntity>()
        //    .Where(x => x.EnCode == "groupType" && x.DeleteMark == null && x.EnabledMark == 1).ToListAsync();
        var typeList = await _dictionaryDataService.GetList("groupType");

        List<TeamSelectorOutput>? treeList = new List<TeamSelectorOutput>();
        typeList.ForEach(item =>
        {
            if (TeamList.Count(x => x.Type == item.Id) > 0)
            {
                treeList.Add(new TeamSelectorOutput()
                {
                    id = item.Id,
                    parentId = "0",
                    num = TeamList.Count(x => x.Type == item.Id),
                    fullName = item.FullName
                });
            }
        });

        TeamList.ForEach(item =>
        {
            treeList.Add(
                new TeamSelectorOutput
                {
                    id = item.Id,
                    parentId = item.Type,
                    fullName = item.FullName,
                    sortCode = item.SortCode
                });
        });

        return treeList.OrderBy(x => x.sortCode).ToList().ToTree("0");
    }

    /// <summary>
    /// 根据组员获取所有分组.
    /// </summary>
    /// <returns></returns>
    [HttpGet("member/{userId}")]
    public async Task<List<TeamListOutput>> GetListByMember(string userId)
    {
        var list = await _repository.Context.Queryable<TeamEntity>()
                //(a, b) => new JoinQueryInfos(JoinType.Left, a.Type == b.Id && b.DictionaryTypeId == "271905527003350725"))

                //.Where(a => SqlFunc.Subqueryable<UserRelationEntity>().Where(x => x.ObjectId == a.Id && x.ObjectType == "Team").Any())
                .WhereIF(!_userManager.IsAdministrator,a => QTSqlFunc.FIND_IN_SET(userId, a.ManagerIds) || QTSqlFunc.FIND_IN_SET(userId, a.MemberIds))
                .Where(a => a.DeleteMark == null)
                .OrderBy(a => a.SortCode)
                .OrderBy(a => a.CreatorTime, OrderByType.Desc)
                .OrderBy(a => a.LastModifyTime, OrderByType.Desc)
                .Select((a) => new TeamListOutput
                {
                    id = a.Id,
                    fullName = a.FullName,
                    enCode = a.EnCode,
                    //type = b.FullName,
                    enabledMark = a.EnabledMark,
                    creatorTime = a.CreatorTime,
                    description = a.Description,
                    sortCode = a.SortCode
                })
                .ToListAsync();

        return list;
    }

    /// <summary>
    /// 组员下拉选项，根据负责人和参与人分组.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/{id}/member/Selector")]
    public async Task<List<TeamSelectorOutput>> GetMemberSelector(string id)
    {
        var entity = await _repository.SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);

        List<TeamSelectorOutput> teamSelectorOutputs = new List<TeamSelectorOutput>();

        if (entity.ManagerIds.IsNotEmptyOrNull())
        {
            var managerIds = entity.ManagerIds.Split(',').ToList();
            var list = await _repository.Context.Queryable<UserEntity>().Where(x => managerIds.Contains(x.Id))
                .Select(x => new TeamSelectorOutput
                {
                    id = x.Id,
                    fullName = x.RealName
                })
                .ToListAsync();

            if (list.IsAny())
            {
                teamSelectorOutputs.Add(new TeamSelectorOutput
                {
                    fullName = "负责人",
                    children = list.ToList<object>() // .Select(x => (object)x)
                });
            }
        }

        if (entity.MemberIds.IsNotEmptyOrNull())
        {
            var memberIds = entity.MemberIds.Split(',').ToList();
            var list = await _repository.Context.Queryable<UserEntity>().Where(x => memberIds.Contains(x.Id))
                .Select(x => new TeamSelectorOutput
                {
                    id = x.Id,
                    fullName = x.RealName
                })
                .ToListAsync();

            if (list.IsAny())
            {
                teamSelectorOutputs.Add(new TeamSelectorOutput
                {
                    fullName = "参与人",
                    children = list.ToList<object>() // .Select(x => (object)x)
                });
            }
        }

        return teamSelectorOutputs;
    }
    #endregion

    #region POST

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] TeamCrInput input)
    {
        if (await _repository.AnyAsync(p => p.FullName == input.fullName && p.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D2402);
        if (await _repository.AnyAsync(p => p.EnCode == input.enCode && p.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D2401);
        TeamEntity? entity = input.Adapt<TeamEntity>();
        int isOk = await _repository.Context.Insertable(entity).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D2400);
    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        //// 岗位下有用户不能删
        //if (await _repository.Context.Queryable<UserRelationEntity>().AnyAsync(u => u.ObjectType == "Team" && u.ObjectId == id))
        //    throw Oops.Oh(ErrorCode.D2406);

        TeamEntity? entity = await _repository.SingleAsync(p => p.Id == id && p.DeleteMark == null);
        int isOk = await _repository.Context.Updateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D2403);
    }

    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] TeamUpInput input)
    {
        TeamEntity oldEntity = await _repository.SingleAsync(it => it.Id == id);
        if (await _repository.AnyAsync(p => p.FullName == input.fullName && p.DeleteMark == null && p.Id != id))
            throw Oops.Oh(ErrorCode.COM1004);
        if (await _repository.AnyAsync(p => p.EnCode == input.enCode && p.DeleteMark == null && p.Id != id))
            throw Oops.Oh(ErrorCode.COM1004);

        TeamEntity entity = input.Adapt<TeamEntity>();
        int isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
        if (!(isOk > 0))
            throw Oops.Oh(ErrorCode.D2404);
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
            throw Oops.Oh(ErrorCode.D2405);

        int isOk = await _repository.Context.Updateable<TeamEntity>().UpdateColumns(it => new TeamEntity()
        {
            EnabledMark = SqlFunc.IIF(it.EnabledMark == 1, 0, 1),
            LastModifyUserId = _userManager.UserId,
            LastModifyTime = SqlFunc.GetDate()
        }).Where(it => it.Id == id).ExecuteCommandAsync();
        if (!(isOk > 0))
            throw Oops.Oh(ErrorCode.D6004);
    }

    #endregion


    #region PrivateMethod

    /// <summary>
    /// 项目参与人员.
    /// </summary>
    /// <param name="outputList"></param>
    /// <returns></returns>
    private async Task GetManagersInfo(IEnumerable<TeamListOutput> outputList)
    {
        var userIds = outputList.SelectMany(x =>
        {
            List<string> strings = new List<string>();
            if (x.managerIds.IsNotEmptyOrNull())
            {
                strings.AddRange(x.managerIds.Split(","));
            }
            if (x.memberIds.IsNotEmptyOrNull())
            {
                strings.AddRange(x.memberIds.Split(","));
            }
            return strings;
        })
            .Where(x => x.IsNotEmptyOrNull()).Distinct().ToList();
        if (userIds.IsAny())
        {
            var userList = await _repository.Context.Queryable<UserEntity>()
                .Where(x => userIds.Contains(x.Id) && x.DeleteMark == null)
                .Select(x => new UserEntity
                {
                    Id = x.Id,
                    RealName = x.RealName,
                    Account = x.Account,
                    HeadIcon = x.HeadIcon,
                })
                .ToListAsync();
            foreach (var output in outputList)
            {
                if (output.managerIds.IsNotEmptyOrNull())
                {
                    foreach (var id in output.managerIds.Split(","))
                    {
                        var managerInfo = new ManagersInfo();
                        var userInfo = userList.Find(x => x.Id == id);

                        if (userInfo != null)
                        {
                            userInfo.Adapt(managerInfo);
                            output.managersInfo.Add(managerInfo);
                        }
                    }
                }


                if (output.memberIds.IsNotEmptyOrNull())
                {
                    foreach (var id in output.memberIds.Split(","))
                    {
                        var managerInfo = new ManagersInfo();
                        var userInfo = userList.Find(x => x.Id == id);
                        if (userInfo != null)
                        {
                            userInfo.Adapt(managerInfo);
                            output.membersInfo.Add(managerInfo);
                        }
                    }
                }

            }
        }

    }
    #endregion
}