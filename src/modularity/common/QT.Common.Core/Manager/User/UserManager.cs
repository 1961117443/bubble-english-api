using QT.Common.Const;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Models.Authorize;
using QT.Common.Models.User;
using QT.Common.Net;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using Microsoft.AspNetCore.Http;
using SqlSugar;
using System.Security.Claims;
using System.ComponentModel.Design;
using Microsoft.AspNetCore.Identity;
using QT.FriendlyException;
using QT.Common.Core.Service;

namespace QT.Common.Core.Manager;

/// <summary>
/// 用户管理.
/// </summary>
public class UserManager : IUserManager, IScoped
{
    /// <summary>
    /// 用户表仓储.
    /// </summary>
    private readonly ISqlSugarRepository<UserEntity> _repository;

    /// <summary>
    /// 缓存管理.
    /// </summary>
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 当前Http请求.
    /// </summary>
    private readonly HttpContext _httpContext;

    /// <summary>
    /// 用户Claim主体.
    /// </summary>
    private readonly ClaimsPrincipal _user;

    private UserEntity _userEntity;

    /// <summary>
    /// 初始化一个<see cref="UserManager"/>类型的新实例.
    /// </summary>
    /// <param name="repository">用户仓储.</param>
    /// <param name="cacheManager">缓存管理.</param>
    public UserManager(
        ISqlSugarRepository<UserEntity> repository,
        ICacheManager cacheManager)
    {
        _repository = repository;
        _cacheManager = cacheManager;
        _httpContext = App.HttpContext;
        _user = _httpContext?.User ?? new ClaimsPrincipal();
    }

    /// <summary>
    /// 用户信息.
    /// </summary>
    public UserEntity User
    {
        get => _userEntity ?? (_userEntity = _repository.Single(u => u.Id == UserId));
    }

    /// <summary>
    /// 用户ID.
    /// </summary>
    public string UserId
    {
        get => _user.FindFirst(ClaimConst.CLAINMUSERID)?.Value;
    }

    /// <summary>
    /// 获取用户角色.
    /// </summary>
    public List<string> Roles
    {
        get
        {
            var user = _repository.Single(u => u.Id == UserId);
            return GetUserRoleIds(user.RoleId, user.OrganizeId);
        }
    }

    /// <summary>
    /// 用户账号.
    /// </summary>
    public string Account
    {
        get => _user.FindFirst(ClaimConst.CLAINMACCOUNT)?.Value;
    }

    /// <summary>
    /// 用户昵称.
    /// </summary>
    public string RealName
    {
        get => _user.FindFirst(ClaimConst.CLAINMREALNAME)?.Value;
    }

    /// <summary>
    /// 当前用户 token.
    /// </summary>
    public string ToKen
    {
        get => App.HttpContext?.Request.Headers["Authorization"];
    }

    /// <summary>
    /// 租户ID.
    /// </summary>
    public string TenantId
    {
        get => _user.FindFirst(ClaimConst.TENANTID)?.Value;
    }

    /// <summary>
    /// 租户数据库名称.
    /// </summary>
    public string TenantDbName
    {
        get => _user.FindFirst(ClaimConst.TENANTDBNAME)?.Value;
    }

    /// <summary>
    /// 是否是管理员.
    /// </summary>
    public bool IsAdministrator
    {
        get => _user.FindFirst(ClaimConst.CLAINMADMINISTRATOR)?.Value == ((int)Enum.AccountType.Administrator).ToString();
    }

    /// <summary>
    /// 获取用户的数据范围.
    /// </summary>
    public List<UserDataScopeModel> DataScope
    {
        get
        {
            return GetUserDataScope(UserId);
        }
    }

    /// <summary>
    /// 获取请求端类型 pc 、 app.
    /// </summary>
    public string UserOrigin
    {
        get => _httpContext?.Request.Headers["qt-origin"];
    }

