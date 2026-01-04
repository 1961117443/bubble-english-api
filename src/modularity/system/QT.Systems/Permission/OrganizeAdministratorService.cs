using QT.Common.Core.Manager;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Models.User;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Dto.OrganizeAdministrator;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.Permission;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.Systems;

/// <summary>
/// 分级管理
/// </summary>
[ApiDescriptionSettings(Tag = "Permission", Name = "OrganizeAdministrator", Order = 166)]
[Route("api/permission/[controller]")]
public class OrganizeAdministratorService : IOrganizeAdministratorService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<OrganizeAdministratorEntity> _repository;

    /// <summary>
    /// 组织管理.
    /// </summary>
    private readonly IOrganizeService _organizeService;

    /// <summary>
    /// 用户管理器.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 初始化一个<see cref="OrganizeAdministratorService"/>类型的新实例.
    /// </summary>
    public OrganizeAdministratorService(
        ISqlSugarRepository<OrganizeAdministratorEntity> organizeAdministratorRepository,
        IUserManager userManager,
        IOrganizeService organizeService,
        ISqlSugarClient context)
    {
        _repository = organizeAdministratorRepository;
        _userManager = userManager;
        _organizeService = organizeService;
        _db = context.AsTenant();
    }

    #region GET

    /// <summary>
    /// 拉取机构分级管理.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        List<OrganizeAdministratorEntity>? list = await _repository.AsQueryable().Where(it => it.OrganizeId == id && it.DeleteMark == null).OrderBy(it => it.CreatorTime, OrderByType.Asc).ToListAsync();
        OrganizeAdministratorInfoOutput? entity = list.FirstOrDefault().Adapt<OrganizeAdministratorInfoOutput>();

        if (entity != null) entity.userId = string.Join(",", list.Select(it => it.UserId));
        else return new OrganizeAdministratorInfoOutput { organizeId = id };

        return entity;
    }

    #endregion

    #region POST

    /// <summary>
    /// 更新机构分级管理.
    /// </summary>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update([FromBody] OrganizeAdminIsTratorUpInput input)
    {
        if (!_userManager.DataScope.Any(it => it.organizeId == input.organizeId && it.Edit) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);
        List<string>? oldUserIds = await _repository.AsQueryable().Where(it => it.OrganizeId == input.organizeId && it.DeleteMark == null).Select(it => it.UserId).ToListAsync();

        try
        {
            _db.BeginTran(); // 开启事务

            // 计算旧用户数组与新用户数组差
            List<string>? addList = input.userId.Split(',').Except(oldUserIds).ToList();
            List<string>? editList = input.userId.Split(',').Intersect(oldUserIds).ToList();
            List<string>? delList = oldUserIds.Except(input.userId.Split(',')).ToList();

            // 创建新数据
            if (addList.Count > 0)
            {
                List<OrganizeAdministratorEntity>? addEntityList = new List<OrganizeAdministratorEntity>();
                addList.ForEach(it =>
                {
                    OrganizeAdministratorEntity entity = input.Adapt<OrganizeAdministratorEntity>();
                    entity.UserId = it;
                    addEntityList.Add(entity);
                });
                await _repository.Context.Insertable(addEntityList).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
            }

            // 修改旧数据
            if (editList.Count > 0)
            {
                List<OrganizeAdministratorEntity>? editEntityList = await _repository.AsQueryable().Where(it => it.OrganizeId == input.organizeId && editList.Contains(SqlFunc.ToString(it.UserId))).ToListAsync();
                editEntityList.ForEach(it =>
                {
                    it.ThisLayerAdd = Convert.ToInt32(input.thisLayerAdd);
                    it.ThisLayerEdit = Convert.ToInt32(input.thisLayerEdit);
                    it.ThisLayerDelete = Convert.ToInt32(input.thisLayerDelete);
                    it.SubLayerAdd = Convert.ToInt32(input.subLayerAdd);
                    it.SubLayerEdit = Convert.ToInt32(input.subLayerEdit);
                    it.SubLayerDelete = Convert.ToInt32(input.subLayerDelete);
                    it.LastModifyTime = DateTime.Now;
                    it.LastModifyUserId = _userManager.UserId;
                });
                await _repository.Context.Updateable(editEntityList).UpdateColumns(it => new
                {
                    it.ThisLayerAdd,
                    it.ThisLayerEdit,
                    it.ThisLayerDelete,
                    it.SubLayerAdd,
                    it.SubLayerEdit,
                    it.SubLayerDelete,
                    it.LastModifyTime,
                    it.LastModifyUserId
                }).ExecuteCommandAsync();
            }

            // 删除旧数据
            if (delList.Count > 0)
            {
                await _repository.Context.Updateable<OrganizeAdministratorEntity>().SetColumns(it => new OrganizeAdministratorEntity()
                {
                    EnabledMark = 0,
                    DeleteMark = 1,
                    DeleteTime = SqlFunc.GetDate(),
                    DeleteUserId = _userManager.UserId
                }).Where(it => delList.Contains(SqlFunc.ToString(it.UserId)) && it.OrganizeId == input.organizeId).ExecuteCommandAsync();
            }

            _db.CommitTran();
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw Oops.Oh(ErrorCode.D2300);
        }
    }

    #endregion

    #region PublicMethod

    /// <summary>
    /// 获取用户数据范围.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<UserDataScopeModel>> GetUserDataScopeModel(string userId)
    {
        List<UserDataScopeModel> data = new List<UserDataScopeModel>();
        List<UserDataScopeModel> subData = new List<UserDataScopeModel>();
        List<UserDataScopeModel> inteList = new List<UserDataScopeModel>();
        List<OrganizeAdministratorEntity>? list = await _repository.AsQueryable().Where(it => SqlFunc.ToString(it.UserId) == userId && it.DeleteMark == null).ToListAsync();

        // 填充数据
        foreach (OrganizeAdministratorEntity? item in list)
        {
            if (item.SubLayerAdd.ParseToBool() || item.SubLayerEdit.ParseToBool() || item.SubLayerDelete.ParseToBool())
            {
                List<string>? subsidiary = await _organizeService.GetSubsidiary(item.OrganizeId);
                subsidiary.Remove(item.OrganizeId);
                subsidiary.ForEach(it =>
                {
                    subData.Add(new UserDataScopeModel()
                    {
                        organizeId = it,
                        Add = item.SubLayerAdd.ParseToBool(),
                        Edit = item.SubLayerEdit.ParseToBool(),
                        Delete = item.SubLayerDelete.ParseToBool()
                    });
                });
            }

            if (item.ThisLayerAdd.ParseToBool() || item.ThisLayerEdit.ParseToBool() || item.ThisLayerDelete.ParseToBool())
            {
                data.Add(new UserDataScopeModel()
                {
                    organizeId = item.OrganizeId,
                    Add = item.ThisLayerAdd.ParseToBool(),
                    Edit = item.ThisLayerEdit.ParseToBool(),
                    Delete = item.ThisLayerDelete.ParseToBool()
                });
            }
        }

        /* 比较数据
        所有分级数据权限以本级权限为主 子级为辅
        将本级数据与子级数据对比 对比出子级数据内组织ID存在本级数据的组织ID*/
        List<string>? intersection = data.Select(it => it.organizeId).Intersect(subData.Select(it => it.organizeId)).ToList();
        intersection.ForEach(it =>
        {
            UserDataScopeModel? parent = data.Find(item => item.organizeId == it);
            UserDataScopeModel? child = subData.Find(item => item.organizeId == it);
            bool add = false;
            bool edit = false;
            bool delete = false;
            if (parent.Add || child.Add) add = true;
            if (parent.Edit || child.Edit) edit = true;
            if (parent.Delete || child.Delete) delete = true;
            inteList.Add(new UserDataScopeModel()
            {
                organizeId = it,
                Add = add,
                Edit = edit,
                Delete = delete
            });
            data.Remove(parent);
            subData.Remove(child);
        });
        return data.Union(subData).Union(inteList).ToList();
    }

    #endregion
}