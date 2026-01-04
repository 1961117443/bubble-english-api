using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Interfaces.Permission;
using QT.Systems.Entitys.Dto.Group;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.Systems;

/// <summary>
/// 业务实现：分组管理
/// 版 本：V3.3.3


/// 日 期：2022.03.11.
/// </summary>
[ApiDescriptionSettings(Tag = "Permission", Name = "Group", Order = 162)]
[Route("api/Permission/[controller]")]
public class GroupService : IUserGroupService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<GroupEntity> _repository;

    /// <summary>
    /// 用户管理器.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="GroupService"/>类型的新实例.
    /// </summary>
    public GroupService(
        ISqlSugarRepository<GroupEntity> repository,
        IUserManager userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }

    #region GET

    /// <summary>
    /// 获取列表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] PageInputBase input)
    {
        SqlSugarPagedList<GroupListOutput>? data = await _repository.Context.Queryable<GroupEntity, DictionaryDataEntity>(
            (a, b) => new JoinQueryInfos(JoinType.Left, a.Type == b.Id && b.DictionaryTypeId == "271905527003350725"))

            // 关键字（名称、编码）
            .WhereIF(input.keyword.IsNotEmptyOrNull(), a => a.FullName.Contains(input.keyword) || a.EnCode.Contains(input.keyword))
            .Where(a => a.DeleteMark == null).OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).OrderBy(a => a.LastModifyTime, OrderByType.Desc)
            .Select((a, b) => new GroupListOutput
            {
                id = a.Id,
                fullName = a.FullName,
                enCode = a.EnCode,
                type = b.FullName,
                enabledMark = a.EnabledMark,
                creatorTime = a.CreatorTime,
                description = a.Description,
                sortCode = a.SortCode
            }).ToPagedListAsync(input.currentPage, input.pageSize);

        return PageResult<GroupListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 获取信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        GroupEntity? entity = await _repository.SingleAsync(p => p.Id == id);
        return entity.Adapt<GroupUpInput>();
    }

    /// <summary>
    /// 获取下拉框.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<dynamic> GetSelector()
    {
        // 获取所有分组数据
        List<GroupEntity>? groupList = await _repository.AsQueryable()
            .Where(t => t.EnabledMark == 1 && t.DeleteMark == null)
            .OrderBy(o => o.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).OrderBy(a => a.LastModifyTime, OrderByType.Desc).ToListAsync();

        // 获取所有分组类型(字典)
        List<DictionaryDataEntity>? typeList = await _repository.Context.Queryable<DictionaryDataEntity>()
            .Where(x => x.DictionaryTypeId == "271905527003350725" && x.DeleteMark == null && x.EnabledMark == 1).ToListAsync();

        List<GroupSelectorOutput>? treeList = new List<GroupSelectorOutput>();
        typeList.ForEach(item =>
        {
            if (groupList.Count(x => x.Type == item.Id) > 0)
            {
                treeList.Add(new GroupSelectorOutput()
                {
                    id = item.Id,
                    parentId = "0",
                    num = groupList.Count(x => x.Type == item.Id),
                    fullName = item.FullName
                });
            }
        });

        groupList.ForEach(item =>
        {
            treeList.Add(
                new GroupSelectorOutput
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
    public async Task<List<GroupListOutput>> GetListByMember(string userId)
    {
        var list = await _repository.Context.Queryable<GroupEntity>()
            //(a, b) => new JoinQueryInfos(JoinType.Left, a.Type == b.Id && b.DictionaryTypeId == "271905527003350725"))

            .Where(a => SqlFunc.Subqueryable<UserRelationEntity>().Where(x => x.ObjectId == a.Id && x.ObjectType == "Group").Any())
            .Where(a => a.DeleteMark == null)
            .OrderBy(a => a.SortCode)
            .OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .OrderBy(a => a.LastModifyTime, OrderByType.Desc)
            .Select((a) => new GroupListOutput
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
    #endregion

    #region POST

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] GroupCrInput input)
    {
        if (await _repository.AnyAsync(p => p.FullName == input.fullName && p.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D2402);
        if (await _repository.AnyAsync(p => p.EnCode == input.enCode && p.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D2401);
        GroupEntity? entity = input.Adapt<GroupEntity>();
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
        // 岗位下有用户不能删
        if (await _repository.Context.Queryable<UserRelationEntity>().AnyAsync(u => u.ObjectType == "Group" && u.ObjectId == id))
            throw Oops.Oh(ErrorCode.D2406);

        GroupEntity? entity = await _repository.SingleAsync(p => p.Id == id && p.DeleteMark == null);
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
    public async Task Update(string id, [FromBody] GroupUpInput input)
    {
        GroupEntity oldEntity = await _repository.SingleAsync(it => it.Id == id);
        if (await _repository.AnyAsync(p => p.FullName == input.fullName && p.DeleteMark == null && p.Id != id))
            throw Oops.Oh(ErrorCode.D2402);
        if (await _repository.AnyAsync(p => p.EnCode == input.enCode && p.DeleteMark == null && p.Id != id))
            throw Oops.Oh(ErrorCode.D2401);

        GroupEntity entity = input.Adapt<GroupEntity>();
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

        int isOk = await _repository.Context.Updateable<GroupEntity>().UpdateColumns(it => new GroupEntity()
        {
            EnabledMark = SqlFunc.IIF(it.EnabledMark == 1, 0, 1),
            LastModifyUserId = _userManager.UserId,
            LastModifyTime = SqlFunc.GetDate()
        }).Where(it => it.Id == id).ExecuteCommandAsync();
        if (!(isOk > 0))
            throw Oops.Oh(ErrorCode.D6004);
    }

    #endregion
}