    /// <summary>
    /// 获取用户登录信息.
    /// </summary>
    /// <returns></returns>
    public async Task<UserInfoModel> GetUserInfo()
    {
        var data = new UserInfoModel();
        var ipAddress = NetHelper.Ip;
        var ipAddressName = ipAddress.IsNotEmptyOrNull() ? await _cacheManager.GetOrCreateAsync($"ip:{ipAddress}", async entry => await NetHelper.GetLocation(ipAddress)) : "";
        var defaultPortalId = string.Empty;
        var userDataScope = await GetUserDataScopeAsync(UserId);

        var sysConfig = await App.GetService<ICoreSysConfigService>().GetSysConfig();
        //var configKeys = new string[] { "tokentimeout", "lastlogintimeswitch" };
        //var configs = await _repository.Context.Queryable<SysConfigEntity>().Where(s => s.Category.Equals("SysConfig") && configKeys.Contains(s.Key.ToLower())).ToListAsync();
        //var sysConfigInfo = configs.FirstOrDefault(s => s.Category.Equals("SysConfig") && s.Key.ToLower().Equals("tokentimeout"));
        //var sysConfigInfo = await _repository.Context.Queryable<SysConfigEntity>().FirstAsync(s => s.Category.Equals("SysConfig") && s.Key.ToLower().Equals("tokentimeout"));
        data = await _repository.Where(a => a.Id == UserId)
           .Select(a => new UserInfoModel
           {
               userId = a.Id,
               headIcon = a.HeadIcon, // SqlFunc.MergeString("/api/File/Image/userAvatar/", a.HeadIcon),
               userAccount = a.Account,
               userName = a.RealName,
               gender = a.Gender,
               organizeId = a.OrganizeId,
               departmentId = a.OrganizeId,
               departmentName = SqlFunc.Subqueryable<OrganizeEntity>().Where(o => o.Id == SqlFunc.ToString(a.OrganizeId)).Select(o => o.FullName),
               organizeName = SqlFunc.Subqueryable<OrganizeEntity>().Where(o => o.Id == SqlFunc.ToString(a.OrganizeId)).Select(o => o.OrganizeIdTree),
               managerId = a.ManagerId,
               isAdministrator = SqlFunc.IIF(a.IsAdministrator == 1, true, false),
               portalId = SqlFunc.IIF(a.PortalId == null, defaultPortalId, a.PortalId),
               positionId = a.PositionId,
               roleId = a.RoleId,
               prevLoginTime = a.PrevLogTime,
               prevLoginIPAddress = a.PrevLogIP,
               landline = a.Landline,
               telePhone = a.TelePhone,
               manager = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == a.ManagerId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
               mobilePhone = a.MobilePhone,
               email = a.Email,
               birthday = a.Birthday,
               lastRoleId = a.LastRoleId
           }).FirstAsync() ?? throw Oops.Oh(ErrorCode.D1017).StatusCode(601);

        if (data.headIcon.IsNotEmptyOrNull() && !data.headIcon.StartsWith("/"))
        {
            data.headIcon = $"/api/File/Image/userAvatar/{data.headIcon}";
        }
        if (data != null && data.organizeName.IsNotEmptyOrNull())
        {
            var orgIdTree = data?.organizeName?.Split(',');
            var organizeName=(await GetAllOrganizes())?.Where(x => orgIdTree.Contains(x.Id)).OrderBy(x => x.SortCode).OrderBy(x => x.CreatorTime).Select(x => x.FullName).ToList();
            //var organizeName = await _repository.Context.Queryable<OrganizeEntity>().Where(x => orgIdTree.Contains(x.Id)).OrderBy(x => x.SortCode).OrderBy(x => x.CreatorTime).Select(x => x.FullName).ToListAsync();
            data.departmentName = string.Join("/", organizeName);
            data.organizeName = data.departmentName;
        }

        data.organizeName = data.departmentName;
        data.loginTime = DateTime.Now;

        //var c1 = configs.FirstOrDefault(s => s.Category.Equals("SysConfig") && s.Key.ToLower().Equals("lastlogintimeswitch"));
        //if (c1!=null)
        //{
        //    data.prevLogin = c1.Value.ParseToInt();
        //}
        data.prevLogin = sysConfig.lastLoginTimeSwitch.ParseToInt();
        //data.prevLogin = (await _repository.Context.Queryable<SysConfigEntity>().FirstAsync(x => x.Category.Equals("SysConfig") && x.Key.ToLower().Equals("lastlogintimeswitch")))?.Value.ParseToInt();
        data.loginIPAddress = ipAddress;
        data.loginIPAddressName = ipAddressName;
        data.prevLoginIPAddressName = data.prevLoginIPAddress.IsNotEmptyOrNull() ? await _cacheManager.GetOrCreateAsync($"ip:{data.prevLoginIPAddress}", async entry => await NetHelper.GetLocation(data.prevLoginIPAddress)) : "";
        data.loginPlatForm = UserAgent.GetBrowser();
        data.subsidiary = await GetSubsidiaryAsync(data.organizeId, data.isAdministrator);
        data.subordinates = await this.GetSubordinates(UserId);
        data.positionIds = data.positionId == null ? null : await GetPosition(data.positionId);
        data.positionName = data.positionIds == null ? null : string.Join(",", data.positionIds.Select(it => it.name));
        var roleList = await GetUserOrgRoleIds(data.roleId, data.organizeId);
        data.roleName = await GetRoleNameByIds(string.Join(",", roleList));
        data.roles = await GetRolesByIds(data.roleId);
        data.roleIds = roleList.ToArray();
        if (!data.isAdministrator && data.roleIds.Any())
        {
            var portalIds = await _repository.Context.Queryable<AuthorizeEntity>().In(a => a.ObjectId, data.roleIds).Where(a => a.ItemType == "portal").GroupBy(it => new { it.ItemId }).Select(it => it.ItemId).ToListAsync();
            if (portalIds.Any())
            {
                if (!portalIds.Any(x => x == data.portalId)) data.portalId = portalIds.FirstOrDefault()?.ToString();
            }
            else data.portalId = string.Empty;
        }
        //var overdueTime = sysConfigInfo != null ? sysConfigInfo.Value : "";
        //data.overdueTime = TimeSpan.FromMinutes(overdueTime.ParseToDouble());
        data.overdueTime = TimeSpan.FromMinutes(sysConfig.tokenTimeout.ParseToDouble());
        //data.overdueTime = TimeSpan.FromMinutes(sysConfigInfo.Value.ParseToDouble());
        data.dataScope = userDataScope;
        data.tenantId = TenantId;
        data.tenantDbName = _user.FindFirst(ClaimConst.TENANTDBNAME)?.Value;
        //data.isJt = !string.IsNullOrEmpty(_user.FindFirst(ClaimConst.CLAINM_JT_COMPANY_ACCOUNT)?.Value) && _user.FindFirst(ClaimConst.CLAINM_JT_COMPANY_ACCOUNT).Value == "1";
        //data.companyId = _user.FindFirst(ClaimConst.CLAINMCOMPANYID)?.Value;

        data.singleRole = this.SingleRole;// !data.isAdministrator;

        if (!string.IsNullOrEmpty(data.roleName) && data.roleName.IndexOf("客户")>-1)
        {
            data.appHome = "/pages/erp/customer/order/order";
        }

        // 根据系统配置过期时间自动过期
        await SetUserInfo(string.Format("{0}{1}_{2}", CommonConst.CACHEKEYUSER, TenantId, UserId), data, data.overdueTime.Value);
        return data;
    }

