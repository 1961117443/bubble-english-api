using Microsoft.AspNetCore.Mvc;
using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Dto.Authorize;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.Permission;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Systems;

/// <summary>
/// 业务实现：这里主要是写禁用权限相关的服务
/// </summary>
[ApiDescriptionSettings(Tag = "Permission", Name = "Disable", Order = 170)]
[Route("api/permission/Authority/[controller]")]
public class AuthorizeDisableService: IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<AuthorizeDisableEntity> _authorizeRepository;
    private readonly IUserManager _userManager;
    private readonly ITenant _db;

    /// <summary>
    /// 初始化一个<see cref="AuthorizeService"/>类型的新实例.
    /// </summary>
    public AuthorizeDisableService(
        ISqlSugarRepository<AuthorizeDisableEntity> authorizeRepository,
        IUserManager userManager,
        ISqlSugarClient context)
    {
        _userManager = userManager;
        _db = context.AsTenant();
        _authorizeRepository = authorizeRepository;
    }

    /// <summary>
    /// 设置或更新岗位/角色/用户禁用的权限.
    /// </summary>
    /// <param name="objectId">参数.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("Data/{objectId}")]
    public async Task UpdateData(string objectId, [FromBody] AuthorizeDataUpInput input)
    {
        //#region 分级权限验证

        //if (input.objectType.Equals("Role") && !_userManager.IsAdministrator)
        //{
        //    RoleEntity? oldRole = await _authorizeRepository.Context.Queryable<RoleEntity>().FirstAsync(x => x.Id.Equals(objectId));
        //    if (oldRole.GlobalMark == 1) throw Oops.Oh(ErrorCode.D1612); // 全局角色 只能超管才能变更
        //}

        //if (input.objectType.Equals("Position") || input.objectType.Equals("Role"))
        //{
        //    var orgIds = new List<string>();
        //    if (input.objectType.Equals("Position")) orgIds = await _authorizeRepository.Context.Queryable<PositionEntity>().Where(x => x.Id.Equals(objectId)).Select(x => x.OrganizeId).ToListAsync();
        //    if (input.objectType.Equals("Role")) orgIds = await _authorizeRepository.Context.Queryable<OrganizeRelationEntity>().Where(x => x.ObjectId.Equals(objectId) && x.ObjectType == input.objectType).Select(x => x.OrganizeId).ToListAsync();

        //    if (!_userManager.DataScope.Any(it => orgIds.Contains(it.organizeId) && it.Edit) && !_userManager.IsAdministrator)
        //        throw Oops.Oh(ErrorCode.D1013); // 分级管控
        //}

        //#endregion

        List<string> roleIds = new List<string>();
        string userId = string.Empty;
        if (input.objectType == "User")
        {
            var userRoleIds = await _authorizeRepository.Context.Queryable<UserEntity>().Where(it => it.Id == objectId).Select(it => it.RoleId).FirstAsync();
            if (!string.IsNullOrEmpty(userRoleIds))
            {
                roleIds = userRoleIds.Split(",",StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            userId = objectId;
        }

        // 找出当前的模块权限进行排除
        var moduleList = await _authorizeRepository.Context.Queryable<AuthorizeEntity>()
            .Where(a => a.ItemType == "module" && (roleIds.Contains(a.ObjectId) || a.ObjectId == userId))
            .Select(a=>a.ItemId)
            .ToListAsync();

        // 排除掉菜单模块
        input.button = input.button.Except(moduleList).ToList();
        input.column = input.column.Except(moduleList).ToList();
        input.form = input.form.Except(moduleList).ToList();
        input.resource = input.resource.Except(moduleList).ToList();
        List<AuthorizeDisableEntity>? authorizeList = new List<AuthorizeDisableEntity>();
        AddAuthorizeEntity(ref authorizeList, input.module, objectId, input.objectType, "module");
        AddAuthorizeEntity(ref authorizeList, input.button, objectId, input.objectType, "button");
        AddAuthorizeEntity(ref authorizeList, input.column, objectId, input.objectType, "column");
        AddAuthorizeEntity(ref authorizeList, input.form, objectId, input.objectType, "form");
        AddAuthorizeEntity(ref authorizeList, input.resource, objectId, input.objectType, "resource");
        try
        {
            // 开启事务
            _db.BeginTran();

            // 删除除了门户外的相关权限
            await _authorizeRepository.DeleteAsync(a => a.ObjectId == objectId && a.ObjectType == input.objectType);

            if (authorizeList.Count > 0)
            {
                // 新增权限
                await _authorizeRepository.Context.Insertable(authorizeList).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
            }

            _db.CommitTran();
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw;
        }
    }

    /// <summary>
    /// 禁用的权限数据（菜单权限、功能权限、表单权限、列表权限、数据权限一起返回）.
    /// </summary>
    /// <param name="objectId">对象主键.</param>
    /// <param name="input">参数(type=User | Role).</param>
    /// <returns></returns>
    [HttpPost("Data/{objectId}/Values/All")]
    public async Task<dynamic> GetDataValuesAll(string objectId, [FromBody] AuthorizeDataQuery input)
    {
        var list = await _authorizeRepository.AsQueryable().Where(a => a.ObjectId == objectId).ToListAsync();

        Dictionary<string, List<string>> keyValuePairs = new Dictionary<string, List<string>>();
        foreach (var item in new string[] { "module", "button", "column", "form", "resource" })
        {
            keyValuePairs.Add(item, list.Where(it => it.ItemType == item).Select(it => it.ItemId).ToList());
        }
        return keyValuePairs;
    }

    #region PrivateMethod
    /// <summary>
    /// 添加权限接口参数组装.
    /// </summary>
    /// <param name="list">返回参数.</param>
    /// <param name="itemIds">权限数据id.</param>
    /// <param name="objectId">对象ID.</param>
    /// <param name="objectType">分类.</param>
    /// <param name="itemType">权限分类.</param>
    private void AddAuthorizeEntity(ref List<AuthorizeDisableEntity> list, List<string> itemIds, string objectId, string objectType, string itemType)
    {
        foreach (string? item in itemIds)
        {
            AuthorizeDisableEntity? entity = new AuthorizeDisableEntity();
            entity.Id = SnowflakeIdHelper.NextId();
            entity.CreatorTime = DateTime.Now;
            entity.CreatorUserId = _userManager.UserId;
            entity.ItemId = item;
            entity.ObjectId = objectId;
            entity.ItemType = itemType;
            entity.ObjectType = objectType;
            entity.SortCode = itemIds.IndexOf(item);
            list.Add(entity);
        }
    } 
    #endregion
}
