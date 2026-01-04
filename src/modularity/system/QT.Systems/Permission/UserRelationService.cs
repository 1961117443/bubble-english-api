using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Models.User;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Dto.UserRelation;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.Permission;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.Systems;

/// <summary>
/// 业务实现：用户关系.
/// </summary>
[ApiDescriptionSettings(Tag = "Permission", Name = "UserRelation", Order = 169)]
[Route("api/permission/[controller]")]
public class UserRelationService : IUserRelationService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<UserRelationEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 初始化一个<see cref="UserRelationService"/>类型的新实例.
    /// </summary>
    public UserRelationService(
        ISqlSugarRepository<UserRelationEntity> userRelationRepository,
        IUserManager userManager,
        ISqlSugarClient context)
    {
        _repository = userRelationRepository;
        _userManager = userManager;
        _db = context.AsTenant();
    }

    #region Get

    /// <summary>
    /// 获取岗位/角色成员列表.
    /// </summary>
    /// <param name="objectId">岗位id或角色id.</param>
    /// <returns></returns>
    [HttpGet("{objectId}")]
    public async Task<dynamic> GetListByObjectId(string objectId)
    {
        return new { ids = await _repository.AsQueryable().Where(u => u.ObjectId == objectId).Select(s => s.UserId).ToListAsync() };
    }

    #endregion

    #region Post

    /// <summary>
    /// 新建用户关系.
    /// </summary>
    /// <param name="objectId">功能主键.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("{objectId}")]
    public async Task Create(string objectId, [FromBody] UserRelationCrInput input)
    {
        #region 分级权限验证

        if (input.objectType.Equals("Role") && !_userManager.IsAdministrator)
        {
            RoleEntity? oldRole = await _repository.Context.Queryable<RoleEntity>().FirstAsync(x => x.Id.Equals(objectId));
            if (oldRole.GlobalMark == 1) throw Oops.Oh(ErrorCode.D1612); // 全局角色 只能超管才能变更
        }

        if (input.objectType.Equals("Position") || input.objectType.Equals("Role"))
        {
            var orgIds = new List<string>();
            if (input.objectType.Equals("Position")) orgIds = await _repository.Context.Queryable<PositionEntity>().Where(x => x.Id.Equals(objectId)).Select(x => x.OrganizeId).ToListAsync();
            if (input.objectType.Equals("Role")) orgIds = await _repository.Context.Queryable<OrganizeRelationEntity>().Where(x => x.ObjectId.Equals(objectId) && x.ObjectType == input.objectType).Select(x => x.OrganizeId).ToListAsync();

            if (!_userManager.DataScope.Any(it => orgIds.Contains(it.organizeId) && it.Edit) && !_userManager.IsAdministrator)
                throw Oops.Oh(ErrorCode.D1013); // 分级管控
        }

        #endregion

        List<string>? oldUserIds = await _repository.AsQueryable().Where(u => u.ObjectId.Equals(objectId) && u.ObjectType.Equals(input.objectType)).Select(s => s.UserId).ToListAsync();
        try
        {
            // 开启事务
            _db.BeginTran();

            // 禁用用户不删除
            List<string>? disabledUserIds = await _repository.Context.Queryable<UserEntity>().Where(u => u.EnabledMark == 0 && oldUserIds.Contains(u.Id)).Select(s => s.Id).ToListAsync();

            // 清空原有数据
            await _repository.DeleteAsync(u => u.ObjectId.Equals(objectId) && u.ObjectType.Equals(input.objectType) && !disabledUserIds.Contains(u.UserId));

            // 创建新数据
            List<UserRelationEntity>? dataList = new List<UserRelationEntity>();
            input.userIds.ForEach(item =>
            {
                dataList.Add(new UserRelationEntity()
                {
                    Id = SnowflakeIdHelper.NextId(),
                    CreatorTime = DateTime.Now,
                    CreatorUserId = _userManager.UserId,
                    UserId = item,
                    ObjectType = input.objectType,
                    ObjectId = objectId,
                    SortCode = input.userIds.IndexOf(item)
                });
            });
            if (dataList.Count > 0)
                await _repository.Context.Insertable(dataList).ExecuteCommandAsync();

            // 修改用户
            // 计算旧用户数组与新用户数组差
            List<string>? addList = input.userIds.Except(oldUserIds).ToList();
            List<string>? delList = oldUserIds.Except(input.userIds).ToList();
            delList = delList.Except(disabledUserIds).ToList();

            // 处理新增用户岗位
            if (addList.Count > 0)
            {
                List<UserEntity>? addUserList = await _repository.Context.Queryable<UserEntity>().Where(u => addList.Contains(u.Id)).ToListAsync();
                addUserList.ForEach(async item =>
                {
                    if (input.objectType.Equals("Position"))
                    {
                        List<string>? idList = string.IsNullOrEmpty(item.PositionId) ? new List<string>() : item.PositionId.Split(',').ToList();
                        idList.Add(objectId);

                        #region 获取默认组织下的岗位

                        if (item.PositionId.IsNullOrEmpty())
                        {
                            List<string>? pIdList = await _repository.Context.Queryable<PositionEntity>()
                            .Where(x => x.OrganizeId == item.OrganizeId && idList.Contains(x.Id)).Select(x => x.Id).ToListAsync();
                            item.PositionId = pIdList.FirstOrDefault(); // 多岗位 默认取第一个
                            item.LastModifyTime = DateTime.Now;
                            item.LastModifyUserId = _userManager.UserId;
                        }
                        #endregion
                    }
                    else if (input.objectType.Equals("Role"))
                    {
                        List<string>? idList = string.IsNullOrEmpty(item.RoleId) ? new List<string>() : item.RoleId.Split(',').ToList();
                        idList.Add(objectId);
                        item.RoleId = string.Join(",", idList.ToArray()).TrimStart(',').TrimEnd(',');
                        item.LastModifyTime = DateTime.Now;
                        item.LastModifyUserId = _userManager.UserId;
                    }
                    else if (input.objectType.Equals("Group"))
                    {
                        List<string>? idList = string.IsNullOrEmpty(item.GroupId) ? new List<string>() : item.GroupId.Split(',').ToList();
                        idList.Add(objectId);
                        item.GroupId = string.Join(",", idList.ToArray()).TrimStart(',').TrimEnd(',');
                    }
                });
                await _repository.Context.Updateable(addUserList).UpdateColumns(it => new { it.RoleId, it.PositionId, it.GroupId }).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
            }

            // 移除用户
            if (delList.Count > 0)
            {
                List<UserEntity>? delUserList = await _repository.Context.Queryable<UserEntity>().Where(u => delList.Contains(u.Id)).ToListAsync();
                foreach (UserEntity? item in delUserList)
                {
                    if (input.objectType.Equals("Position"))
                    {
                        if (item.PositionId.IsNotEmptyOrNull())
                        {
                            List<string>? idList = item.PositionId.Split(',').ToList();
                            idList.RemoveAll(x => x == objectId);
                        }

                        #region 获取默认组织下的岗位

                        List<string>? pList = _repository.Context.Queryable<PositionEntity>().Where(x => x.OrganizeId == item.OrganizeId).Select(x => x.Id).ToList();
                        List<string>? pIdList = _repository.AsQueryable().Where(x => x.UserId == item.Id && x.ObjectType == "Position" && pList.Contains(x.ObjectId)).Select(x => x.ObjectId).ToList();

                        item.PositionId = pIdList.FirstOrDefault(); // 多岗位 默认取第一个

                        #endregion

                        _repository.Context.Updateable<UserEntity>().SetColumns(it => new UserEntity()
                        {
                            PositionId = item.PositionId,
                            LastModifyTime = SqlFunc.GetDate(),
                            LastModifyUserId = _userManager.UserId
                        }).Where(it => it.Id == item.Id).ExecuteCommand();
                    }
                    else if (input.objectType.Equals("Role"))
                    {
                        if (item.RoleId.IsNotEmptyOrNull())
                        {
                            List<string>? idList = item.RoleId.Split(',').ToList();
                            idList.RemoveAll(x => x == objectId);

                            #region 多组织 优先选择有权限组织

                            string? defaultOrgId = item.OrganizeId;
                            item.OrganizeId = string.Empty;

                            List<string>? roleList = await _userManager.GetUserOrgRoleIds(string.Join(",", idList), item.OrganizeId);

                            // 如果该组织下有角色并且有角色权限 则为默认组织
                            if (roleList.Any() && _repository.Context.Queryable<AuthorizeEntity>().Where(x => x.ObjectType == "Role" && x.ItemType == "module" && roleList.Contains(x.ObjectId)).Any())
                                item.OrganizeId = defaultOrgId; // 多组织 默认

                            if (item.OrganizeId.IsNullOrEmpty())
                            {
                                List<string>? orgList = await _repository.AsQueryable().Where(x => x.ObjectType == "Organize" && x.UserId == item.Id).Select(x => x.ObjectId).ToListAsync(); // 多组织
                                foreach (string? it in orgList)
                                {
                                    roleList = await _userManager.GetUserOrgRoleIds(string.Join(",", idList), it);

                                    // 如果该组织下有角色并且有角色权限 则为默认组织
                                    if (roleList.Any() && _repository.Context.Queryable<AuthorizeEntity>().Where(x => x.ObjectType == "Role" && x.ItemType == "module" && roleList.Contains(x.ObjectId)).Any())
                                    {
                                        item.OrganizeId = it; // 多组织 默认
                                        break;
                                    }
                                }
                            }

                            if (item.OrganizeId.IsNullOrEmpty()) item.OrganizeId = defaultOrgId; // 如果所选组织下都没有角色或者没有角色权限 多组织 默认

                            // 获取默认组织下的岗位
                            List<string>? pIdList = await _repository.Context.Queryable<PositionEntity>().Where(x => x.OrganizeId == item.OrganizeId).Select(x => x.Id).ToListAsync();

                            if (!pIdList.Contains(item.PositionId)) item.PositionId = pIdList.FirstOrDefault(); // 多 岗位 默认取第一个

                            #endregion

                            _repository.Context.Updateable<UserEntity>().SetColumns(it => new UserEntity()
                            {
                                RoleId = string.Join(",", idList.ToArray()).TrimStart(',').TrimEnd(','),
                                LastModifyTime = SqlFunc.GetDate(),
                                OrganizeId = item.OrganizeId,
                                PositionId = item.PositionId,
                                LastModifyUserId = _userManager.UserId
                            }).Where(it => it.Id == item.Id).ExecuteCommand();
                        }
                    }
                    else if (input.objectType.Equals("Group"))
                    {
                        if (item.GroupId.IsNotEmptyOrNull())
                        {
                            List<string>? idList = item.GroupId.Split(',').ToList();
                            idList.RemoveAll(x => x == objectId);
                            _repository.Context.Updateable<UserEntity>().SetColumns(it => new UserEntity()
                            {
                                GroupId = string.Join(",", idList.ToArray()).TrimStart(',').TrimEnd(','),
                                LastModifyTime = SqlFunc.GetDate(),
                                LastModifyUserId = _userManager.UserId
                            }).Where(it => it.Id == item.Id).ExecuteCommand();
                        }
                    }
                }
            }

            _db.CommitTran();
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw;
        }
    }

    #endregion

    #region PublicMethod

    /// <summary>
    /// 创建用户关系.
    /// </summary>
    /// <param name="userId">用户ID.</param>
    /// <param name="ids">对象ID组.</param>
    /// <param name="relationType">关系类型(岗位-Position;角色-Role;组织-Organize;分组-Group;).</param>
    /// <returns></returns>
    public List<UserRelationEntity> CreateUserRelation(string userId, string ids, string relationType)
    {
        List<UserRelationEntity> userRelationList = new List<UserRelationEntity>();
        if (!ids.IsNullOrEmpty())
        {
            List<string>? position = new List<string>(ids.Split(','));
            position.ForEach(item =>
            {
                UserRelationEntity? entity = new UserRelationEntity();
                entity.Id = SnowflakeIdHelper.NextId();
                entity.ObjectType = relationType;
                entity.ObjectId = item;
                entity.SortCode = position.IndexOf(item);
                entity.UserId = userId;
                entity.CreatorTime = DateTime.Now;
                entity.CreatorUserId = _userManager.UserId;
                userRelationList.Add(entity);
            });
        }

        return userRelationList;
    }

    /// <summary>
    /// 创建用户关系.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [NonAction]
    public async Task Create(List<UserRelationEntity> input)
    {
        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.InsertAsync(input);

            _db.CommitTran();
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw;
        }
    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">用户ID.</param>
    /// <returns></returns>
    [NonAction]
    public async Task Delete(string id)
    {
        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.DeleteAsync(u => u.UserId == id);

            _db.CommitTran();
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw Oops.Oh(ErrorCode.D5003);
        }
    }

    /// <summary>
    /// 根据用户主键获取列表.
    /// </summary>
    /// <param name="userId">用户主键.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<UserRelationEntity>> GetListByUserId(string userId)
    {
        return await _repository.AsQueryable().Where(m => m.UserId == userId).OrderBy(o => o.CreatorTime).ToListAsync();
    }

    /// <summary>
    /// 获取用户.
    /// </summary>
    /// <param name="type">关系类型.</param>
    /// <param name="objId">对象ID.</param>
    /// <returns></returns>
    [NonAction]
    public List<string> GetUserId(string type, string objId)
    {
        return _repository.Context.Queryable<UserRelationEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.UserId == b.Id))
                .Where((a, b) => a.ObjectType == type && a.ObjectId == objId && b.DeleteMark == null && b.EnabledMark > 0).Select(a => a.UserId).Distinct().ToList();
    }

    /// <summary>
    /// 获取用户.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="objId"></param>
    /// <returns></returns>
    [NonAction]
    public List<string> GetUserId(string type, List<string> objId)
    {
        if (objId == null || objId.Count == 0)
        {
            return new List<string>();
        }
        else
        {
            return _repository.Context.Queryable<UserRelationEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.UserId == b.Id))
                .Where((a, b) => a.ObjectType == type && b.DeleteMark == null && objId.Contains(a.ObjectId) && b.EnabledMark > 0).Select(a => a.UserId).Distinct().ToList();
        }
    }

    /// <summary>
    /// 获取用户(分页).
    /// </summary>
    /// <param name="userIds">用户ID组.</param>
    /// <param name="objIds">对象ID组.</param>
    /// <param name="pageInputBase">分页参数.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<dynamic> GetUserPage(List<string> userIds, List<string> objIds, PageInputBase pageInputBase)
    {
        ISugarQueryable<UserPageListOutput>? list1 = _repository.Context.Queryable<UserRelationEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.UserId == b.Id))
           .Where((a, b) => a.ObjectType != "Organize" && b.EnabledMark > 0 && b.DeleteMark == null).WhereIF(objIds.Any(), a => objIds.Contains(a.ObjectId)).WhereIF(!objIds.Any(), a => 1 != 1)
           .Select((a, b) => new UserPageListOutput()
           {
               userId = a.UserId,
               userName = SqlFunc.MergeString(b.RealName, "/", b.Account),
           });

        ISugarQueryable<UserPageListOutput>? list2 = _repository.Context.Queryable<UserEntity>().Where(x => x.DeleteMark == null && x.EnabledMark > 0).WhereIF(userIds.Any(), x => userIds.Contains(x.Id))
            .WhereIF(!userIds.Any(), a => 1 != 1)
            .Select(a => new UserPageListOutput()
            {
                userId = a.Id,
                userName = SqlFunc.MergeString(a.RealName, "/", a.Account),
            });
        SqlSugarPagedList<UserPageListOutput>? output = await _repository.Context.UnionAll(list1, list2).Distinct().MergeTable()
            .WhereIF(pageInputBase.keyword.IsNotEmptyOrNull(), a => a.userName.Contains(pageInputBase.keyword)).ToPagedListAsync(pageInputBase.currentPage, pageInputBase.pageSize);

        return PageResult<UserPageListOutput>.SqlSugarPageResult(output);
    }

    #endregion
}