    /// <summary>
    /// 获取用户数据范围.
    /// </summary>
    /// <param name="userId">用户ID.</param>
    /// <returns></returns>
    private async Task<List<UserDataScopeModel>> GetUserDataScopeAsync(string userId)
    {
        List<UserDataScopeModel> data = new List<UserDataScopeModel>();
        List<UserDataScopeModel> subData = new List<UserDataScopeModel>();
        List<UserDataScopeModel> inteList = new List<UserDataScopeModel>();
        var list = await _repository.Context.Queryable<OrganizeAdministratorEntity>().Where(it => it.UserId == userId && it.DeleteMark == null).ToListAsync();

        // 填充数据
        foreach (var item in list)
        {
            if (item.SubLayerAdd.ParseToBool() || item.SubLayerEdit.ParseToBool() || item.SubLayerDelete.ParseToBool())
            {
                var subsidiary = (await GetSubsidiaryAsync(item.OrganizeId, false)).ToList();
                subsidiary.Remove(item.OrganizeId);
                subsidiary.ToList().ForEach(it =>
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
        var intersection = data.Select(it => it.organizeId).Intersect(subData.Select(it => it.organizeId)).ToList();
        intersection.ForEach(it =>
        {
            var parent = data.Find(item => item.organizeId == it);
            var child = subData.Find(item => item.organizeId == it);
            var add = false;
            var edit = false;
            var delete = false;
            if (parent.Add || child.Add)
                add = true;
            if (parent.Edit || child.Edit)
                edit = true;
            if (parent.Delete || child.Delete)
                delete = true;
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

    /// <summary>
    /// 获取用户数据范围.
    /// </summary>
    /// <param name="userId">用户ID.</param>
    /// <returns></returns>
    private List<UserDataScopeModel> GetUserDataScope(string userId)
    {
        List<UserDataScopeModel> data = new List<UserDataScopeModel>();
        List<UserDataScopeModel> subData = new List<UserDataScopeModel>();
        List<UserDataScopeModel> inteList = new List<UserDataScopeModel>();

        // 填充数据
        foreach (var item in _repository.Context.Queryable<OrganizeAdministratorEntity>().Where(it => it.UserId == userId && it.DeleteMark == null).ToList())
        {
            if (item.SubLayerAdd.ParseToBool() || item.SubLayerEdit.ParseToBool() || item.SubLayerDelete.ParseToBool())
            {
                var subsidiary = GetSubsidiary(item.OrganizeId, false).ToList();
                subsidiary.Remove(item.OrganizeId);
                subsidiary.ToList().ForEach(it =>
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
        var intersection = data.Select(it => it.organizeId).Intersect(subData.Select(it => it.organizeId)).ToList();
        intersection.ForEach(it =>
        {
            var parent = data.Find(item => item.organizeId == it);
            var child = subData.Find(item => item.organizeId == it);
            var add = false;
            var edit = false;
            var delete = false;
            if (parent.Add || child.Add)
                add = true;
            if (parent.Edit || child.Edit)
                edit = true;
            if (parent.Delete || child.Delete)
                delete = true;
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

    /// <summary>
    /// 获取数据条件.
    /// </summary>
    /// <typeparam name="T">实体.</typeparam>
    /// <param name="moduleId">模块ID.</param>
    /// <param name="primaryKey">表主键.</param>
    /// <param name="isDataPermissions">是否开启数据权限.</param>
    /// <param name="tableNumber">联表编号.</param>
    /// <returns></returns>
    public async Task<List<IConditionalModel>> GetConditionAsync<T>(string moduleId, string primaryKey = "F_Id", bool isDataPermissions = true, string tableNumber = "")
        where T : new()
    {
        var userInfo = await GetUserInfo();
        var conModels = new List<IConditionalModel>();
        if (this.IsAdministrator)
            return conModels;
        var items = await _repository.Context.Queryable<AuthorizeEntity, RoleEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.ObjectId && b.EnabledMark == 1 && b.DeleteMark == null))
                   .In((a, b) => b.Id, userInfo.roleIds)
                   .Where(a => a.ItemType == "resource")
                   .GroupBy(a => new { a.ItemId }).Select(a => a.ItemId).ToListAsync();

        if (!isDataPermissions)
        {
            conModels.Add(new ConditionalCollections()
            {
                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                    {
                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = string.Format("{0}{1}", tableNumber, primaryKey), ConditionalType = ConditionalType.NoEqual, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                    }
            });
            return conModels;
        }
        else if (items.Count == 0 && isDataPermissions)
        {
            conModels.Add(new ConditionalCollections()
            {
                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                    {
                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = string.Format("{0}{1}", tableNumber, primaryKey), ConditionalType = ConditionalType.Equal, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                    }
            });
            return conModels;
        }

        var resourceList = _repository.Context.Queryable<ModuleDataAuthorizeSchemeEntity>().In(it => it.Id, items).Where(it => it.ModuleId == moduleId && it.DeleteMark == null).ToList();

        if (resourceList.Any(x => x.EnCode != null && x.EnCode.Equals("qt_alldata")))
        {
            conModels.Add(new ConditionalCollections()
            {
                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>() {
                            new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = string.Format("{0}{1}", tableNumber, primaryKey), ConditionalType = ConditionalType.NoEqual, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                        }
            });
        }
        else
        {
            foreach (var item in resourceList)
            {
                foreach (var conditionItem in JsonHelper.ToList<AuthorizeModuleResourceConditionModel>(item.ConditionJson))
                {
                    foreach (var fieldItem in conditionItem.Groups)
                    {
                        var itemField = string.Format("{0}{1}", tableNumber, fieldItem.Field);
                        var itemValue = fieldItem.Value;
                        var itemMethod = (QueryType)System.Enum.Parse(typeof(QueryType), fieldItem.Op);
                        switch (itemValue)
                        {
                            // 当前用户
                            case "@userId":
                                {
                                    switch (conditionItem.Logic)
                                    {
                                        case "and":
                                            conModels.Add(new ConditionalCollections()
                                            {
                                                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                            {
                                                    new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, GetConditionalModel(itemMethod, itemField, userInfo.userId))
                                            }
                                            });
                                            break;
                                        case "or":
                                            conModels.Add(new ConditionalCollections()
                                            {
                                                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                            {
                                                    new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(itemMethod, itemField, userInfo.userId))
                                            }
                                            });
                                            break;
                                    }
                                }

                                break;

                            // 当前组织
                            case "@organizeId":
                                {
                                    if (!string.IsNullOrEmpty(userInfo.organizeId))
                                    {
                                        switch (conditionItem.Logic)
                                        {
                                            case "and":
                                                conModels.Add(new ConditionalCollections()
                                                {
                                                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                                {
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, GetConditionalModel(itemMethod, itemField, userInfo.organizeId))
                                                }
                                                });
                                                break;
                                            case "or":
                                                conModels.Add(new ConditionalCollections()
                                                {
                                                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                                {
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(itemMethod, itemField, userInfo.organizeId))
                                                }
                                                });
                                                break;
                                        }
                                    }
                                }

                                break;

                            // 当前用户集下属
                            case "@userAraSubordinates":
                                {
                                    switch (conditionItem.Logic)
                                    {
                                        case "and":
                                            conModels.Add(new ConditionalCollections()
                                            {
                                                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                            {
                                                   new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, GetConditionalModel(itemMethod, itemField, userInfo.userId)),
                                                   new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(QueryType.In, itemField, string.Join(",", userInfo.subordinates)))
                                            }
                                            });
                                            break;
                                        case "or":
                                            conModels.Add(new ConditionalCollections()
                                            {
                                                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                            {
                                                   new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(itemMethod, itemField, userInfo.userId)),
                                                   new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(QueryType.In, itemField, string.Join(",", userInfo.subordinates)))
                                            }
                                            });
                                            break;
                                    }
                                }

                                break;

                            // 当前组织及子组织
                            case "@organizationAndSuborganization":
                                {
                                    if (!string.IsNullOrEmpty(userInfo.organizeId))
                                    {
                                        switch (conditionItem.Logic)
                                        {
                                            case "and":
                                                conModels.Add(new ConditionalCollections()
                                                {
                                                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                                {
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, GetConditionalModel(itemMethod, itemField, userInfo.organizeId)),
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(QueryType.In, itemField, string.Join(",", userInfo.subsidiary)))
                                                }
                                                });
                                                break;
                                            case "or":
                                                conModels.Add(new ConditionalCollections()
                                                {
                                                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                                {
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(itemMethod, itemField, userInfo.organizeId)),
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(QueryType.In, itemField, string.Join(",", userInfo.subsidiary)))
                                                }
                                                });
                                                break;
                                        }
                                    }

                                }

                                break;
                            default:
                                {
                                    if (!string.IsNullOrEmpty(itemValue))
                                    {
                                        switch (conditionItem.Logic)
                                        {
                                            case "and":
                                                conModels.Add(new ConditionalCollections()
                                                {
                                                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                                {
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, GetConditionalModel(itemMethod, itemField, itemValue, fieldItem.Type))
                                                }
                                                });
                                                break;
                                            case "or":
                                                conModels.Add(new ConditionalCollections()
                                                {
                                                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                                {
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(itemMethod, itemField, itemValue, fieldItem.Type))
                                                }
                                                });
                                                break;
                                        }
                                    }
                                }

                                break;
                        }
                    }
                }
            }
        }

        if (resourceList.Count == 0)
        {
            conModels.Add(new ConditionalCollections()
            {
                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                    {
                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = string.Format("{0}{1}", tableNumber, primaryKey), ConditionalType = ConditionalType.Equal, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                    }
            });
        }

        return conModels;
    }

    /// <summary>
    /// 获取数据条件.
    /// </summary>
    /// <typeparam name="T">实体.</typeparam>
    /// <param name="moduleId">模块ID.</param>
    /// <param name="primaryKey">表主键.</param>
    /// <param name="isDataPermissions">是否开启数据权限.</param>
    /// <returns></returns>
    public async Task<List<IConditionalModel>> GetDataConditionAsync<T>(string moduleId, string primaryKey, bool isDataPermissions = true)
        where T : new()
    {
        var userInfo = await GetUserInfo();
        var conModels = new List<IConditionalModel>();
        if (this.IsAdministrator)
            return conModels;
        var items = await _repository.Context.Queryable<AuthorizeEntity, RoleEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.ObjectId && b.EnabledMark == 1 && b.DeleteMark == null))
                   .In((a, b) => b.Id, userInfo.roleIds)
                   .Where(a => a.ItemType == "resource")
                   .GroupBy(a => new { a.ItemId }).Select(a => a.ItemId).ToListAsync();

        if (!isDataPermissions)
        {
            conModels.Add(new ConditionalCollections()
            {
                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                    {
                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = primaryKey, ConditionalType = ConditionalType.NoEqual, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                    }
            });
            return conModels;
        }
        else if (items.Count == 0 && isDataPermissions)
        {
            conModels.Add(new ConditionalCollections()
            {
                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                    {
                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = primaryKey, ConditionalType = ConditionalType.Equal, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                    }
            });
            return conModels;
        }

        var resourceList = _repository.Context.Queryable<ModuleDataAuthorizeSchemeEntity>().In(it => it.Id, items).Where(it => it.ModuleId == moduleId && it.DeleteMark == null).ToList();

        if (resourceList.Any(x => x.EnCode != null && x.EnCode.Equals("qt_alldata")))
        {
            conModels.Add(new ConditionalCollections()
            {
                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>() {
                            new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = primaryKey, ConditionalType = ConditionalType.NoEqual, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                        }
            });
        }
        else
        {
            foreach (var item in resourceList)
            {
                foreach (var conditionItem in JsonHelper.ToList<AuthorizeModuleResourceConditionModel>(item.ConditionJson))
                {
                    foreach (var fieldItem in conditionItem.Groups)
                    {
                        var itemField = fieldItem.FieldRule == 0 ? fieldItem.Field.ReplaceRegex("^f_", string.Empty).ParseToPascalCase().ToLowerCase() : string.Format("qt_{0}_qt_{1}", fieldItem.BindTable, fieldItem.Field.ReplaceRegex("^f_", string.Empty).ParseToPascalCase().ToLowerCase());
                        var itemValue = fieldItem.Value;
                        var itemMethod = (QueryType)System.Enum.Parse(typeof(QueryType), fieldItem.Op);
                        switch (itemValue)
                        {
                            // 当前用户
                            case "@userId":
                                {
                                    switch (conditionItem.Logic)
                                    {
                                        case "and":
                                            conModels.Add(new ConditionalCollections()
                                            {
                                                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                            {
                                                    new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, GetConditionalModel(itemMethod, itemField, userInfo.userId))
                                            }
                                            });
                                            break;
                                        case "or":
                                            conModels.Add(new ConditionalCollections()
                                            {
                                                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                            {
                                                    new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(itemMethod, itemField, userInfo.userId))
                                            }
                                            });
                                            break;
                                    }
                                }

                                break;

                            // 当前组织
                            case "@organizeId":
                                {
                                    if (!string.IsNullOrEmpty(userInfo.organizeId))
                                    {
                                        switch (conditionItem.Logic)
                                        {
                                            case "and":
                                                conModels.Add(new ConditionalCollections()
                                                {
                                                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                                {
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, GetConditionalModel(itemMethod, itemField, userInfo.organizeId))
                                                }
                                                });
                                                break;
                                            case "or":
                                                conModels.Add(new ConditionalCollections()
                                                {
                                                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                                {
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(itemMethod, itemField, userInfo.organizeId))
                                                }
                                                });
                                                break;
                                        }
                                    }
                                }

                                break;

                            // 当前用户集下属
                            case "@userAraSubordinates":
                                {
                                    switch (conditionItem.Logic)
                                    {
                                        case "and":
                                            conModels.Add(new ConditionalCollections()
                                            {
                                                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                            {
                                                   new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, GetConditionalModel(itemMethod, itemField, userInfo.userId)),
                                                   new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(QueryType.In, itemField, string.Join(",", userInfo.subordinates)))
                                            }
                                            });
                                            break;
                                        case "or":
                                            conModels.Add(new ConditionalCollections()
                                            {
                                                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                            {
                                                   new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(itemMethod, itemField, userInfo.userId)),
                                                   new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(QueryType.In, itemField, string.Join(",", userInfo.subordinates)))
                                            }
                                            });
                                            break;
                                    }
                                }

                                break;

                            // 当前组织及子组织
                            case "@organizationAndSuborganization":
                                {
                                    if (!string.IsNullOrEmpty(userInfo.organizeId))
                                    {
                                        switch (conditionItem.Logic)
                                        {
                                            case "and":
                                                conModels.Add(new ConditionalCollections()
                                                {
                                                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                                {
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, GetConditionalModel(itemMethod, itemField, userInfo.organizeId)),
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(QueryType.In, itemField, string.Join(",", userInfo.subsidiary)))
                                                }
                                                });
                                                break;
                                            case "or":
                                                conModels.Add(new ConditionalCollections()
                                                {
                                                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                                {
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(itemMethod, itemField, userInfo.organizeId)),
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(QueryType.In, itemField, string.Join(",", userInfo.subsidiary)))
                                                }
                                                });
                                                break;
                                        }
                                    }

                                }

                                break;
                            default:
                                {
                                    if (!string.IsNullOrEmpty(itemValue))
                                    {
                                        switch (conditionItem.Logic)
                                        {
                                            case "and":
                                                conModels.Add(new ConditionalCollections()
                                                {
                                                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                                {
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, GetConditionalModel(itemMethod, itemField, itemValue, fieldItem.Type))
                                                }
                                                });
                                                break;
                                            case "or":
                                                conModels.Add(new ConditionalCollections()
                                                {
                                                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                                {
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(itemMethod, itemField, itemValue, fieldItem.Type))
                                                }
                                                });
                                                break;
                                        }
                                    }
                                }

                                break;
                        }
                    }
                }
            }
        }

        if (resourceList.Count == 0)
        {
            conModels.Add(new ConditionalCollections()
            {
                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                    {
                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = primaryKey, ConditionalType = ConditionalType.Equal, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                    }
            });
        }

        return conModels;
    }

    /// <summary>
    /// 获取数据条件(在线开发专用).
    /// </summary>
    /// <typeparam name="T">实体.</typeparam>
    /// <param name="primaryKey">表主键.</param>
    /// <param name="moduleId">模块ID.</param>
    /// <param name="isDataPermissions">是否开启数据权限.</param>
    /// <returns></returns>
    public List<IConditionalModel> GetCondition<T>(string primaryKey, string moduleId, bool isDataPermissions = true)
        where T : new()
    {
        var userInfo = GetUserInfo().Result;
        var conModels = new List<IConditionalModel>();
        if (this.IsAdministrator)
            return conModels;

        var items = _repository.Context.Queryable<AuthorizeEntity, RoleEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.ObjectId && b.EnabledMark == 1 && b.DeleteMark == null))
                   .In((a, b) => b.Id, userInfo.roleIds)
                   .Where(a => a.ItemType == "resource")
                   .GroupBy(a => new { a.ItemId }).Select(a => a.ItemId).ToList();

        if (!isDataPermissions)
        {
            conModels.Add(new ConditionalCollections()
            {
                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                    {
                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = primaryKey, ConditionalType = ConditionalType.NoEqual, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                    }
            });
            return conModels;
        }
        else if (items.Count == 0 && isDataPermissions)
        {
            conModels.Add(new ConditionalCollections()
            {
                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                    {
                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = primaryKey, ConditionalType = ConditionalType.Equal, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                    }
            });
            return conModels;
        }

        var resourceList = _repository.Context.Queryable<ModuleDataAuthorizeSchemeEntity>().In(it => it.Id, items).Where(it => it.ModuleId == moduleId && it.DeleteMark == null).ToList();

        // 方案和方案，分组和分组 之间 必须要 用 And 条件 拼接
        var isAnd = false;

        if (resourceList.Any(x => x.EnCode != null && x.EnCode.Equals("qt_alldata")))
        {
            conModels.Add(new ConditionalCollections()
            {
                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>() {
                            new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = primaryKey, ConditionalType = ConditionalType.NoEqual, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                        }
            });
        }
        else
        {
            foreach (var item in resourceList)
            {
                foreach (var conditionItem in item.ConditionJson.ToList<AuthorizeModuleResourceConditionModel>())
                {
                    foreach (var fieldItem in conditionItem.Groups)
                    {
                        var itemField = fieldItem.Field;
                        var itemValue = fieldItem.Value;
                        var itemMethod = (QueryType)System.Enum.Parse(typeof(QueryType), fieldItem.Op);

                        switch (itemValue)
                        {
                            // 当前用户
                            case "@userId":
                                {
                                    switch (conditionItem.Logic)
                                    {
                                        case "and":
                                            conModels.Add(new ConditionalCollections()
                                            {
                                                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                            {
                                                    new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, GetConditionalModel(itemMethod, itemField, userInfo.userId))
                                            }
                                            });
                                            break;
                                        case "or":
                                            conModels.Add(new ConditionalCollections()
                                            {
                                                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                            {
                                                    new KeyValuePair<WhereType, ConditionalModel>(isAnd ? WhereType.And : WhereType.Or, GetConditionalModel(itemMethod, itemField, userInfo.userId))
                                            }
                                            });
                                            break;
                                    }
                                }

                                break;

                            // 当前用户集下属
                            case "@userAraSubordinates":
                                {
                                    switch (conditionItem.Logic)
                                    {
                                        case "and":
                                            conModels.Add(new ConditionalCollections()
                                            {
                                                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                            {
                                                   new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, GetConditionalModel(itemMethod, itemField, userInfo.userId)),
                                                   new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(QueryType.In, itemField, string.Join(",", userInfo.subordinates)))
                                            }
                                            });
                                            break;
                                        case "or":
                                            conModels.Add(new ConditionalCollections()
                                            {
                                                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                            {
                                                   new KeyValuePair<WhereType, ConditionalModel>(isAnd ? WhereType.And : WhereType.Or, GetConditionalModel(itemMethod, itemField, userInfo.userId)),
                                                   new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(QueryType.In, itemField, string.Join(",", userInfo.subordinates)))
                                            }
                                            });
                                            break;
                                    }
                                }

                                break;

                            // 当前组织
                            case "@organizeId":
                                {
                                    if (!string.IsNullOrEmpty(userInfo.organizeId))
                                    {
                                        switch (conditionItem.Logic)
                                        {
                                            case "and":
                                                conModels.Add(new ConditionalCollections()
                                                {
                                                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                                {
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, GetConditionalModel(itemMethod, itemField, userInfo.organizeId))
                                                }
                                                });
                                                break;
                                            case "or":
                                                conModels.Add(new ConditionalCollections()
                                                {
                                                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                                {
                                                        new KeyValuePair<WhereType, ConditionalModel>(isAnd ? WhereType.And : WhereType.Or, GetConditionalModel(itemMethod, itemField, userInfo.organizeId))
                                                }
                                                });
                                                break;
                                        }
                                    }

                                }

                                break;

                            // 当前组织及子组织
                            case "@organizationAndSuborganization":
                                {
                                    if (!string.IsNullOrEmpty(userInfo.organizeId))
                                    {
                                        switch (conditionItem.Logic)
                                        {
                                            case "and":
                                                conModels.Add(new ConditionalCollections()
                                                {
                                                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                                {
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, GetConditionalModel(itemMethod, itemField, userInfo.organizeId)),
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(QueryType.In, itemField, string.Join(",", userInfo.subsidiary)))
                                                }
                                                });
                                                break;
                                            case "or":
                                                conModels.Add(new ConditionalCollections()
                                                {
                                                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                                {
                                                        new KeyValuePair<WhereType, ConditionalModel>(isAnd ? WhereType.And : WhereType.Or, GetConditionalModel(itemMethod, itemField, userInfo.organizeId)),
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(QueryType.In, itemField, string.Join(",", userInfo.subsidiary)))
                                                }
                                                });
                                                break;
                                        }
                                    }

                                }

                                break;
                            default:
                                {
                                    if (!string.IsNullOrEmpty(itemValue))
                                    {
                                        switch (conditionItem.Logic)
                                        {
                                            case "and":
                                                conModels.Add(new ConditionalCollections()
                                                {
                                                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                                {
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, GetConditionalModel(itemMethod, itemField, itemValue, fieldItem.Type))
                                                }
                                                });
                                                break;
                                            case "or":
                                                conModels.Add(new ConditionalCollections()
                                                {
                                                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                                {
                                                        new KeyValuePair<WhereType, ConditionalModel>(isAnd ? WhereType.And : WhereType.Or, GetConditionalModel(itemMethod, itemField, itemValue, fieldItem.Type))
                                                }
                                                });
                                                break;
                                        }
                                    }

                                }

                                break;
                        }

                        isAnd = false;
                    }

                    // 分组和分组
                    isAnd = true;
                }

                // 方案和方案
                isAnd = true;
            }
        }

        if (resourceList.Count == 0)
        {
            conModels.Add(new ConditionalCollections()
            {
                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                    {
                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = primaryKey, ConditionalType = ConditionalType.Equal, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                    }
            });
        }

        return conModels;
    }

    /// <summary>
    /// 下属机构.
    /// </summary>
    /// <param name="organizeId">机构ID.</param>
    /// <param name="isAdmin">是否管理员.</param>
    /// <returns></returns>
    private async Task<string[]> GetSubsidiaryAsync(string organizeId, bool isAdmin)
    {
        var data = await GetAllOrganizes(); // await _repository.Context.Queryable<OrganizeEntity>().Where(it => it.DeleteMark == null && it.EnabledMark.Equals(1)).ToListAsync();
        if (!isAdmin)
            data = data.TreeChildNode(organizeId, t => t.Id, t => t.ParentId);

        return data.Select(m => m.Id).ToArray();
    }

    private List<OrganizeEntity> _organizeEntities;
    private async Task<List<OrganizeEntity>> GetAllOrganizes()
    {
        if (!_organizeEntities.IsAny())
        {
            _organizeEntities = await _repository.Context.Queryable<OrganizeEntity>().Where(it => it.DeleteMark == null && it.EnabledMark.Equals(1)).WithCache(3600*6).ToListAsync();
        }

        return _organizeEntities;
    }

    /// <summary>
    /// 下属机构.
    /// </summary>
    /// <param name="organizeId">机构ID.</param>
    /// <param name="isAdmin">是否管理员.</param>
    /// <returns></returns>
    private string[] GetSubsidiary(string organizeId, bool isAdmin)
    {
        var data = _repository.Context.Queryable<OrganizeEntity>().Where(it => it.DeleteMark == null && it.EnabledMark.Equals(1)).WithCache(3600 * 6).ToList();
        if (!isAdmin)
            data = data.TreeChildNode(organizeId, t => t.Id, t => t.ParentId);

        return data.Select(m => m.Id).ToArray();
    }

    /// <summary>
    /// 获取下属.
    /// </summary>
    /// <param name="managerId">主管Id.</param>
    /// <returns></returns>
    private async Task<string[]> GetSubordinates(string managerId)
    {
        List<string> data = new List<string>();
        var userIds = await _repository.Where(m => m.ManagerId == managerId && m.DeleteMark == null).Select(m => m.Id).ToListAsync();
        data.AddRange(userIds);

        // 关闭无限级我的下属
        // data.AddRange(await GetInfiniteSubordinats(userIds.ToArray()));
        return data.ToArray();
    }

    /// <summary>
    /// 获取下属无限极.
    /// </summary>
    /// <param name="parentIds"></param>
    /// <returns></returns>
    private async Task<List<string>> GetInfiniteSubordinats(string[] parentIds)
    {
        List<string> data = new List<string>();
        if (parentIds.ToList().Count > 0)
        {
            var userIds = await _repository.AsQueryable().In(it => it.ManagerId, parentIds).Where(it => it.DeleteMark == null).OrderBy(it => it.SortCode).Select(it => it.Id).ToListAsync();
            data.AddRange(userIds);
            data.AddRange(await GetInfiniteSubordinats(userIds.ToArray()));
        }

        return data;
    }

    /// <summary>
    /// 获取当前用户岗位信息.
    /// </summary>
    /// <param name="PositionIds"></param>
    /// <returns></returns>
    private async Task<List<PositionInfoModel>> GetPosition(string PositionIds)
    {
        if (string.IsNullOrEmpty(PositionIds))
        {
            return new List<PositionInfoModel>();
        }
        return await _repository.Context.Queryable<PositionEntity>().In(it => it.Id, PositionIds.Split(",")).Select(it => new PositionInfoModel { id = it.Id, name = it.FullName }).ToListAsync();
    }

    /// <summary>
    /// 获取条件模型.
    /// </summary>
    /// <returns></returns>
    private ConditionalModel GetConditionalModel(QueryType expressType, string fieldName, string fieldValue, string dataType = "string")
    {
        switch (expressType)
        {
            // 模糊
            case QueryType.Contains:
                return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.Like, FieldValue = fieldValue };

            // 等于
            case QueryType.Equal:
                switch (dataType)
                {
                    case "Double":
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.Equal, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(double)) };
                    case "Int32":
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.Equal, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(int)) };
                    default:
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.Equal, FieldValue = fieldValue };
                }

            // 不等于
            case QueryType.NotEqual:
                switch (dataType)
                {
                    case "Double":
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.NoEqual, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(double)) };
                    case "Int32":
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.NoEqual, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(int)) };
                    default:
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.NoEqual, FieldValue = fieldValue };
                }

            // 小于
            case QueryType.LessThan:
                switch (dataType)
                {
                    case "Double":
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.LessThan, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(double)) };
                    case "Int32":
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.LessThan, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(int)) };
                    default:
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.LessThan, FieldValue = fieldValue };
                }

            // 小于等于
            case QueryType.LessThanOrEqual:
                switch (dataType)
                {
                    case "Double":
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.LessThanOrEqual, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(double)) };
                    case "Int32":
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.LessThanOrEqual, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(int)) };
                    default:
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.LessThanOrEqual, FieldValue = fieldValue };
                }

            // 大于
            case QueryType.GreaterThan:
                switch (dataType)
                {
                    case "Double":
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.GreaterThan, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(double)) };
                    case "Int32":
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.GreaterThan, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(int)) };
                    default:
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.GreaterThan, FieldValue = fieldValue };
                }

            // 大于等于
            case QueryType.GreaterThanOrEqual:
                switch (dataType)
                {
                    case "Double":
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.GreaterThanOrEqual, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(double)) };
                    case "Int32":
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.GreaterThanOrEqual, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(int)) };
                    default:
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.GreaterThanOrEqual, FieldValue = fieldValue };
                }

            // 包含
            case QueryType.In:
                return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.In, FieldValue = fieldValue };
        }

        return new ConditionalModel();
    }

    /// <summary>
    /// 获取角色名称 根据 角色Ids.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public async Task<string> GetRoleNameByIds(string ids)
    {
        if (ids.IsNullOrEmpty())
            return string.Empty;

        var idList = ids.Split(",").ToList();
        var nameList = new List<string>();
        var roleList =await GetAllRoles(); // await _repository.Context.Queryable<RoleEntity>().Where(x => x.DeleteMark == null && x.EnabledMark == 1).ToListAsync();
        foreach (var item in idList)
        {
            var info = roleList.Find(x => x.Id == item);
            if (info != null && info.FullName.IsNotEmptyOrNull())
            {
                nameList.Add(info.FullName);
            }
        }

        var name = string.Join(",", nameList);
        return name;
    }

    /// <summary>
    /// 获取角色集合 根据 角色Ids.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    private async Task<List<BaseKVModel>> GetRolesByIds(string ids)
    {
        if (ids.IsNullOrEmpty())
            return new List<BaseKVModel>();

        var idList = ids.Split(",").ToList();
        var roleList = (await GetAllRoles())?.Where(x => idList.Contains(x.Id))
            .Select(x => new BaseKVModel
            {
                id = x.Id,
                name = x.FullName
            }).ToList();
        //var roleList = await _repository.Context.Queryable<RoleEntity>()
        //    .Where(x => x.DeleteMark == null && x.EnabledMark == 1)
        //    .Where(x=> idList.Contains(x.Id))
        //    .Select(x=>new BaseKVModel
        //    {
        //        id = x.Id,
        //        name = x.FullName
        //    })
        //    .ToListAsync();
        return roleList;
    }


    private List<RoleEntity> _roles;
    /// <summary>
    /// 获取所有的角色
    /// </summary>
    /// <returns></returns>
    private async Task<List<RoleEntity>> GetAllRoles()
    {
        if (!_roles.IsAny())
        {
            _roles = await _repository.Context.Queryable<RoleEntity>().Where(x => x.DeleteMark == null && x.EnabledMark == 1).WithCache(3600*12).ToListAsync();
        }
        return _roles;
    }


    /// <summary>
    /// 根据角色Ids和组织Id 获取组织下的角色以及全局角色.
    /// </summary>
    /// <param name="roleIds">角色Id集合.</param>
    /// <param name="organizeId">组织Id.</param>
    /// <returns></returns>
    public async Task<List<string>> GetUserOrgRoleIds(string roleIds, string organizeId)
    {
        if (roleIds.IsNotEmptyOrNull())
        {
            var userRoleIds = roleIds.Split(",");

            // 当前组织下的角色Id 集合
            var roleList = await _repository.Context.Queryable<OrganizeRelationEntity>()
                .Where(x => x.OrganizeId == organizeId && x.ObjectType == "Role" && userRoleIds.Contains(x.ObjectId)).Select(x => x.ObjectId).ToListAsync();

            //// 全局角色Id 集合
            //var gRoleList = await _repository.Context.Queryable<RoleEntity>().Where(x => userRoleIds.Contains(x.Id) && x.GlobalMark == 1)
            //    .Where(r => r.EnabledMark == 1 && r.DeleteMark == null).Select(x => x.Id).ToListAsync();

           var gRoleList = (await GetAllRoles())?.Where(x => userRoleIds.Contains(x.Id) && x.GlobalMark == 1)
                .Where(r => r.EnabledMark == 1 && r.DeleteMark == null).Select(x => x.Id).ToList();

            roleList.AddRange(gRoleList); // 组织角色 + 全局角色

            return roleList;
        }
        else
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// 根据角色Ids和组织Id 获取组织下的角色以及全局角色.
    /// </summary>
    /// <param name="roleIds">角色Id集合.</param>
    /// <param name="organizeId">组织Id.</param>
    /// <returns></returns>
    public List<string> GetUserRoleIds(string roleIds, string organizeId)
    {
        if (roleIds.IsNotEmptyOrNull())
        {
            var userRoleIds = roleIds.Split(",");

            // 当前组织下的角色Id 集合
            var roleList = _repository.Context.Queryable<OrganizeRelationEntity>()
                .Where(x => x.OrganizeId == organizeId && x.ObjectType == "Role" && userRoleIds.Contains(x.ObjectId)).Select(x => x.ObjectId).ToList();

            // 全局角色Id 集合
            var gRoleList = _repository.Context.Queryable<RoleEntity>().Where(x => userRoleIds.Contains(x.Id) && x.GlobalMark == 1)
                .Where(r => r.EnabledMark == 1 && r.DeleteMark == null).Select(x => x.Id).ToList();

            roleList.AddRange(gRoleList); // 组织角色 + 全局角色

            return roleList;
        }
        else
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// 保存用户登录信息.
    /// </summary>
    /// <param name="cacheKey">key.</param>
    /// <param name="userInfo">用户信息.</param>
    /// <param name="timeSpan">过期时间.</param>
    /// <returns></returns>
    private async Task<bool> SetUserInfo(string cacheKey, UserInfoModel userInfo, TimeSpan timeSpan)
    {
        return await _cacheManager.SetAsync(cacheKey, userInfo, timeSpan);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string CompanyId
    {
        get
        {
            //var companyId = this.User?.OrganizeId;
            var companyId = _repository.Where(u => u.Id == UserId).Select(x => x.OrganizeId).First();
            if (string.IsNullOrEmpty(companyId))
            {
                string cacheKey = string.Format("{0}{1}_{2}", CommonConst.CACHEKEYONLINEUSER, App.User?.FindFirst(ClaimConst.CLAINMUSERID)?.Value, "company");
                var org = _cacheManager.Get<OrganizeEntity>(cacheKey);
                //var org = App.GetService<ICacheManager>().Get<OrganizeEntity>(cacheKey);
                if (org != null)
                {
                    companyId = org.Id;
                    if (App.HttpContext != null && App.HttpContext.Items != null)
                    {
                        App.HttpContext.Items.TryAdd(ClaimConst.CLAINMCOMPANYID, org.Id);
                    }
                }
            }
            return companyId?.ToString() ?? string.Empty;
        }
    }

    /// <summary>
    /// 当前角色
    /// </summary>
    public string LastRoleId => this.User?.LastRoleId ?? string.Empty;

    /// <summary>
    /// 是否单角色模式
    /// </summary>
    public bool SingleRole
    {
        get
        {
            if (IsAdministrator && Account == CommonConst.SUPPER_ADMIN_ACCOUNT)
            {
                return false;
            }
            if (_cacheManager != null)
            {
                var config = _repository.Context.Queryable<SysConfigEntity>().Where(x => x.Key == "diableSingleRole").WithCache(60*60).First();
                if (config != null && config.Value.IsNotEmptyOrNull() && config.Value == "1")
                {
                    // 停用单角色模式，SingleRole = false
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// 当前的角色集合（判断是否为单角色模式）
    /// </summary>
    public List<string> CurrentRoles
    {
        get
        {
            if (this.SingleRole)
            {
                if (string.IsNullOrEmpty(this.LastRoleId))
                {
                    throw Oops.Oh("当前未绑定角色！").StatusCode(40301);
                }
                return new List<string>() { this.LastRoleId };
            }
            return this.Roles;
        }
    }

    /// <summary>
    /// 当前的用户id + 角色id
    /// </summary>
    public List<string> CurrentUserAndRole
    {
        get
        {
            var list = this.CurrentRoles ?? new List<string>();
            list.Add(this.UserId);
            return list;
        }
    }

    /// <summary>
    /// 获取当前的模块id（菜单id）
    /// </summary>
    public string CurrentMenuId => _httpContext?.Request.Headers["qt-model"];

    public bool IsReadonlyRole
    {
        get
        {
            if (_httpContext != null && _httpContext.Items.TryGetValue(CommonConst.LoginUserDisableChangeDatabase, out var flag))
            {
                if (flag != null && bool.TryParse(flag.ToString(), out var result) && result)
                {
                    return true;
                }
            }

            return false;
        }
    }
}