using QT.Common.Configuration;
using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DataEncryption;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Dto.UsersCurrent;
using QT.Systems.Entitys.Model.UsersCurrent;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.Permission;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Common.Cache;
using System.ComponentModel.DataAnnotations;
using QT.Systems.Entitys.Model.Organize;
using QT.Common.Core.Filter;
using QT.Common.Core.Manager.Tenant;
using QT.Systems.Entitys;

namespace QT.Systems;

/// <summary>
/// 业务实现:个人资料.
/// </summary>
[ApiDescriptionSettings(Tag = "Permission", Name = "Current", Order = 168)]
[Route("api/permission/Users/[controller]")]
[ProhibitOperation(ProhibitOperationEnum.Allow)]
public class UsersCurrentService : IUsersCurrentService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<UserEntity> _repository;

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
    private readonly ITenantManager _tenantManager;

    /// <summary>
    /// 初始化一个<see cref="UsersCurrentService"/>类型的新实例.
    /// </summary>
    public UsersCurrentService(
        ISqlSugarRepository<UserEntity> userRepository,
        IAuthorizeService authorizeService,
        ICacheManager cacheManager,
        IUserManager userManager,
        ITenantManager tenantManager)
    {
        _repository = userRepository;
        _authorizeService = authorizeService;
        _cacheManager = cacheManager;
        _userManager = userManager;
        _tenantManager = tenantManager;
    }

    #region GET

    /// <summary>
    /// 获取我的下属.
    /// </summary>
    /// <param name="id">用户Id.</param>
    /// <returns></returns>
    [HttpGet("Subordinate/{id}")]
    public async Task<dynamic> GetSubordinate(string id)
    {
        // 获取用户Id 下属 ,顶级节点为 自己
        List<string>? userIds = new List<string>();
        if (id == "0") userIds.Add(_userManager.UserId);
        else userIds = await _repository.AsQueryable().Where(m => m.ManagerId == id && m.DeleteMark == null).Select(m => m.Id).ToListAsync();

        if (userIds.Any())
        {
            return await _repository.Context.Queryable<UserEntity, OrganizeEntity, PositionEntity>((a, b, c) => new JoinQueryInfos(JoinType.Left, b.Id == SqlFunc.ToString(a.OrganizeId), JoinType.Left, c.Id == SqlFunc.ToString(a.PositionId)))
                .WhereIF(userIds.Any(), a => userIds.Contains(a.Id))
                .Where(a => a.DeleteMark == null && a.EnabledMark == 1)
                .OrderBy(a => a.SortCode)
                .Select((a, b, c) => new UsersCurrentSubordinateOutput
                {
                    id = a.Id,
                    avatar = SqlFunc.MergeString("/api/File/Image/userAvatar/", a.HeadIcon),
                    userName = SqlFunc.MergeString(a.RealName, "/", a.Account),
                    isLeaf = false,
                    department = b.FullName,
                    position = c.FullName
                })
                .ToListAsync();
        }
        else
        {
            return new List<UsersCurrentSubordinateOutput>();
        }
    }

    /// <summary>
    /// 获取个人资料.
    /// </summary>
    /// <returns></returns>
    [HttpGet("BaseInfo")]
    public async Task<dynamic> GetBaseInfo()
    {
        UsersCurrentInfoOutput? data = await _repository.Context.Queryable<UserEntity, UserEntity>((a, d) => new JoinQueryInfos(JoinType.Left, d.Id == a.ManagerId)).Where(a => a.Id == _userManager.UserId)
            .Select((a, d) => new UsersCurrentInfoOutput
            {
                id = a.Id,
                account = SqlFunc.IIF(KeyVariable.MultiTenancy == true, SqlFunc.MergeString(_userManager.TenantId, "@", a.Account), a.Account),
                realName = a.RealName,
                position = string.Empty,
                positionId = a.PositionId,
                organizeId = a.OrganizeId,
                manager = SqlFunc.IIF(d.Account == null, null, SqlFunc.MergeString(d.RealName, "/", d.Account)),
                roleId = string.Empty,
                roleIds = a.RoleId,
                creatorTime = a.CreatorTime,
                prevLogTime = a.PrevLogTime,
                signature = a.Signature,
                gender = a.Gender.ToString(),
                nation = a.Nation,
                nativePlace = a.NativePlace,
                entryDate = a.EntryDate,
                certificatesType = a.CertificatesType,
                certificatesNumber = a.CertificatesNumber,
                education = a.Education,
                birthday = a.Birthday,
                telePhone = a.TelePhone,
                landline = a.Landline,
                mobilePhone = a.MobilePhone,
                email = a.Email,
                urgentContacts = a.UrgentContacts,
                urgentTelePhone = a.UrgentTelePhone,
                postalAddress = a.PostalAddress,
                theme = a.Theme,
                language = a.Language,
                avatar = a.HeadIcon // SqlFunc.IIF(SqlFunc.IsNullOrEmpty(SqlFunc.ToString(a.HeadIcon)), string.Empty, SqlFunc.MergeString("/api/File/Image/userAvatar/", SqlFunc.ToString(a.HeadIcon)))
            }).FirstAsync() ?? throw Oops.Oh(ErrorCode.D1017).StatusCode(601);

        if (data.avatar.IsNotEmptyOrNull() && !data.avatar.StartsWith("/"))
        {
            data.avatar = $"/api/File/Image/userAvatar/{data.avatar}";
        }

        // 组织结构
        List<string>? olist = _repository.Context.Queryable<OrganizeEntity>().ToParentList(it => it.ParentId, data.organizeId).OrderBy(x => x.CreatorTime).Select(x => x.FullName).ToList();
        data.organize = string.Join("/", olist);

        // 获取当前用户、当前组织下的所有岗位
        List<string>? pNameList = await _repository.Context.Queryable<PositionEntity, UserRelationEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.Id == b.ObjectId))
            .Where((a, b) => b.ObjectType == "Position" && b.UserId == _userManager.UserId && a.OrganizeId == data.organizeId).Select(a => a.FullName).ToListAsync();
        data.position = string.Join(",", pNameList);

        // 获取当前用户、全局角色 和当前组织下的所有角色
        List<string>? roleList = await _userManager.GetUserOrgRoleIds(data.roleIds, data.organizeId);
        data.roleId = await _userManager.GetRoleNameByIds(string.Join(",", roleList));
        data.miniProgramQRCode = await GetMiniProgramQRCode(_userManager.UserId);
        return data;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<string> GetMiniProgramQRCode(string userId)
    {
        var param = new
        {
            tenant = _tenantManager.TenantId,
            //uid = _userManager.UserId,
            page = "/pages/login/index",
            param = $"sid={userId}"
        };
        //string? encryptStr = DESCEncryption.Encrypt(param.ToJsonString(), "QT");

        WechatSceneEntity wechatSceneEntity = new WechatSceneEntity
        {
            Id = $"uid_{userId}",
            Content = param.ToJsonString()
        };

        if (!await _repository.Context.Queryable<WechatSceneEntity>().AnyAsync(x => x.Id == wechatSceneEntity.Id))
        {
            await _repository.Context.Insertable<WechatSceneEntity>(wechatSceneEntity).ExecuteCommandAsync();
        }

        //var encryptStr = DESCEncryption.Encrypt($"{param.tenant}@{wechatSceneEntity.Id}", "QT");


        var encryptStr = param.tenant.IsNotEmptyOrNull() ? $"{param.tenant}@{wechatSceneEntity.Id}" : $"{wechatSceneEntity.Id}";

        return $"https://erp.95033.cn/mp/miniprogram?secret={encryptStr}";
    }

    /// <summary>
    /// 获取系统权限.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Authorize")]
    public async Task<dynamic> GetAuthorize()
    {
        List<string>? roleIds = _userManager.Roles;
        string? userId = _userManager.UserId;
        bool isAdmin = _userManager.IsAdministrator;
        UsersCurrentAuthorizeOutput? output = new UsersCurrentAuthorizeOutput();
        List<ModuleEntity>? moduleList = await _authorizeService.GetCurrentUserModuleAuthorize(userId, isAdmin, roleIds.ToArray());
        if (moduleList.Any(it => it.Category.Equals("App")))
        {
            moduleList.Where(it => it.Category.Equals("App") && it.ParentId.Equals("-1")).ToList().ForEach(it =>
            {
                it.ParentId = "1";
            });
            moduleList.Add(new ModuleEntity()
            {
                Id = "1",
                FullName = "app菜单",
                Icon = "qt-custom qt-custom-cellphone",
                ParentId = "-1",
                Category = "App",
                Type = 1,
                SortCode = 99999
            });
        }
        List<ModuleButtonEntity>? buttonList = await _authorizeService.GetCurrentUserButtonAuthorize(userId, isAdmin, roleIds.ToArray());
        List<ModuleColumnEntity>? columnList = await _authorizeService.GetCurrentUserColumnAuthorize(userId, isAdmin, roleIds.ToArray());
        List<ModuleDataAuthorizeSchemeEntity>? resourceList = await _authorizeService.GetCurrentUserResourceAuthorize(userId, isAdmin, roleIds.ToArray());
        List<ModuleFormEntity>? formList = await _authorizeService.GetCurrentUserFormAuthorize(userId, isAdmin, roleIds.ToArray());
        if (moduleList.Count != 0)
            output.module = moduleList.Adapt<List<UsersCurrentAuthorizeMoldel>>().ToTree("-1");
        if (buttonList.Count != 0)
        {
            List<UsersCurrentAuthorizeMoldel>? menuAuthorizeData = new List<UsersCurrentAuthorizeMoldel>();
            List<string>? pids = buttonList.Select(m => m.ModuleId).ToList();
            GetParentsModuleList(pids, moduleList, ref menuAuthorizeData);
            output.button = menuAuthorizeData.Union(buttonList.Adapt<List<UsersCurrentAuthorizeMoldel>>()).ToList().ToTree("-1");
        }

        if (columnList.Count != 0)
        {
            List<UsersCurrentAuthorizeMoldel>? menuAuthorizeData = new List<UsersCurrentAuthorizeMoldel>();
            List<string>? pids = columnList.Select(m => m.ModuleId).ToList();
            GetParentsModuleList(pids, moduleList, ref menuAuthorizeData);
            output.column = menuAuthorizeData.Union(columnList.Adapt<List<UsersCurrentAuthorizeMoldel>>()).ToList().ToTree("-1");
        }

        if (resourceList.Count != 0)
        {
            List<UsersCurrentAuthorizeMoldel>? resourceData = resourceList.Select(r => new UsersCurrentAuthorizeMoldel
            {
                id = r.Id,
                parentId = r.ModuleId,
                fullName = r.FullName,
                icon = "icon-qt icon-qt-extend"
            }).ToList();
            List<UsersCurrentAuthorizeMoldel>? menuAuthorizeData = new List<UsersCurrentAuthorizeMoldel>();
            List<string>? pids = resourceList.Select(bt => bt.ModuleId).ToList();
            GetParentsModuleList(pids, moduleList, ref menuAuthorizeData);
            output.resource = menuAuthorizeData.Union(resourceData.Adapt<List<UsersCurrentAuthorizeMoldel>>()).ToList().ToTree("-1");
        }

        if (formList.Count != 0)
        {
            List<UsersCurrentAuthorizeMoldel>? formData = formList.Select(r => new UsersCurrentAuthorizeMoldel
            {
                id = r.Id,
                parentId = r.ModuleId,
                fullName = r.FullName,
                icon = "icon-qt icon-qt-extend"
            }).ToList();
            List<UsersCurrentAuthorizeMoldel>? menuAuthorizeData = new List<UsersCurrentAuthorizeMoldel>();
            List<string>? pids = formList.Select(bt => bt.ModuleId).ToList();
            GetParentsModuleList(pids, moduleList, ref menuAuthorizeData);
            output.form = menuAuthorizeData.Union(formData.Adapt<List<UsersCurrentAuthorizeMoldel>>()).ToList().ToTree("-1");
        }

        return output;
    }

    /// <summary>
    /// 获取系统日志.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("SystemLog")]
    public async Task<dynamic> GetSystemLog([FromQuery] UsersCurrentSystemLogQuery input)
    {
        string? userId = _userManager.UserId;
        PageInputBase? requestParam = input.Adapt<PageInputBase>();
        SqlSugarPagedList<UsersCurrentSystemLogOutput>? data = await _repository.Context.Queryable<SysLogEntity>()
            .WhereIF(!input.startTime.IsNullOrEmpty(), s => s.CreatorTime >= new DateTime(input.startTime.ParseToDateTime().Year, input.startTime.ParseToDateTime().Month, input.startTime.ParseToDateTime().Day, 0, 0, 0, 0))
            .WhereIF(!input.endTime.IsNullOrEmpty(), s => s.CreatorTime <= new DateTime(input.endTime.ParseToDateTime().Year, input.endTime.ParseToDateTime().Month, input.endTime.ParseToDateTime().Day, 23, 59, 59, 999))
            .WhereIF(!input.keyword.IsNullOrEmpty(), s => s.UserName.Contains(input.keyword) || s.IPAddress.Contains(input.keyword) || s.ModuleName.Contains(input.keyword))
            .Where(s => s.Category == input.category && s.UserId == userId).OrderBy(o => o.CreatorTime, OrderByType.Desc)
            .SplitTable()
            .OrderByDescending(a=>a.CreatorTime)
            .Select(a => new UsersCurrentSystemLogOutput
            {
                creatorTime = a.CreatorTime,
                userName = a.UserName,
                ipaddress = a.IPAddress,
                moduleName = a.ModuleName,
                category = a.Category,
                userId = a.UserId,
                platForm = a.PlatForm,
                requestURL = a.RequestURL,
                requestMethod = a.RequestMethod,
                requestDuration = a.RequestDuration
            }).ToPagedListAsync(requestParam.currentPage, requestParam.pageSize);
        return PageResult<UsersCurrentSystemLogOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 判断当前用户所在公司是否启动分拣特殊入库
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/EnableCreateTs")]
    public async Task<dynamic> GetErpEnableCreateTs()
    {
        var propertyJson = await _repository.Context.Queryable<OrganizeEntity>().Where(x => x.Id == _userManager.CompanyId).Select(x => x.PropertyJson).FirstAsync();

        if (propertyJson.IsNotEmptyOrNull())
        {
            try
            {
                var organizePropertyModel = propertyJson.ToObject<OrganizePropertyModel>();

                return new
                {
                    erpEnableCreateTs= organizePropertyModel.erpEnableCreateTs
                };
            }
            catch (Exception)
            {
            }
        }
        return new
        {
            erpEnableCreateTs = 0
        };
    }

    #endregion

    #region Post

    /// <summary>
    /// 修改密码.
    /// </summary>
    /// <returns></returns>
    [HttpPost("Actions/ModifyPassword")]
    public async Task ModifyPassword([FromBody] UsersCurrentActionsModifyPasswordInput input)
    {
        UserEntity? user = _userManager.User;
        if (MD5Encryption.Encrypt(input.oldPassword + user.Secretkey) != user.Password.ToLower())
            throw Oops.Oh(ErrorCode.D5007);
        string? imageCode = await GetCode(input.timestamp);
        if (!input.code.ToLower().Equals(imageCode.ToLower()))
        {
            throw Oops.Oh(ErrorCode.D5015);
        }
        else
        {
            await DelCode(input.timestamp);
            await DelUserInfo(string.Format("{0}_{1}", _userManager.TenantId, user.Id));
        }

        user.Password = MD5Encryption.Encrypt(input.password + user.Secretkey);
        user.ChangePasswordDate = DateTime.Now;
        user.LastModifyTime = DateTime.Now;
        user.LastModifyUserId = _userManager.UserId;
        int isOk = await _repository.Context.Updateable(user).UpdateColumns(it => new
        {
            it.Password,
            it.ChangePasswordDate,
            it.LastModifyUserId,
            it.LastModifyTime
        }).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D5008);
    }

    /// <summary>
    /// 修改个人资料.
    /// </summary>
    /// <returns></returns>
    [HttpPut("BaseInfo")]
    public async Task UpdateBaseInfo([FromBody] UsersCurrentInfoUpInput input)
    {
        UserEntity? userInfo = input.Adapt<UserEntity>();
        userInfo.Id = _userManager.UserId;
        userInfo.IsAdministrator = Convert.ToInt32(_userManager.IsAdministrator);
        userInfo.LastModifyTime = DateTime.Now;
        userInfo.LastModifyUserId = _userManager.UserId;
        int isOk = await _repository.Context.Updateable(userInfo).UpdateColumns(it => new
        {
            it.RealName,
            it.Signature,
            it.Gender,
            it.Nation,
            it.NativePlace,
            it.CertificatesType,
            it.CertificatesNumber,
            it.Education,
            it.Birthday,
            it.TelePhone,
            it.Landline,
            it.MobilePhone,
            it.Email,
            it.UrgentContacts,
            it.UrgentTelePhone,
            it.PostalAddress,
            it.LastModifyUserId,
            it.LastModifyTime
        }).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D5009);
    }

    /// <summary>
    /// 修改主题.
    /// </summary>
    /// <returns></returns>
    [HttpPut("SystemTheme")]
    public async Task UpdateBaseInfo([FromBody] UsersCurrentSysTheme input)
    {
        UserEntity? user = _userManager.User;
        user.Theme = input.theme;
        user.LastModifyTime = DateTime.Now;
        user.LastModifyUserId = _userManager.UserId;
        int isOk = await _repository.Context.Updateable(user).UpdateColumns(it => new
        {
            it.Theme,
            it.LastModifyUserId,
            it.LastModifyTime
        }).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D5010);
    }

    /// <summary>
    /// 修改语言.
    /// </summary>
    /// <returns></returns>
    [HttpPut("SystemLanguage")]
    public async Task UpdateLanguage([FromBody] UsersCurrentSysLanguage input)
    {
        UserEntity? user = _userManager.User;
        user.Language = input.language;
        user.LastModifyTime = DateTime.Now;
        user.LastModifyUserId = _userManager.UserId;
        int isOk = await _repository.Context.Updateable(user).UpdateColumns(it => new
        {
            it.Language,
            it.LastModifyUserId,
            it.LastModifyTime
        }).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D5011);
    }

    /// <summary>
    /// 修改头像.
    /// </summary>
    /// <returns></returns>
    [HttpPut("Avatar/{name}")]
    public async Task UpdateAvatar(string name)
    {
        UserEntity? user = _userManager.User;
        user.HeadIcon = name;
        user.LastModifyTime = DateTime.Now;
        user.LastModifyUserId = _userManager.UserId;
        int isOk = await _repository.Context.Updateable(user).UpdateColumns(it => new
        {
            it.HeadIcon,
            it.LastModifyUserId,
            it.LastModifyTime
        }).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D5012);
    }

    /// <summary>
    /// 修改头像.
    /// </summary>
    /// <returns></returns>
    [HttpPut("AvatarUrl")]
    public async Task UpdateAvatarUrl([FromQuery, Required] string url)
    {
        await this.UpdateAvatar(url);
    }

    /// <summary>
    /// 切换 默认 ： 组织、岗位、角色.
    /// </summary>
    /// <returns></returns>
    [HttpPut("major")]
    public async Task DefaultOrganize([FromBody] UsersCurrentDefaultOrganizeInput input)
    {
        UserEntity? userInfo = _userManager.User;

        switch (input.majorType)
        {
            case "Organize": // 组织
            case "Role": // 角色
                {
                    // 如果是变更角色，那么传入的 majorId={roleId}:{organizeId} 角色和组织一起变更
                    if (input.majorType == "Role")
                    {
                        var array = input.majorId.Split(':');
                        if (!array.IsAny() || array.Length != 2)
                        {
                            throw Oops.Oh($"入参[majorId={input.majorId}]异常");
                        }
                        userInfo.LastRoleId = array[0];
                        // 非全局机构才赋值.
                        if (array[1] != "1")
                        {
                            input.majorId = array[1];
                        }
                        
                    }


                    userInfo.OrganizeId = input.majorId;

                    List<string>? roleList = await _userManager.GetUserOrgRoleIds(userInfo.RoleId, userInfo.OrganizeId);

                    // 如果该组织下没有角色 则 切换组织失败
                    if (!roleList.Any())
                        throw Oops.Oh(ErrorCode.D5023);

                    // 该组织下没有任何权限 则 切换组织失败
                    if (!_repository.Context.Queryable<AuthorizeEntity>().Where(x => x.ObjectType == "Role" && x.ItemType == "module" && roleList.Contains(x.ObjectId)).Any())
                        throw Oops.Oh(ErrorCode.D5023);

                    // 获取切换组织 Id 下的所有岗位
                    List<string>? pList = await _repository.Context.Queryable<PositionEntity>().Where(x => x.OrganizeId == input.majorId).Select(x => x.Id).ToListAsync();

                    // 获取切换组织的 岗位，如果该组织没有岗位则为空
                    List<string>? idList = await _repository.Context.Queryable<UserRelationEntity>()
                        .Where(x => x.UserId == userInfo.Id && pList.Contains(x.ObjectId) && x.ObjectType == "Position").Select(x => x.ObjectId).ToListAsync();
                    userInfo.PositionId = idList.FirstOrDefault() == null ? string.Empty : idList.FirstOrDefault();
                }

                break;
            case "Position": // 岗位
                userInfo.PositionId = input.majorId;
                break;
        }

        userInfo.LastModifyTime = DateTime.Now;
        userInfo.LastModifyUserId = _userManager.UserId;
        int isOk = await _repository.Context.Updateable(userInfo).UpdateColumns(it => new
        {
            it.OrganizeId,
            it.PositionId,
            it.LastModifyUserId,
            it.LastModifyTime,
            it.LastRoleId
        }).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D5020);
    }

    /// <summary>
    /// 获取当前用户所有组织.
    /// </summary>
    /// <returns></returns>
    [HttpGet("getUserOrganizes")]
    public async Task<dynamic> GetUserOrganizes()
    {
        UserEntity? userInfo = _userManager.User;

        // 获取当前用户所有关联 组织ID 集合
        List<string>? idList = await _repository.Context.Queryable<UserRelationEntity>()
            .Where(x => x.UserId == userInfo.Id && x.ObjectType == "Organize")
            .Select(x => x.ObjectId).ToListAsync();

        // 获取所有组织
        List<OrganizeEntity>? allOranizeList = await _repository.Context.Queryable<OrganizeEntity>().Where(x => x.DeleteMark == null).OrderBy(x => x.CreatorTime).ToListAsync();
        allOranizeList.Where(x => x.OrganizeIdTree == null || x.OrganizeIdTree == string.Empty).ToList().ForEach(item => { item.OrganizeIdTree = item.Id; });

        // 根据关联组织ID 查询组织信息
        List<CurrentUserOrganizesOutput>? oList = allOranizeList.Where(x => idList.Contains(x.Id))
            .Select(x => new CurrentUserOrganizesOutput
            {
                id = x.Id,
                fullName = string.Join("/", allOranizeList.Where(all => x.OrganizeIdTree.Split(",").Contains(all.Id)).Select(s => s.FullName))
            }).ToList();

        CurrentUserOrganizesOutput? def = oList.Where(x => x.id == userInfo.OrganizeId).FirstOrDefault();
        if (def != null) def.isDefault = true;

        return oList;
    }

    /// <summary>
    /// 获取当前用户所有岗位.
    /// </summary>
    /// <returns></returns>
    [HttpGet("getUserPositions")]
    public async Task<dynamic> GetUserPositions()
    {
        UserEntity? userInfo = _userManager.User;

        // 获取当前用户所有关联 岗位ID 集合
        List<string>? idList = await _repository.Context.Queryable<UserRelationEntity>()
            .Where(x => x.UserId == userInfo.Id && x.ObjectType == "Position")
            .Select(x => x.ObjectId).ToListAsync();

        // 根据关联 岗位ID 查询岗位信息
        List<CurrentUserOrganizesOutput>? oList = await _repository.Context.Queryable<PositionEntity>()
            .Where(x => x.OrganizeId == userInfo.OrganizeId).Where(x => idList.Contains(x.Id))
            .Select(x => new CurrentUserOrganizesOutput
            {
                id = x.Id,
                fullName = x.FullName
            }).ToListAsync();

        CurrentUserOrganizesOutput? def = oList.Where(x => x.id == userInfo.PositionId).FirstOrDefault();
        if (def != null) def.isDefault = true;

        return oList;
    }

    #endregion

    #region PrivateMethod

    /// <summary>
    /// 过滤菜单权限数据.
    /// </summary>
    /// <param name="pids">其他权限数据.</param>
    /// <param name="moduleList">勾选菜单权限数据.</param>
    /// <param name="output">返回值.</param>
    private void GetParentsModuleList(List<string> pids, List<ModuleEntity> moduleList, ref List<UsersCurrentAuthorizeMoldel> output)
    {
        List<UsersCurrentAuthorizeMoldel>? authorizeModuleData = moduleList.Adapt<List<UsersCurrentAuthorizeMoldel>>();
        foreach (string? item in pids)
        {
            GteModuleListById(item, authorizeModuleData, output);
        }

        output = output.Distinct().ToList();
    }

    /// <summary>
    /// 根据菜单id递归获取authorizeDataOutputModel的父级菜单.
    /// </summary>
    /// <param name="id">菜单id.</param>
    /// <param name="authorizeModuleData">选中菜单集合.</param>
    /// <param name="output">返回数据.</param>
    private void GteModuleListById(string id, List<UsersCurrentAuthorizeMoldel> authorizeModuleData, List<UsersCurrentAuthorizeMoldel> output)
    {
        UsersCurrentAuthorizeMoldel? data = authorizeModuleData.Find(l => l.id == id);
        if (data != null)
        {
            if (!data.parentId.Equals("-1"))
            {
                if (!output.Contains(data)) output.Add(data);

                GteModuleListById(data.parentId, authorizeModuleData, output);
            }
            else
            {
                if (!output.Contains(data)) output.Add(data);
            }
        }
    }

    /// <summary>
    /// 获取验证码.
    /// </summary>
    /// <param name="timestamp">时间戳.</param>
    /// <returns></returns>
    private async Task<string> GetCode(string timestamp)
    {
        string? cacheKey = string.Format("{0}{1}", CommonConst.CACHEKEYCODE, timestamp);
        var code = await _cacheManager.GetAsync<string>(cacheKey);
        if (code.IsNullOrEmpty())
        {
            var cache = App.GetService<ICache>();
            if (cache != null)
            {
                return await cache.GetAsync<string>(cacheKey);
            }
        }

        return code;
    }

    /// <summary>
    /// 删除验证码.
    /// </summary>
    /// <param name="timestamp">时间戳.</param>
    /// <returns></returns>
    private async Task<bool> DelCode(string timestamp)
    {
        string? cacheKey = string.Format("{0}{1}", CommonConst.CACHEKEYCODE, timestamp);
        var cache = App.GetService<ICache>();
        if (cache != null)
        {
            var ok = await cache.DelAsync(cacheKey);
            return ok > 0;
        }
        return await _cacheManager.DelAsync(cacheKey);
        //return Task.FromResult(true);
    }

    /// <summary>
    /// 删除用户登录信息缓存.
    /// </summary>
    /// <param name="userId">用户ID.</param>
    /// <returns></returns>
    private Task<bool> DelUserInfo(string userId)
    {
        string? cacheKey = string.Format("{0}{1}", CommonConst.CACHEKEYUSER, userId);
        _cacheManager.DelAsync(cacheKey);
        return Task.FromResult(true);
    }

    #endregion
}