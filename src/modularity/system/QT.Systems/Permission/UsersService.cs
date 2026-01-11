using QT.Common.Configuration;
using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Core.Manager.Files;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Helper;
using QT.Common.Models.NPOI;
using QT.Common.Models.User;
using QT.Common.Security;
using QT.DataEncryption;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Dto.Organize;
using QT.Systems.Entitys.Dto.Role;
using QT.Systems.Entitys.Dto.SysConfig;
using QT.Systems.Entitys.Dto.User;
using QT.Systems.Entitys.Dto.UserRelation;
using QT.Systems.Entitys.Enum;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.Permission;
using QT.Systems.Interfaces.System;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System.Linq.Expressions;
using QT.Systems.Entitys.Dto.UsersCurrent;
using QT.Systems.Entitys.Model.UsersCurrent;
using System.Data;

namespace QT.Systems;

/// <summary>
///  业务实现：用户信息.
/// </summary>
[ApiDescriptionSettings(Tag = "Permission", Name = "Users", Order = 163)]
[Route("api/permission/[controller]")]
public class UsersService : IUsersService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<UserEntity> _repository;  // 用户表仓储

    /// <summary>
    /// 机构表服务.
    /// </summary>
    private readonly IOrganizeService _organizeService;

    /// <summary>
    /// 用户关系表服务.
    /// </summary>
    private readonly IUserRelationService _userRelationService;

    /// <summary>
    /// 系统配置服务.
    /// </summary>
    private readonly ISysConfigService _sysConfigService;

    /// <summary>
    /// 第三方同步服务.
    /// </summary>
    private readonly ISynThirdInfoService _synThirdInfoService;

    /// <summary>
    /// 文件服务.
    /// </summary>
    private readonly IFileManager _fileManager;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly ICacheManager _cacheManager;
    private readonly IAuthorizeService _authorizeService;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 初始化一个<see cref="UsersService"/>类型的新实例.
    /// </summary>
    public UsersService(
        ISqlSugarRepository<UserEntity> userRepository,
        IOrganizeService organizeService,
        IUserRelationService userRelationService,
        ISysConfigService sysConfigService,
        ISynThirdInfoService synThirdInfoService,
        IFileManager fileService,
        IUserManager userManager,
        ISqlSugarClient context,
        ICacheManager cacheManager,
        IAuthorizeService authorizeService)
    {
        _repository = userRepository;
        _organizeService = organizeService;
        _userRelationService = userRelationService;
        _sysConfigService = sysConfigService;
        _userManager = userManager;
        _cacheManager = cacheManager;
        _authorizeService = authorizeService;
        _synThirdInfoService = synThirdInfoService;
        _fileManager = fileService;
        _db = context.AsTenant();
    }

    #region GET

    /// <summary>
    /// 获取列表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] UserListQuery input)
    {
        PageInputBase? pageInput = input.Adapt<PageInputBase>();

        #region 处理组织树 名称

        List<OrganizeEntity>? orgTreeNameList = new List<OrganizeEntity>();
        List<OrganizeEntity>? orgList = await _repository.Context.Queryable<OrganizeEntity>().Where(x => x.DeleteMark == null && x.EnabledMark == 1).ToListAsync();
        orgList.ForEach(item =>
        {
            if (item.OrganizeIdTree.IsNullOrEmpty()) item.OrganizeIdTree = item.Id;
            OrganizeEntity? newItem = new OrganizeEntity();
            newItem.Id = item.Id;
            var orgNameList = new List<string>();
            item.OrganizeIdTree.Split(",").ToList().ForEach(it =>
            {
                var org = orgList.Find(x => x.Id == it);
                if (org != null) orgNameList.Add(org.FullName);
            });
            newItem.FullName = string.Join("/", orgNameList);
            orgTreeNameList.Add(newItem);
        });

        #endregion

        #region 获取组织层级

        List<string>? childOrgIds = new List<string>();
        if (input.organizeId.IsNotEmptyOrNull())
        {
            childOrgIds.Add(input.organizeId);

            // 根据组织Id 获取所有子组织Id集合
            childOrgIds.AddRange(_repository.Context.Queryable<OrganizeEntity>().Where(x => x.DeleteMark == null && x.EnabledMark == 1)
                .ToChildList(x => x.ParentId, input.organizeId).Select(x => x.Id).ToList());
            childOrgIds = childOrgIds.Distinct().ToList();
        }

        #endregion

        // 获取配置文件 账号锁定类型
        SysConfigEntity? config = await _repository.Context.Queryable<SysConfigEntity>().Where(x => x.Key.Equals("lockType") && x.Category.Equals("SysConfig")).FirstAsync();
        ErrorStrategy configLockType = (ErrorStrategy)Enum.Parse(typeof(ErrorStrategy), config?.Value);

        SqlSugarPagedList<UserListOutput>? data = new SqlSugarPagedList<UserListOutput>();
        if (childOrgIds.Any())
        {
            // 拼接查询
            List<ISugarQueryable<UserListOutput>>? listQuery = new List<ISugarQueryable<UserListOutput>>();
            /*
            foreach (string item in childOrgIds)
            {
                var quer = _repository.Context.Queryable<UserRelationEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.UserId))
                .Where((a, b) => item == a.ObjectId)
                .WhereIF(!pageInput.keyword.IsNullOrEmpty(), (a, b) => b.Account.Contains(pageInput.keyword) || b.RealName.Contains(pageInput.keyword))
                .Where((a, b) => b.DeleteMark == null)
                .Select((a, b) => new UserListOutput
                {
                    id = b.Id,
                    account = b.Account,
                    realName = b.RealName,
                    creatorTime = b.CreatorTime,
                    gender = b.Gender,
                    mobilePhone = b.MobilePhone,
                    sortCode = b.SortCode,
                    unLockTime = b.UnLockTime,
                    enabledMark = b.EnabledMark
                });
                listQuery.Add(quer);
            }
            */

            var quer = _repository.Context.Queryable<UserRelationEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Inner, b.Id == a.UserId))
                .Where((a, b) => childOrgIds.Contains(a.ObjectId))
                .WhereIF(!pageInput.keyword.IsNullOrEmpty(), (a, b) => b.Account.Contains(pageInput.keyword) || b.RealName.Contains(pageInput.keyword))
                //.WhereIF(input.hideCustomer, (a, b) => SqlFunc.Subqueryable<ErpCustomer>().AS("erp_customer").Where(xxx => (xxx.F_LoginId == b.Account || xxx.F_Admintel == b.Account)).NotAny())
                .Where((a, b) => b.DeleteMark == null)
                .Select((a, b) => new UserListOutput
                {
                    id = b.Id,
                    account = b.Account,
                    realName = b.RealName,
                    creatorTime = b.CreatorTime,
                    gender = b.Gender,
                    mobilePhone = b.MobilePhone,
                    sortCode = b.SortCode,
                    unLockTime = b.UnLockTime,
                    enabledMark = b.EnabledMark
                });

            //data = await _repository.Context.UnionAll(listQuery)
            data = await quer.MergeTable()
                .GroupBy(a => new { a.id, a.account, a.realName, a.creatorTime, a.gender, a.mobilePhone, a.sortCode, a.enabledMark, a.unLockTime })
                .Select(a => new UserListOutput
                {
                    id = a.id,
                    account = a.account,
                    realName = a.realName,
                    creatorTime = a.creatorTime,
                    gender = a.gender,
                    mobilePhone = a.mobilePhone,
                    sortCode = a.sortCode,
                    enabledMark = SqlFunc.IIF(configLockType == ErrorStrategy.Delay && a.enabledMark == 2 && a.unLockTime < DateTime.Now, 1, a.enabledMark)
                })
                // 非超级管理员，不显示sysadmin账号
                .WhereIF(_userManager.Account != CommonConst.SUPPER_ADMIN_ACCOUNT, x=>x.account !=CommonConst.SUPPER_ADMIN_ACCOUNT)
                .Where(x=> !string.IsNullOrEmpty(x.id))
                .ToPagedListAsync(input.currentPage, input.pageSize);
        }
        else
        {
            data = await _repository.Context.Queryable<UserEntity>()
                .WhereIF(!pageInput.keyword.IsNullOrEmpty(), a => a.Account.Contains(pageInput.keyword) || a.RealName.Contains(pageInput.keyword))
                //.WhereIF(input.hideCustomer, (a) => SqlFunc.Subqueryable<ErpCustomer>().AS("erp_customer").Where(xxx => (xxx.F_LoginId == a.Account || xxx.F_Admintel == a.Account)).NotAny())
                .Where(a => a.DeleteMark == null)
                // 非超级管理员，不显示sysadmin账号
                .WhereIF(_userManager.Account != CommonConst.SUPPER_ADMIN_ACCOUNT, x => x.Account != CommonConst.SUPPER_ADMIN_ACCOUNT)
                .OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).OrderBy(a => a.LastModifyTime, OrderByType.Desc)
                .Select(a => new UserListOutput
                {
                    id = a.Id,
                    account = a.Account,
                    realName = a.RealName,
                    creatorTime = a.CreatorTime,
                    gender = a.Gender,
                    mobilePhone = a.MobilePhone,
                    sortCode = a.SortCode,
                    enabledMark = SqlFunc.IIF(configLockType == ErrorStrategy.Delay && a.EnabledMark == 2 && a.UnLockTime < DateTime.Now, 1, a.EnabledMark)
                }).ToPagedListAsync(input.currentPage, input.pageSize);
        }

        #region 处理 用户 多组织

        List<UserRelationEntity>? orgUserIdAll = await _repository.Context.Queryable<UserRelationEntity>()
            .Where(x => data.list.Select(u => u.id).Contains(x.UserId)).ToListAsync();
        foreach (UserListOutput? item in data.list)
        {
            // 获取用户组织集合
            List<string>? roleOrgList = orgUserIdAll.Where(x => x.UserId == item.id).Select(x => x.ObjectId).ToList();
            item.organize = string.Join(" ; ", orgTreeNameList.Where(x => roleOrgList.Contains(x.Id)).Select(x => x.FullName));
        }

        #endregion

        return PageResult<UserListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 获取全部用户.
    /// </summary>
    /// <returns></returns>
    [HttpGet("All")]
    public async Task<dynamic> GetUserAllList()
    {
        return await _repository.Context.Queryable<UserEntity, OrganizeEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == SqlFunc.ToString(a.OrganizeId)))
            .Where(a => a.EnabledMark == 1 && a.DeleteMark == null).OrderBy(a => a.SortCode)
            .Select((a, b) => new UserListAllOutput
            {
                id = a.Id,
                account = a.Account,
                realName = a.RealName,
                headIcon = SqlFunc.MergeString("/api/File/Image/userAvatar/", a.HeadIcon),
                gender = a.Gender,
                department = b.FullName,
                sortCode = a.SortCode,
                quickQuery = a.QuickQuery,
            }).ToListAsync();
    }

    /// <summary>
    /// 获取用户数据分页 根据角色Id.
    /// </summary>
    /// <returns></returns>
    [HttpGet("getUsersByRoleId")]
    public async Task<dynamic> GetUsersByRoleId([FromQuery] RoleListInput input)
    {
        RoleEntity? roleInfo = await _repository.Context.Queryable<RoleEntity>().Where(x => x.Id == input.roleId).FirstAsync();

        // 查询全部用户 (全局角色)
        if (roleInfo.GlobalMark == 1)
        {
            SqlSugarPagedList<UserListAllOutput>? list = await _repository.Context.Queryable<UserEntity>()
                .WhereIF(!input.keyword.IsNullOrEmpty(), a => a.Account.Contains(input.keyword) || a.RealName.Contains(input.keyword))
                .Where(p => p.EnabledMark == 1 && p.DeleteMark == null).OrderBy(p => p.SortCode)
                .Select(a => new UserListAllOutput
                {
                    id = a.Id,
                    account = a.Account,
                    realName = a.RealName,
                    gender = a.Gender,
                    sortCode = a.SortCode,
                    quickQuery = a.QuickQuery
                }).ToPagedListAsync(input.currentPage, input.pageSize);

            return PageResult<UserListAllOutput>.SqlSugarPageResult(list);
        }

        // 查询角色 所属 所有组织 用户
        else
        {
            // 查询角色 所有所属组织
            List<string>? orgList = await _repository.Context.Queryable<OrganizeRelationEntity>().Where(x => x.ObjectType == "Role" && x.ObjectId == roleInfo.Id).Select(x => x.OrganizeId).ToListAsync();

            List<string>? userIdList = await _repository.Context.Queryable<UserRelationEntity>().Where(x => x.ObjectType == "Organize" && orgList.Contains(x.ObjectId)).Select(x => x.UserId).Distinct().ToListAsync();

            SqlSugarPagedList<UserListAllOutput>? list = await _repository.Context.Queryable<UserEntity>()
                .Where(a => userIdList.Contains(a.Id))
                .Where(p => p.EnabledMark == 1 && p.DeleteMark == null).OrderBy(p => p.SortCode)
                .WhereIF(!input.keyword.IsNullOrEmpty(), a => a.Account.Contains(input.keyword) || a.RealName.Contains(input.keyword))
                .Select(a => new UserListAllOutput
                {
                    id = a.Id,
                    account = a.Account,
                    realName = a.RealName,
                    gender = a.Gender,
                    sortCode = a.SortCode,
                    quickQuery = a.QuickQuery,
                }).ToPagedListAsync(input.currentPage, input.pageSize);

            return PageResult<UserListAllOutput>.SqlSugarPageResult(list);
        }
    }

    /// <summary>
    /// 获取用户数据 根据角色所属组织.
    /// </summary>
    /// <returns></returns>
    [HttpGet("GetUsersByRoleOrgId")]
    public async Task<dynamic> GetUsersByRoleOrgId([FromQuery] RoleListInput input)
    {
        RoleEntity? roleInfo = await _repository.Context.Queryable<RoleEntity>().Where(x => x.Id == input.roleId).FirstAsync();
        input.organizeId = input.organizeId == null ? "0" : input.organizeId;

        // 获取角色所属组织集合
        List<string>? orgList = await _repository.Context.Queryable<OrganizeRelationEntity>().Where(x => x.ObjectType == "Role" && x.ObjectId == roleInfo.Id).Select(x => x.OrganizeId).ToListAsync();

        List<OrganizeMemberListOutput>? output = new List<OrganizeMemberListOutput>();
        if (input.organizeId.Equals("0"))
        {
            if (input.keyword.IsNotEmptyOrNull())
            {
                // 获取角色所属组织 成员id
                output = await _repository.Context.Queryable<UserEntity, UserRelationEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.UserId == a.Id))
                .Where((a, b) => b.ObjectType == "Organize" && orgList.Contains(b.ObjectId)).Where((a, b) => a.EnabledMark == 1 && a.DeleteMark == null)
                .Where((a, b) => a.RealName.Contains(input.keyword) || a.Account.Contains(input.keyword))
                .GroupBy((a, b) => new { a.Id, a.RealName, a.Account, a.EnabledMark })
                .Select((a, b) => new OrganizeMemberListOutput()
                {
                    id = a.Id,
                    fullName = a.RealName + "/" + a.Account,
                    enabledMark = a.EnabledMark,
                    type = "user",
                    icon = "icon-qt icon-qt-tree-user2",
                    hasChildren = false,
                    isLeaf = true
                }).ToListAsync();
            }
            else
            {
                List<OrganizeEntity>? allOrg = await _repository.Context.Queryable<OrganizeEntity>().Where(o => o.DeleteMark == null && o.EnabledMark == 1).OrderBy(o => o.ParentId).ToListAsync();

                List<OrganizeEntity>? data = await _repository.Context.Queryable<OrganizeEntity>()
                    .Where(o => orgList.Contains(o.Id) && o.DeleteMark == null && o.EnabledMark == 1)
                    .OrderBy(o => o.SortCode).ToListAsync();

                foreach (OrganizeEntity? o in data)
                {
                    if (o.OrganizeIdTree.IsNullOrEmpty()) o.OrganizeIdTree = o.Id;
                    if (!data.Where(x => x.Id != o.Id && o.OrganizeIdTree.Contains(x.OrganizeIdTree)).Any())
                    {
                        var oids = o.OrganizeIdTree?.Split(",").ToList();
                        if (!oids.Any()) oids = new List<string>() { o.FullName };
                        var orgTree = allOrg.Where(x => oids.Contains(x.Id)).Select(x => x.FullName).ToList();

                        output.Add(new OrganizeMemberListOutput
                        {
                            id = o.Id,
                            fullName = string.Join("/", orgTree),
                            enabledMark = o.EnabledMark,
                            type = o.Category,
                            icon = "icon-qt icon-qt-tree-organization3",
                            hasChildren = true,
                            isLeaf = false
                        });
                    }
                }
            }
        }
        else
        {
            List<OrganizeEntity>? allOrg = await _repository.Context.Queryable<OrganizeEntity>().Where(o => o.DeleteMark == null && o.EnabledMark == 1).OrderBy(o => o.ParentId).ToListAsync();

            output = await _repository.Context.Queryable<UserEntity, UserRelationEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.UserId == a.Id))
                .Where((a, b) => b.ObjectType == "Organize" && b.ObjectId == input.organizeId).Where((a, b) => a.EnabledMark == 1 && a.DeleteMark == null)
                .GroupBy((a, b) => new { a.Id, a.RealName, a.Account, a.EnabledMark })
                .Select((a, b) => new OrganizeMemberListOutput()
                {
                    id = a.Id,
                    fullName = a.RealName + "/" + a.Account,
                    enabledMark = a.EnabledMark,
                    type = "user",
                    icon = "icon-qt icon-qt-tree-user2",
                    hasChildren = false,
                    isLeaf = true
                }).ToListAsync();
            var departmentList = await _repository.Context.Queryable<OrganizeEntity>().Where(o => o.OrganizeIdTree.Contains(input.organizeId) && orgList.Contains(o.Id)).ToListAsync();

            departmentList.OrderBy(x => x.OrganizeIdTree.Length).ToList().ForEach(o =>
            {
                var pOrgTree = departmentList.Where(x => x.OrganizeIdTree != o.OrganizeIdTree && o.OrganizeIdTree.Contains(x.OrganizeIdTree)).FirstOrDefault()?.OrganizeIdTree;
                if (pOrgTree.IsNotEmptyOrNull() && o.OrganizeIdTree.IsNotEmptyOrNull()) o.OrganizeIdTree = o.OrganizeIdTree.Replace(pOrgTree, "");
                var oids = o.OrganizeIdTree?.Split(",").ToList();
                if (!oids.Any()) oids = new List<string>() { o.FullName };

                var orgTree = allOrg.Where(x => oids.Contains(x.Id)).Select(x => x.FullName).ToList();
                var treeName = string.Join("/", orgTree);

                if (o.Id != input.organizeId && !output.Any(x => treeName.Contains(x.fullName)))
                {
                    output.Add(new OrganizeMemberListOutput()
                    {
                        id = o.Id,
                        fullName = treeName,
                        enabledMark = o.EnabledMark,
                        type = o.Category,
                        icon = "icon-qt icon-qt-tree-department1",
                        hasChildren = true,
                        isLeaf = false
                    });
                }
            });
        }

        return output;
    }

    /// <summary>
    /// 获取IM用户列表.
    /// </summary>
    /// <returns></returns>
    [HttpGet("ImUser")]
    public async Task<dynamic> GetImUserList([FromQuery] PageInputBase input)
    {
        SqlSugarPagedList<IMUserListOutput>? list = await _repository.Context.Queryable<UserEntity, OrganizeEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == SqlFunc.ToString(a.OrganizeId)))
            .WhereIF(!input.keyword.IsNullOrEmpty(), a => a.Account.Contains(input.keyword) || a.RealName.Contains(input.keyword))
            .Where(a => a.Id != _userManager.UserId && a.EnabledMark == 1 && a.DeleteMark == null).OrderBy(a => a.SortCode)
            .Select((a, b) => new IMUserListOutput
            {
                id = a.Id,
                account = a.Account,
                realName = a.RealName,
                headIcon = SqlFunc.MergeString("/api/File/Image/userAvatar/", a.HeadIcon),
                department = b.FullName,
            }).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<IMUserListOutput>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 获取下拉框（公司+部门+用户）.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<dynamic> GetSelector()
    {
        List<UserEntity>? userList = await _repository.AsQueryable().Where(t => t.EnabledMark == 1 && t.DeleteMark == null).OrderBy(u => u.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).ToListAsync();
        List<UserSelectorOutput>? treeList = userList.Adapt<List<UserSelectorOutput>>();
        treeList.ForEach(t =>
        {
            t.num = 1;
            t.sortCode = 0;
        });
        List<OrganizeEntity>? organizeList = await _organizeService.GetListAsync();
        organizeList.ForEach(item =>
        {
            string? icon = string.Empty;
            if (item.Category.Equals("department"))
                icon = "icon-qt icon-qt-tree-department1";
            else
                icon = "icon-qt icon-qt-tree-organization3";
            treeList.Add(
                new UserSelectorOutput
                {
                    id = item.Id,
                    parentId = item.ParentId,
                    fullName = item.FullName,
                    enabledMark = item.EnabledMark,
                    icon = icon,
                    type = item.Category,
                    organizeInfo = item.OrganizeIdTree,
                    num = userList.Count(x => x.OrganizeId == item.Id),
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
        UserEntity? entity = await _repository.FirstOrDefaultAsync(u => u.Id == id);
        SysConfigEntity? config = await _repository.Context.Queryable<SysConfigEntity>().Where(x => x.Key.Equals("lockType") && x.Category.Equals("SysConfig")).FirstAsync();
        string? configLockType = config?.Value;
        entity.EnabledMark = configLockType.IsNotEmptyOrNull() && configLockType == "2" && entity.EnabledMark == 2 && entity.UnLockTime < DateTime.Now ? 1 : entity.EnabledMark;
        UserInfoOutput? output = entity.Adapt<UserInfoOutput>();
        if (output.headIcon == "/api/File/Image/userAvatar/") output.headIcon = string.Empty;
        if (entity != null)
        {
            List<UserRelationEntity>? allRelationList = await _userRelationService.GetListByUserId(id);
            var relationIds = allRelationList.Where(x => x.ObjectType == "Organize" || x.ObjectType == "Position").Select(x => new { x.ObjectId, x.ObjectType }).ToList();
            List<OrganizeEntity>? oList = await _repository.Context.Queryable<OrganizeEntity>().Where(x => relationIds.Where(x => x.ObjectType == "Organize").Select(x => x.ObjectId).Contains(x.Id)).ToListAsync();
            output.organizeIdTree = new List<List<string>>();
            oList.ForEach(item =>
            {
                if (item.OrganizeIdTree.IsNotEmptyOrNull()) output.organizeIdTree.Add(item.OrganizeIdTree.Split(",").ToList());
            });
            output.organizeId = string.Join(",", relationIds.Where(x => x.ObjectType == "Organize").Select(x => x.ObjectId));
            output.positionId = string.Join(",", relationIds.Where(x => x.ObjectType == "Position").Select(x => x.ObjectId));
        }

        return output;
    }

    /// <summary>
    /// 获取当前用户所属机构下属成员.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("getOrganization")]
    public async Task<dynamic> GetOrganizeMember([FromQuery] UserListQuery input)
    {
        UserInfoModel? user = await _userManager.GetUserInfo();
        if (input.organizeId.IsNotEmptyOrNull() && input.organizeId != "0") input.organizeId = input.organizeId.Split(",").LastOrDefault();
        else input.organizeId = user.organizeId;

        // 获取岗位所属组织的所有成员
        //List<string>? userList = await _repository.Context.Queryable<UserRelationEntity>().Where(x => x.ObjectType == "Organize" && x.ObjectId == input.organizeId).Select(x => x.UserId).Distinct().ToListAsync();

        return await _repository.AsQueryable()
                .WhereIF(!input.keyword.IsNullOrEmpty(), u => u.Account.Contains(input.keyword) || u.RealName.Contains(input.keyword))
                .Where(u => u.EnabledMark == 1 && u.DeleteMark == null /*&& userList.Contains(u.Id)*/)
                .Where(u=> SqlFunc.Subqueryable<UserRelationEntity>().Where(x => x.UserId == u.Id && x.ObjectType == "Organize" && x.ObjectId == input.organizeId).Any())
                .OrderBy(o => o.SortCode)
                .Select(u => new OrganizeMemberListOutput
                {
                    id = u.Id,
                    fullName = SqlFunc.MergeString(u.RealName, "/", u.Account),
                    enabledMark = u.EnabledMark,
                    icon = "icon-qt icon-qt-tree-user2",
                    isLeaf = true,
                    hasChildren = false,
                    type = "user",
                }).ToListAsync();
    }

    /// <summary>
    /// 获取用户系统权限.
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id}/Authorize")]
    public async Task<dynamic> GetAuthorize([FromRoute]string id)
    {
        var user = await _repository.AsQueryable().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.D5002);
        List<string>? roleIds = user.RoleId.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
        string? userId = user.Id;
        bool isAdmin = user.IsAdministrator == 1;
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
    #endregion

    #region POST

    /// <summary>
    /// 获取.
    /// </summary>
    /// <returns></returns>
    [HttpPost("GetUserList")]
    public async Task<dynamic> GetUserList([FromBody] UserRelationInput input)
    {
        var data = await _repository.Context
            .Queryable<UserEntity>().Where(it => it.EnabledMark > 0 && it.DeleteMark == null)
            .Where(it => input.userId.Contains(it.Id))
            .Select(it => new
            {
                id = it.Id,
                fullName = SqlFunc.MergeString(it.RealName, "/", it.Account),
                enabledMark = it.EnabledMark,
                deleteMark = it.DeleteMark,
                sortCode = it.SortCode
            })
            .OrderBy(it => it.sortCode).ToListAsync();
        return new { list = data };
    }

    /// <summary>
    /// 获取机构成员列表.
    /// </summary>
    /// <param name="organizeId">机构ID.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("ImUser/Selector/{organizeId}")]
    public async Task<dynamic> GetOrganizeMemberList(string organizeId, [FromBody] KeywordInput input)
    {
        List<OrganizeMemberListOutput>? output = new List<OrganizeMemberListOutput>();
        if (!input.keyword.IsNullOrEmpty())
        {
            output = await _repository.AsQueryable()
                .WhereIF(!input.keyword.IsNullOrEmpty(), u => u.Account.Contains(input.keyword) || u.RealName.Contains(input.keyword))
                .Where(u => u.EnabledMark > 0 && u.DeleteMark == null).OrderBy(o => o.SortCode)
                .Select(u => new OrganizeMemberListOutput
                {
                    id = u.Id,
                    fullName = SqlFunc.MergeString(u.RealName, "/", u.Account),
                    enabledMark = SqlFunc.IIF(u.EnabledMark == 2 && u.UnLockTime < DateTime.Now, 1, u.EnabledMark),
                    icon = "icon-qt icon-qt-tree-user2",
                    isLeaf = true,
                    hasChildren = false,
                    type = "user",
                }).Take(50).ToListAsync();
        }
        else
        {
            output = await _organizeService.GetOrganizeMemberList(organizeId);
        }

        return new { list = output };
    }

    /// <summary>
    /// 获取当前用户下属成员.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("getSubordinates")]
    public async Task<dynamic> GetSubordinate([FromBody] KeywordInput input)
    {
        return await _repository.AsQueryable()
                .WhereIF(!input.keyword.IsNullOrEmpty(), u => u.Account.Contains(input.keyword) || u.RealName.Contains(input.keyword))
                .Where(u => u.EnabledMark == 1 && u.DeleteMark == null && u.ManagerId == _userManager.UserId).OrderBy(o => o.SortCode)
                .Select(u => new OrganizeMemberListOutput
                {
                    id = u.Id,
                    fullName = SqlFunc.MergeString(u.RealName, "/", u.Account),
                    enabledMark = u.EnabledMark,
                    icon = "icon-qt icon-qt-tree-user2",
                    isLeaf = true,
                    hasChildren = false,
                    type = "user",
                }).ToListAsync();
    }

    /// <summary>
    /// 获取当前用户所属机构下属成员.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("GetUsersByPositionId")]
    public async Task<dynamic> GetUsersByPositionId([FromQuery] UserListQuery input)
    {
        List<OrganizeMemberListOutput>? outData = new List<OrganizeMemberListOutput>();
        UserEntity? user = _userManager.User;

        // 获取岗位所属组织信息
        OrganizeMemberListOutput? orgInfo = await _repository.Context.Queryable<PositionEntity, OrganizeEntity>((a, b) =>
                new JoinQueryInfos(JoinType.Left, b.Id == SqlFunc.ToString(a.OrganizeId) && b.EnabledMark == 1 && b.DeleteMark == null))
            .Where((a, b) => a.Id == input.positionId).Select((a, b) => new OrganizeMemberListOutput
            {
                id = b.Id,
                fullName = b.FullName,
                enabledMark = b.EnabledMark,
                type = b.Category,
                parentId = "0",
                RealName = b.OrganizeIdTree,
                icon = "icon-qt icon-qt-tree-department1",
                hasChildren = true,
                isLeaf = false
            }).FirstAsync();

        // 处理组织树
        if (orgInfo.RealName.IsNotEmptyOrNull())
        {
            string[]? treeId = orgInfo.RealName.Split(",");
            List<string>? treeNameList = await _repository.Context.Queryable<OrganizeEntity>().Where(x => treeId.Contains(x.Id)).OrderBy(x => x.ParentId).Select(x => x.FullName).ToListAsync();
            orgInfo.fullName = string.Join("/", treeNameList);
        }

        outData.Add(orgInfo);

        // 获取岗位所属组织的所有成员
        List<OrganizeMemberListOutput>? userData = await _repository.Context.Queryable<UserRelationEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == SqlFunc.ToString(a.UserId)))
            .Where((a, b) => a.ObjectType == "Organize" && a.ObjectId == orgInfo.id && b.EnabledMark == 1 && b.DeleteMark == null)
            .WhereIF(!input.keyword.IsNullOrEmpty(), (a, b) => b.Account.Contains(input.keyword) || b.RealName.Contains(input.keyword))
            .Select((a, b) => new OrganizeMemberListOutput
            {
                id = b.Id,
                parentId = orgInfo.id,
                fullName = SqlFunc.MergeString(b.RealName, "/", b.Account),
                enabledMark = b.EnabledMark,
                icon = "icon-qt icon-qt-tree-user2",
                isLeaf = true,
                hasChildren = false,
                type = "user"
            }).ToListAsync();

        outData.AddRange(userData);

        return outData.ToTree("0");
    }

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] UserCrInput input)
    {
        var orgids = input.organizeId.Split(',');
        if (!_userManager.DataScope.Any(it => orgids.Contains(it.organizeId) && it.Add) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);
        if (await _repository.AnyAsync(u => u.Account == input.account && u.DeleteMark == null)) throw Oops.Oh(ErrorCode.D1003);
        UserEntity? entity = input.Adapt<UserEntity>();

        #region 用户表单

        entity.IsAdministrator = 0;
        entity.EntryDate = input.entryDate.IsNullOrEmpty() ? DateTime.Now : input.entryDate;
        entity.Birthday = input.birthday.IsNullOrEmpty() ? DateTime.Now : input.birthday;
        entity.QuickQuery = PinyinHelper.PinyinString(input.realName);
        entity.Secretkey = Guid.NewGuid().ToString();
        entity.Password = MD5Encryption.Encrypt(MD5Encryption.Encrypt(CommonConst.DEFAULTPASSWORD) + entity.Secretkey);
        string? headIcon = input.headIcon.Split('/').ToList().Last();
        if (string.IsNullOrEmpty(headIcon))
            headIcon = "001.png";
        entity.HeadIcon = headIcon;

        #region 多组织 优先选择有权限组织

        // 多组织
        string[]? orgList = entity.OrganizeId.Split(",");
        entity.OrganizeId = string.Empty;

        foreach (string? item in orgList)
        {
            List<string>? roleList = await _userManager.GetUserOrgRoleIds(entity.RoleId, item);

            // 如果该组织下有角色并且有角色权限 则为默认组织
            if (roleList.Any() && _repository.Context.Queryable<AuthorizeEntity>().Where(x => x.ObjectType == "Role" && x.ItemType == "module" && roleList.Contains(x.ObjectId)).Any())
            {
                // 多 组织 默认
                entity.OrganizeId = item;
                break;
            }
        }

        // 如果所选组织下都没有角色或者没有角色权限 默认取第一个
        if (entity.OrganizeId.IsNullOrEmpty()) entity.OrganizeId = input.organizeId.Split(",").FirstOrDefault();

        #endregion

        string[]? positionIds = entity.PositionId?.Split(",");
        List<string>? pIdList = await _repository.Context.Queryable<PositionEntity>().Where(x => x.OrganizeId == entity.OrganizeId && positionIds.Contains(x.Id)).Select(x => x.Id).ToListAsync();
        entity.PositionId = pIdList.FirstOrDefault(); // 多 岗位 默认取当前组织第一个

        #endregion

        try
        {
            // 开启事务
            _db.BeginTran();

            // 新增用户记录
            await _repository.Context.Insertable(entity).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();

            // 将临时文件迁移至正式文件
            FileHelper.MoveFile(Path.Combine(FileVariable.TemporaryFilePath , headIcon), Path.Combine(FileVariable.UserAvatarFilePath , headIcon));

            List<UserRelationEntity>? userRelationList = new List<UserRelationEntity>();
            userRelationList.AddRange(_userRelationService.CreateUserRelation(entity.Id, input.roleId, "Role"));
            userRelationList.AddRange(_userRelationService.CreateUserRelation(entity.Id, input.positionId, "Position"));
            userRelationList.AddRange(_userRelationService.CreateUserRelation(entity.Id, input.organizeId, "Organize"));
            userRelationList.AddRange(_userRelationService.CreateUserRelation(entity.Id, input.groupId, "Group"));

            if (userRelationList.Count > 0) await _userRelationService.Create(userRelationList); // 批量新增用户关系
            _db.CommitTran();
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw Oops.Oh(ErrorCode.D5001);
        }

        #region 第三方同步

        try
        {
            SysConfigOutput? sysConfig = await _sysConfigService.GetInfo();
            List<UserEntity>? userList = new List<UserEntity>();
            userList.Add(entity);
            if (sysConfig.dingSynIsSynUser)
                await _synThirdInfoService.SynUser(2, 3, sysConfig, userList);
            if (sysConfig.qyhIsSynUser)
                await _synThirdInfoService.SynUser(1, 3, sysConfig, userList);
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
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        UserEntity? entity = await _repository.FirstOrDefaultAsync(u => u.Id == id && u.DeleteMark == null);

        // 所属组织 分级权限验证
        List<string>? orgIdList = await _repository.Context.Queryable<UserRelationEntity>().Where(x => x.UserId == id && x.ObjectType == "Organize").Select(x => x.ObjectId).ToListAsync();
        if (!_userManager.DataScope.Any(it => orgIdList.Contains(it.organizeId) && it.Delete) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        if (await _organizeService.GetIsManagerByUserId(id))
            throw Oops.Oh(ErrorCode.D2003);
        _ = entity ?? throw Oops.Oh(ErrorCode.D5002);
        if (entity.IsAdministrator == (int)AccountType.Administrator)
            throw Oops.Oh(ErrorCode.D1014);
        if (entity.Id == _userManager.UserId)
            throw Oops.Oh(ErrorCode.D1001);
        entity.DeleteTime = DateTime.Now;
        entity.DeleteMark = 1;
        entity.DeleteUserId = _userManager.UserId;

        try
        {
            // 开启事务
            _db.BeginTran();

            // 用户软删除
            await _repository.Context.Updateable(entity).UpdateColumns(it => new { it.DeleteTime, it.DeleteMark, it.DeleteUserId }).ExecuteCommandAsync();

            // 直接删除用户关系表相关相关数据
            await _userRelationService.Delete(id);

            _db.CommitTran();
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw;
        }

        #region 第三方同步

        try
        {
            SysConfigOutput? sysConfig = await _sysConfigService.GetInfo();
            if (sysConfig.dingSynIsSynUser)
                await _synThirdInfoService.DelSynData(2, 3, sysConfig, id);
            if (sysConfig.qyhIsSynUser)
                await _synThirdInfoService.DelSynData(1, 3, sysConfig, id);
        }
        catch (Exception)
        {
        }

        #endregion
    }

    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] UserUpInput input)
    {
        UserEntity? oldUserEntity = await _repository.FirstOrDefaultAsync(it => it.Id == id);

        // 超级管理员 只有 sysadmin 账号才有变更权限
        if (_userManager.UserId != oldUserEntity.Id && oldUserEntity.IsAdministrator == 1 && _userManager.Account != CommonConst.SUPPER_ADMIN_ACCOUNT)
            throw Oops.Oh("只有超级管理员账号才有权限变更管理员信息");
        //throw Oops.Oh(ErrorCode.D1013);

        // 旧数据
        List<string>? orgIdList = await _repository.Context.Queryable<UserRelationEntity>().Where(x => x.UserId == id && x.ObjectType == "Organize").Select(x => x.ObjectId).ToListAsync();
        if (!_userManager.DataScope.Any(it => orgIdList.Contains(it.organizeId) && it.Edit) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        // 新数据
        var orgids = input.organizeId.Split(',');
        if (!_userManager.DataScope.Any(it => orgids.Contains(it.organizeId) && it.Edit) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        // 排除自己并且判断与其他是否相同
        if (await _repository.AnyAsync(u => u.Account == input.account && u.DeleteMark == null && u.Id != id)) throw Oops.Oh(ErrorCode.D1003);
        if (id == input.managerId) throw Oops.Oh(ErrorCode.D1021);

        // 直属主管的上级不能为自己的下属
        if (await GetIsMyStaff(id, input.managerId, 10)) throw Oops.Oh(ErrorCode.D1026);
        UserEntity? entity = input.Adapt<UserEntity>();
        entity.QuickQuery = PinyinHelper.PinyinString(input.realName);
        string? headIcon = input.headIcon.Split('/').ToList().Last();
        entity.HeadIcon = headIcon;
        entity.LastModifyTime = DateTime.Now;
        entity.LastModifyUserId = _userManager.UserId;
        if (entity.EnabledMark == 2) entity.UnLockTime = null;

        #region 多组织 优先选择有权限组织

        // 多 组织
        string[]? orgList = entity.OrganizeId.Split(",");
        entity.OrganizeId = string.Empty;

        if (orgList.Contains(oldUserEntity.OrganizeId))
        {
            List<string>? roleList = await _userManager.GetUserOrgRoleIds(entity.RoleId, oldUserEntity.OrganizeId);

            // 如果该组织下有角色并且有角色权限 则为默认组织
            if (roleList.Any() && _repository.Context.Queryable<AuthorizeEntity>().Where(x => x.ObjectType == "Role" && x.ItemType == "module" && roleList.Contains(x.ObjectId)).Any())
                entity.OrganizeId = oldUserEntity.OrganizeId; // 多 组织 默认
        }

        if (entity.OrganizeId.IsNullOrEmpty())
        {
            foreach (string? item in orgList)
            {
                List<string>? roleList = await _userManager.GetUserOrgRoleIds(entity.RoleId, item);

                // 如果该组织下有角色并且有角色权限 则为默认组织
                if (roleList.Any() && _repository.Context.Queryable<AuthorizeEntity>().Where(x => x.ObjectType == "Role" && x.ItemType == "module" && roleList.Contains(x.ObjectId)).Any())
                {
                    // 多 组织 默认
                    entity.OrganizeId = item;
                    break;
                }
            }
        }

        // 如果所选组织下都没有角色或者没有角色权限 默认取第一个
        if (entity.OrganizeId.IsNullOrEmpty()) entity.OrganizeId = input.organizeId.Split(",").FirstOrDefault();

        #endregion

        // 获取默认组织下的岗位
        string[]? positionIds = entity.PositionId?.Split(",");
        List<string>? pIdList = await _repository.Context.Queryable<PositionEntity>().Where(x => x.OrganizeId == entity.OrganizeId && positionIds.Contains(x.Id)).Select(x => x.Id).ToListAsync();

        if (entity.PositionId.IsNotEmptyOrNull() && pIdList.Contains(oldUserEntity.PositionId))
            entity.PositionId = oldUserEntity.PositionId;
        else entity.PositionId = pIdList.FirstOrDefault(); // 多 岗位 默认取第一个

        // 如果用户状态正常，重置错误次数
        if (entity.EnabledMark == 1)
        {
            entity.LogErrorCount = 0;
        }
        try
        {
            // 开启事务
            _db.BeginTran();

            // 更新用户记录
            int newEntity = await _repository.Context.Updateable(entity).UpdateColumns(it => new
            {
                it.Account,
                it.RealName,
                it.QuickQuery,
                it.Gender,
                it.Email,
                it.OrganizeId,
                it.ManagerId,
                it.PositionId,
                it.RoleId,
                it.SortCode,
                it.EnabledMark,
                it.Description,
                it.HeadIcon,
                it.Nation,
                it.NativePlace,
                it.EntryDate,
                it.CertificatesType,
                it.CertificatesNumber,
                it.Education,
                it.UrgentContacts,
                it.UrgentTelePhone,
                it.PostalAddress,
                it.MobilePhone,
                it.Birthday,
                it.TelePhone,
                it.Landline,
                it.UnLockTime,
                it.GroupId,
                it.LastModifyTime,
                it.LastModifyUserId,
                it.LogErrorCount
            }).ExecuteCommandAsync();

            // 将临时文件迁移至正式文件
            FileHelper.MoveFile(Path.Combine(FileVariable.TemporaryFilePath , headIcon), Path.Combine(FileVariable.UserAvatarFilePath , headIcon));

            // 获取当前关系
            var oldList = await _userRelationService.GetListByUserId(id);

            // 直接删除用户关系表相关相关数据
            await _userRelationService.Delete(id);

            List<UserRelationEntity>? userRelationList = oldList.Where(x => !new string[] { "Role", "Position", "Organize", "Group" }.Contains(x.ObjectType)).ToList(); // new List<UserRelationEntity>();
            userRelationList.AddRange(_userRelationService.CreateUserRelation(id, entity.RoleId, "Role"));
            userRelationList.AddRange(_userRelationService.CreateUserRelation(id, input.positionId, "Position"));
            userRelationList.AddRange(_userRelationService.CreateUserRelation(id, input.organizeId, "Organize"));
            userRelationList.AddRange(_userRelationService.CreateUserRelation(id, input.groupId, "Group"));
            if (userRelationList.Count > 0) await _userRelationService.Create(userRelationList); // 批量新增用户关系
            _db.CommitTran();

            // 如果是改成禁止状态，该用户立即退出登录
            if (oldUserEntity.EnabledMark != 0 && input.enabledMark == 0)
            {
                await Scoped.Create((_, scope) =>
                {
                    var services = scope.ServiceProvider;
                    var _onlineuser = App.GetService<OnlineUserService>(services);
                    _onlineuser.ForcedOffline(entity.Id);
                    return Task.CompletedTask;
                });
            }

            // 删除缓存
            await _cacheManager.DelAsync($"u:{entity.Id}:organize");
        }
        catch (Exception)
        {
            _db.RollbackTran();
            FileHelper.MoveFile(Path.Combine(FileVariable.UserAvatarFilePath , headIcon), Path.Combine(FileVariable.TemporaryFilePath , headIcon));
            throw Oops.Oh(ErrorCode.D5004);
        }

        #region 第三方同步

        try
        {
            SysConfigOutput? sysConfig = await _sysConfigService.GetInfo();
            List<UserEntity>? userList = new List<UserEntity>();
            userList.Add(entity);
            if (sysConfig.dingSynIsSynUser)
                await _synThirdInfoService.SynUser(2, 3, sysConfig, userList);
            if (sysConfig.qyhIsSynUser)
                await _synThirdInfoService.SynUser(1, 3, sysConfig, userList);
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
    [HttpPut("{id}/Actions/State")]
    public async Task UpdateState(string id)
    {
        UserEntity? entity = await _repository.FirstOrDefaultAsync(it => it.Id == id);
        if (!_userManager.DataScope.Any(it => it.organizeId == entity.OrganizeId && it.Edit == true) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);
        if (!await _repository.AnyAsync(u => u.Id == id && u.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D1002);
        int isOk = await _repository.Context.Updateable<UserEntity>().SetColumns(it => new UserEntity()
        {
            EnabledMark = SqlFunc.IIF(it.EnabledMark == 1, 0, 1),
            LastModifyUserId = _userManager.UserId,
            LastModifyTime = SqlFunc.GetDate()
        }).Where(it => it.Id == id).ExecuteCommandAsync();

        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D5005);
    }

    /// <summary>
    /// 重置密码.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("{id}/Actions/ResetPassword")]
    public async Task ResetPassword(string id, [FromBody] UserResetPasswordInput input)
    {
        UserEntity? entity = await _repository.FirstOrDefaultAsync(u => u.Id == id && u.DeleteMark == null);

        // 所属组织 分级权限验证
        List<string>? orgIdList = await _repository.Context.Queryable<UserRelationEntity>().Where(x => x.UserId == id && x.ObjectType == "Organize").Select(x => x.ObjectId).ToListAsync();
        if (!_userManager.DataScope.Any(it => orgIdList.Contains(it.organizeId) && it.Edit) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        if (!input.userPassword.Equals(input.validatePassword))
            throw Oops.Oh(ErrorCode.D5006);
        _ = entity ?? throw Oops.Oh(ErrorCode.D1002);

        string? password = MD5Encryption.Encrypt(input.userPassword + entity.Secretkey);

        int isOk = await _repository.Context.Updateable<UserEntity>().SetColumns(it => new UserEntity()
        {
            Password = password,
            ChangePasswordDate = SqlFunc.GetDate(),
            LastModifyUserId = _userManager.UserId,
            LastModifyTime = SqlFunc.GetDate()
        }).Where(it => it.Id == id).ExecuteCommandAsync();

        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D5005);

        // 强制将用户提掉线
    }

    /// <summary>
    /// 解除锁定.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/Unlock")]
    public async Task Unlock(string id)
    {
        UserEntity? entity = await _repository.FirstOrDefaultAsync(u => u.Id == id && u.DeleteMark == null);
        if (!_userManager.DataScope.Any(it => it.organizeId == entity.OrganizeId && it.Edit) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);
        int isOk = await _repository.Context.Updateable<UserEntity>().SetColumns(it => new UserEntity()
        {
            LockMark = 0, // 解锁
            LogErrorCount = 0, // 解锁
            EnabledMark = 1, // 解锁
            UnLockTime = DateTime.Now, // 取消解锁时间
            LastModifyUserId = _userManager.UserId,
            LastModifyTime = SqlFunc.GetDate()
        }).Where(it => it.Id == id).ExecuteCommandAsync();

        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D5005);
    }

    /// <summary>
    /// 导出Excel.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("ExportExcel")]
    public async Task<dynamic> ExportExcel([FromQuery] UserExportDataInput input)
    {
        // 用户信息列表
        List<UserListImportDataInput>? userList = new List<UserListImportDataInput>();

        ISugarQueryable<UserListImportDataInput>? sqlQuery = _repository.Context.Queryable<UserEntity, OrganizeEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == SqlFunc.ToString(a.OrganizeId)))
            .WhereIF(!string.IsNullOrWhiteSpace(input.organizeId), a => a.OrganizeId == input.organizeId) // 组织机构
            .WhereIF(!input.keyword.IsNullOrEmpty(), a => a.Account.Contains(input.keyword) || a.RealName.Contains(input.keyword))
            .Where(a => a.DeleteMark == null)
            .OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .Select(a => new UserListImportDataInput()
            {
                id = a.Id,
                account = a.Account,
                realName = a.RealName,
                birthday = SqlFunc.ToString(a.Birthday),
                certificatesNumber = a.CertificatesNumber,
                managerId = SqlFunc.Subqueryable<UserEntity>().Where(e => e.Id == a.ManagerId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
                organizeId = a.OrganizeId, // 组织结构
                positionId = a.PositionId, // 岗位
                roleId = a.RoleId, // 多角色
                certificatesType = SqlFunc.Subqueryable<DictionaryDataEntity>().Where(d => d.DictionaryTypeId == "7866376d5f694d4d851c7164bd00ebfc" && d.Id == a.CertificatesType).Select(b => b.FullName),
                education = SqlFunc.Subqueryable<DictionaryDataEntity>().Where(d => d.DictionaryTypeId == "6a6d6fb541b742fbae7e8888528baa16" && d.Id == a.Education).Select(b => b.FullName),
                gender = SqlFunc.Subqueryable<DictionaryDataEntity>().Where(d => d.DictionaryTypeId == "963255a34ea64a2584c5d1ba269c1fe6" && d.EnCode == SqlFunc.ToString(a.Gender)).Select(b => b.FullName),
                nation = SqlFunc.Subqueryable<DictionaryDataEntity>().Where(d => d.DictionaryTypeId == "b6cd65a763fa45eb9fe98e5057693e40" && d.Id == a.Nation).Select(b => b.FullName),
                description = a.Description,
                entryDate = SqlFunc.ToString(a.EntryDate),
                email = a.Email,
                enabledMark = SqlFunc.IF(a.EnabledMark.Equals(0)).Return("禁用").ElseIF(a.EnabledMark.Equals(1)).Return("正常").End("锁定"),
                mobilePhone = a.MobilePhone,
                nativePlace = a.NativePlace,
                postalAddress = a.PostalAddress,
                telePhone = a.TelePhone,
                urgentContacts = a.UrgentContacts,
                urgentTelePhone = a.UrgentTelePhone,
                landline = a.Landline,
                sortCode = a.SortCode.ToString()
            });

        if (input.dataType == "0") userList = await sqlQuery.ToPageListAsync(input.currentPage, input.pageSize);
        else userList = await sqlQuery.ToListAsync();

        userList.ForEach(item =>
        {
            if (item.birthday.IsNotEmptyOrNull()) item.birthday = Convert.ToDateTime(item.birthday).ToString("yyyy-MM-dd HH:mm:ss");
            if (item.entryDate.IsNotEmptyOrNull()) item.entryDate = Convert.ToDateTime(item.entryDate).ToString("yyyy-MM-dd HH:mm:ss");
        });

        List<OrganizeEntity>? olist = await _repository.Context.Queryable<OrganizeEntity>().Where(it => it.EnabledMark == 1 && it.DeleteMark == null).OrderBy(a => a.CreatorTime, OrderByType.Asc).ToListAsync(); // 获取所有组织
        List<PositionEntity>? plist = await _repository.Context.Queryable<PositionEntity>().Where(it => it.EnabledMark == 1 && it.DeleteMark == null).ToListAsync(); // 获取所有岗位
        List<RoleEntity>? rlist = await _repository.Context.Queryable<RoleEntity>().Where(it => it.EnabledMark == 1 && it.DeleteMark == null).ToListAsync(); // 获取所有角色

        // 转换 组织结构 和 岗位(多岗位)
        foreach (UserListImportDataInput? item in userList)
        {
            // 获取用户组织关联数据
            List<string>? orgRelList = await _repository.Context.Queryable<UserRelationEntity>().Where(x => x.ObjectType == "Organize" && x.UserId == item.id).Select(x => x.ObjectId).ToListAsync();

            if (orgRelList.Any())
            {
                List<OrganizeEntity>? oentityList = olist.Where(x => orgRelList.Contains(x.Id)).ToList();
                if (oentityList.Any())
                {
                    List<string>? userOrgList = new List<string>();
                    oentityList.ForEach(oentity =>
                    {
                        List<string>? oidList = oentity.OrganizeIdTree?.Split(',').ToList();
                        if (oidList != null)
                        {
                            List<string>? oNameList = olist.Where(x => oidList.Contains(x.Id)).Select(x => x.FullName).ToList();
                            userOrgList.Add(string.Join("/", oNameList));
                        }
                        else
                        {
                            List<string>? oNameList = new List<string>();
                            oNameList.Add(oentity.FullName);

                            // 递归获取上级组织
                            GetOrganizeParentName(olist, oentity.ParentId, oNameList);
                            userOrgList.Add(string.Join("/", oNameList));
                        }
                    });
                    item.organizeId = string.Join(";", userOrgList);
                }
            }
            else
            {
                item.organizeId = string.Empty;
            }

            // 获取用户岗位关联
            List<string>? posRelList = await _repository.Context.Queryable<UserRelationEntity>().Where(x => x.ObjectType == "Position" && x.UserId == item.id).Select(x => x.ObjectId).ToListAsync();
            if (posRelList.Any())
                item.positionId = string.Join(";", plist.Where(x => posRelList.Contains(x.Id)).Select(x => x.FullName + "/" + x.EnCode).ToList());
            else
                item.positionId = string.Empty;

            // 角色
            if (item.roleId.IsNotEmptyOrNull())
            {
                List<string>? ridList = item.roleId.Split(',').ToList();
                item.roleId = string.Join(";", rlist.Where(x => ridList.Contains(x.Id)).Select(x => x.FullName).ToList());
            }
        }

        ExcelConfig excelconfig = new ExcelConfig();
        excelconfig.FileName = string.Format("{0:yyyy-MM-dd}_用户信息.xls", DateTime.Now);
        excelconfig.HeadFont = "微软雅黑";
        excelconfig.HeadPoint = 10;
        excelconfig.IsAllSizeColumn = true;
        excelconfig.ColumnModel = new List<ExcelColumnModel>();
        foreach (KeyValuePair<string, string> item in GetUserInfoFieldToTitle(input.selectKey.Split(',').ToList()))
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath , excelconfig.FileName);
        var fs = ExcelExportHelper<UserListImportDataInput>.ExportMemoryStream(userList, excelconfig);
        var flag = await _fileManager.UploadFileByType(fs, FileVariable.TemporaryFilePath,excelconfig.FileName);
        if (flag.Item1)
        {
            fs.Flush();
            fs.Close();
        }

        return new { name = excelconfig.FileName, url = flag.Item2 ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "QT") };
    }

    /// <summary>
    /// 模板下载.
    /// </summary>
    /// <returns></returns>
    [HttpGet("TemplateDownload")]
    public dynamic TemplateDownload()
    {
        // 初始化 一条空数据 
        List<UserListImportDataInput>? dataList = new List<UserListImportDataInput>() { new UserListImportDataInput() { } };

        ExcelConfig excelconfig = new ExcelConfig();
        excelconfig.FileName = "用户信息导入模板.xls";
        excelconfig.HeadFont = "微软雅黑";
        excelconfig.HeadPoint = 10;
        excelconfig.IsAllSizeColumn = true;
        excelconfig.ColumnModel = new List<ExcelColumnModel>();
        foreach (KeyValuePair<string, string> item in GetUserInfoFieldToTitle())
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
        ExcelExportHelper<UserListImportDataInput>.Export(dataList, excelconfig, addPath);

        return new { name = excelconfig.FileName, url = "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "QT") };
    }

    /// <summary>
    /// 上传文件.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("Uploader")]
    public async Task<dynamic> Uploader(IFormFile file)
    {
        var _filePath = _fileManager.GetPathByType(string.Empty);
        var _fileName = DateTime.Now.ToString("yyyyMMdd") + "_" + SnowflakeIdHelper.NextId() + Path.GetExtension(file.FileName);
        var stream = file.OpenReadStream();
        var flag = await _fileManager.UploadFileByType(stream, _filePath, _fileName);
        return new { name = _fileName, url = flag.Item2 ?? string.Format("/api/File/Image/{0}/{1}", string.Empty, _fileName) };
    }

    /// <summary>
    /// 导入预览.
    /// </summary>
    /// <returns></returns>
    [HttpGet("ImportPreview")]
    public async Task<dynamic> ImportPreview(string fileName)
    {
        try
        {
            Dictionary<string, string>? FileEncode = GetUserInfoFieldToTitle();

            //string? filePath = FileVariable.TemporaryFilePath;
            //string? savePath = Path.Combine(filePath , fileName);

            //// 得到数据
            //global::System.Data.DataTable? excelData = ExcelImportHelper.ToDataTable(savePath);
            //foreach (object? item in excelData.Columns)
            //{
            //    excelData.Columns[item.ToString()].ColumnName = FileEncode.Where(x => x.Value == item.ToString()).FirstOrDefault().Key;
            //}

            //// 返回结果
            //return new { dataRow = excelData };

            string? filePath = Path.Combine(FileVariable.TemporaryFilePath, fileName.Replace("@", "."));
            using (var stream = (await _fileManager.DownloadFileByType(filePath, fileName))?.FileStream)
            {
                //var excelData1 = ExcelImportHelper.ToDataTable(stream,true);
                // 得到数据
                var excelData = ExcelImportHelper.ToDataTable(stream, ExcelImportHelper.IsXls(fileName));
                foreach (DataColumn item in excelData.Columns)
                {
                    excelData.Columns[item.ToString()].ColumnName = FileEncode.Where(x => x.Value == item.ToString()).FirstOrDefault().Key;
                }

                // 返回结果
                return new { dataRow = excelData };
            }
        }
        catch (Exception e)
        {
            throw Oops.Oh(ErrorCode.D1801);
        }
    }

    /// <summary>
    /// 导出错误报告.
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    [HttpPost("ExportExceptionData")]
    public async Task<dynamic> ExportExceptionData([FromBody] UserImportDataInput list)
    {
        object[]? res = await ImportUserData(list.list);

        // 错误数据
        List<UserListImportDataInput>? errorlist = res.Last() as List<UserListImportDataInput>;

        ExcelConfig excelconfig = new ExcelConfig();
        excelconfig.FileName = "用户导入错误报告.xls";
        excelconfig.HeadFont = "微软雅黑";
        excelconfig.HeadPoint = 10;
        excelconfig.IsAllSizeColumn = true;
        excelconfig.ColumnModel = new List<ExcelColumnModel>();
        foreach (KeyValuePair<string, string> item in GetUserInfoFieldToTitle())
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath , excelconfig.FileName);
        ExcelExportHelper<UserListImportDataInput>.Export(errorlist, excelconfig, addPath);

        return new { name = excelconfig.FileName, url = "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "QT") };
    }

    /// <summary>
    /// 导入数据.
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    [HttpPost("ImportData")]
    public async Task<dynamic> ImportData([FromBody] UserImportDataInput list)
    {
        object[]? res = await ImportUserData(list.list);
        List<UserEntity>? addlist = res.First() as List<UserEntity>;
        List<UserListImportDataInput>? errorlist = res.Last() as List<UserListImportDataInput>;
        return new UserImportResultOutput() { snum = addlist.Count, fnum = errorlist.Count, failResult = errorlist, resultType = errorlist.Count < 1 ? 0 : 1 };
    }

    /// <summary>
    /// 设置当前公司.
    /// </summary>
    /// <param name="id">公司id.</param>
    /// <returns></returns>
    [HttpPost("Company/{id}")]
    public async Task SetLoginCompany(string id)
    {
        OrganizeEntity? org = await _organizeService.GetInfoById(id) ?? throw Oops.Oh(ErrorCode.COM1005);
        string cacheKey = string.Format("{0}{1}_{2}", CommonConst.CACHEKEYONLINEUSER, _userManager.UserId, "company");
        await _cacheManager.SetAsync(cacheKey, org);
    }

    #endregion

    #region PublicMethod

    /// <summary>
    /// 获取用户信息 根据用户ID.
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns></returns>
    [NonAction]
    public UserEntity GetInfoByUserId(string userId)
    {
        return _repository.FirstOrDefault(u => u.Id == userId && u.DeleteMark == null);
    }

    /// <summary>
    /// 获取用户信息 根据用户ID.
    /// </summary>
    /// <param name="userId">用户ID.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<UserEntity> GetInfoByUserIdAsync(string userId)
    {
        return await _repository.FirstOrDefaultAsync(u => u.Id == userId && u.DeleteMark == null);
    }

    /// <summary>
    /// 获取用户列表.
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<List<UserEntity>> GetList()
    {
        return await _repository.AsQueryable().Where(u => u.DeleteMark == null).ToListAsync();
    }

    /// <summary>
    /// 获取用户信息 根据用户账户.
    /// </summary>
    /// <param name="account">用户账户.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<UserEntity> GetInfoByAccount(string account)
    {
        return await _repository.FirstOrDefaultAsync(u => u.Account == account && u.DeleteMark == null);
    }

    /// <summary>
    /// 获取用户信息 根据登录信息.
    /// </summary>
    /// <param name="account">用户账户.</param>
    /// <param name="password">用户密码.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<UserEntity> GetInfoByLogin(string account, string password)
    {
        return await _repository.FirstOrDefaultAsync(u => u.Account == account && u.Password == password && u.DeleteMark == null);
    }

    /// <summary>
    /// 根据用户姓名获取用户ID.
    /// </summary>
    /// <param name="realName">用户姓名.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<string> GetUserIdByRealName(string realName)
    {
        return (await _repository.FirstOrDefaultAsync(u => u.RealName == realName && u.DeleteMark == null)).Id;
    }

    /// <summary>
    /// 获取用户名.
    /// </summary>
    /// <param name="userId">用户id.</param>
    /// <param name="isAccount">是否显示账号.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<string> GetUserName(string userId, bool isAccount = true)
    {
        UserEntity? entity = await _repository.FirstOrDefaultAsync(x => x.Id == userId && x.DeleteMark == null);
        if (entity.IsNullOrEmpty()) return string.Empty;
        return isAccount ? entity.RealName + "/" + entity.Account : entity.RealName;
    }

    /// <summary>
    /// 获取用户名.
    /// </summary>
    /// <param name="entity">用户.</param>
    /// <param name="isAccount">是否显示账号.</param>
    /// <returns></returns>
    [NonAction]
    public string GetUserName(UserEntity entity, bool isAccount = true)
    {
        if (entity.IsNullOrEmpty()) return string.Empty;
        return isAccount ? entity.RealName + "/" + entity.Account : entity.RealName;
    }

    /// <summary>
    /// 获取当前用户岗位信息.
    /// </summary>
    /// <param name="PositionIds"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<PositionInfoModel>> GetPosition(string PositionIds)
    {
        string[]? ids = PositionIds.Split(",");
        return await _repository.Context.Queryable<PositionEntity>().In(it => it.Id, ids).Select(it => new PositionInfoModel { id = it.Id, name = it.FullName }).ToListAsync();
    }

    /// <summary>
    /// 表达式获取用户.
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<UserEntity> GetUserByExp(Expression<Func<UserEntity, bool>> expression)
    {
        return await _repository.FirstOrDefaultAsync(expression);
    }

    /// <summary>
    /// 表达式获取用户列表.
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<UserEntity>> GetUserListByExp(Expression<Func<UserEntity, bool>> expression)
    {
        return await _repository.AsQueryable().Where(expression).ToListAsync();
    }

    /// <summary>
    /// 表达式获取指定字段的用户列表.
    /// </summary>
    /// <param name="expression">where 条件表达式.</param>
    /// <param name="select">select 选择字段表达式.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<UserEntity>> GetUserListByExp(Expression<Func<UserEntity, bool>> expression, Expression<Func<UserEntity, UserEntity>> select)
    {
        return await _repository.AsQueryable().Where(expression).Select(select).ToListAsync();
    }


    /// <summary>
    /// 获取用户绑定的公司集合
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<OrganizeEntity>> GetRelationOrganizeList(string id)
    {
        List<UserRelationEntity>? allRelationList = await _userRelationService.GetListByUserId(id);
        var relationIds = allRelationList.Where(x => x.ObjectType == "Organize" || x.ObjectType == "Position").Select(x => new { x.ObjectId, x.ObjectType }).ToList();
        List<OrganizeEntity>? oList = await _repository.Context.Queryable<OrganizeEntity>().Where(x => relationIds.Where(x => x.ObjectType == "Organize").Select(x => x.ObjectId).Contains(x.Id)).ToListAsync();
        return oList;
    }

    #endregion

    #region PrivateMethod

    /// <summary>
    /// 获取集合中的组织 树,根据上级ID.
    /// </summary>
    /// <param name="list">组织 集合.</param>
    /// <param name="parentId">上级ID.</param>
    /// <param name="addList">返回.</param>
    /// <returns></returns>
    private List<string> GetOrganizeParentName(List<OrganizeEntity> list, string parentId, List<string> addList)
    {
        OrganizeEntity? entity = list.Find(x => x.Id == parentId);

        if (entity.ParentId != "-1") GetOrganizeParentName(list, entity.ParentId, addList);
        else addList.Add(entity.FullName);

        return addList;
    }

    /// <summary>
    /// 是否我的下属.
    /// </summary>
    /// <param name="userId">当前用户.</param>
    /// <param name="managerId">主管ID.</param>
    /// <param name="tier">层级.</param>
    /// <returns></returns>
    private async Task<bool> GetIsMyStaff(string userId, string managerId, int tier)
    {
        bool isMyStaff = false;
        if (tier <= 0) return true;
        string? superiorUserId = (await _repository.FirstOrDefaultAsync(it => it.Id.Equals(managerId) && it.DeleteMark == null))?.ManagerId;
        if (superiorUserId == null)
        {
            isMyStaff = false;
        }
        else if (userId == superiorUserId)
        {
            isMyStaff = true;
        }
        else
        {
            tier--;
            isMyStaff = await GetIsMyStaff(userId, superiorUserId, tier);
        }

        return isMyStaff;
    }

    /// <summary>
    /// 用户信息 字段对应 列名称.
    /// </summary>
    /// <returns></returns>
    private Dictionary<string, string> GetUserInfoFieldToTitle(List<string> fields = null)
    {
        Dictionary<string, string>? res = new Dictionary<string, string>();
        res.Add("account", "账户");
        res.Add("realName", "姓名");
        res.Add("gender", "性别");
        res.Add("email", "电子邮箱");
        res.Add("organizeId", "所属组织");
        res.Add("managerId", "直属主管");
        res.Add("positionId", "岗位");
        res.Add("roleId", "角色");
        res.Add("sortCode", "排序");
        res.Add("enabledMark", "状态");
        res.Add("description", "说明");
        res.Add("nation", "民族");
        res.Add("nativePlace", "籍贯");
        res.Add("entryDate", "入职时间");
        res.Add("certificatesType", "证件类型");
        res.Add("certificatesNumber", "证件号码");
        res.Add("education", "文化程度");
        res.Add("birthday", "出生年月");
        res.Add("telePhone", "办公电话");
        res.Add("landline", "办公座机");
        res.Add("mobilePhone", "手机号码");
        res.Add("urgentContacts", "紧急联系");
        res.Add("urgentTelePhone", "紧急电话");
        res.Add("postalAddress", "通讯地址");

        if (fields == null || !fields.Any()) return res;

        Dictionary<string, string>? result = new Dictionary<string, string>();

        foreach (KeyValuePair<string, string> item in res)
        {
            if (fields.Contains(item.Key))
                result.Add(item.Key, item.Value);
        }

        return result;
    }

    /// <summary>
    /// 导入用户数据函数.
    /// </summary>
    /// <param name="list">list.</param>
    /// <returns>[成功列表,失败列表].</returns>
    private async Task<object[]> ImportUserData(List<UserListImportDataInput> list)
    {
        List<UserListImportDataInput> userInputList = list;

        #region 初步排除错误数据

        if (userInputList == null || userInputList.Count() < 1)
            throw Oops.Oh(ErrorCode.D5019);

        // 必填字段验证 (账号，姓名，所属组织)
        List<UserListImportDataInput>? errorList = userInputList.Where(x => !x.account.IsNotEmptyOrNull() || !x.realName.IsNotEmptyOrNull() || !x.organizeId.IsNotEmptyOrNull()).ToList();

        // 上传重复的账号
        userInputList.ForEach(item =>
        {
            if (userInputList.Count(x => x.account == item.account) > 1)
            {
                var errorItems = userInputList.Where(x => x.account == item.account).ToList();
                errorItems.Remove(errorItems.First());
                errorList.AddRange(errorItems);
            }
        });

        errorList = errorList.Distinct().ToList();
        userInputList = userInputList.Except(errorList).ToList();

        // 用户账号 (匹配直属主管 和 验证重复账号)
        List<UserEntity>? _userRepositoryList = await _repository.AsQueryable().Where(it => it.DeleteMark == null).ToListAsync();

        // 已存在的账号
        List<UserEntity>? repeat = _userRepositoryList.Where(u => userInputList.Select(x => x.account).Contains(u.Account)).ToList();

        // 已存在的账号 列入 错误列表
        if (repeat.Any()) errorList.AddRange(userInputList.Where(u => repeat.Select(x => x.Account).Contains(u.account)));

        userInputList = userInputList.Except(errorList).ToList();

        #endregion

        List<UserEntity>? userList = new List<UserEntity>();

        #region 预处理关联表数据

        // 组织机构
        List<OrganizeEntity>? _organizeServiceList = await _organizeService.GetListAsync();
        Dictionary<string, string>? organizeDic = new Dictionary<string, string>();

        _organizeServiceList.ForEach(item =>
        {
            if (item.OrganizeIdTree.IsNullOrEmpty()) item.OrganizeIdTree = item.Id;
            var orgNameList = new List<string>();
            item.OrganizeIdTree.Split(",").ToList().ForEach(it =>
            {
                var org = _organizeServiceList.Find(x => x.Id == it);
                if (org != null) orgNameList.Add(org.FullName);
            });
            organizeDic.Add(item.Id, string.Join("/", orgNameList));
        });

        List<PositionEntity>? _positionRepositoryList = await _repository.Context.Queryable<PositionEntity>().Where(x => x.DeleteMark == null).ToListAsync(); // 岗位
        List<RoleEntity>? _roleRepositoryList = await _repository.Context.Queryable<RoleEntity>().Where(x => x.DeleteMark == null).ToListAsync(); // 角色

        DictionaryTypeEntity? typeEntity = await _repository.Context.Queryable<DictionaryTypeEntity>().Where(x => (x.Id == "963255a34ea64a2584c5d1ba269c1fe6" || x.EnCode == "963255a34ea64a2584c5d1ba269c1fe6") && x.DeleteMark == null).FirstAsync();
        List<DictionaryDataEntity>? _genderList = await _repository.Context.Queryable<DictionaryDataEntity>().Where(d => d.DictionaryTypeId == typeEntity.Id && d.DeleteMark == null).ToListAsync(); // 性别

        typeEntity = await _repository.Context.Queryable<DictionaryTypeEntity>().Where(x => (x.Id == "b6cd65a763fa45eb9fe98e5057693e40" || x.EnCode == "b6cd65a763fa45eb9fe98e5057693e40") && x.DeleteMark == null).FirstAsync();
        List<DictionaryDataEntity>? _nationList = await _repository.Context.Queryable<DictionaryDataEntity>().Where(d => d.DictionaryTypeId == typeEntity.Id && d.DeleteMark == null).ToListAsync(); // 民族

        typeEntity = await _repository.Context.Queryable<DictionaryTypeEntity>().Where(x => (x.Id == "7866376d5f694d4d851c7164bd00ebfc" || x.EnCode == "7866376d5f694d4d851c7164bd00ebfc") && x.DeleteMark == null).FirstAsync();
        List<DictionaryDataEntity>? _certificateTypeList = await _repository.Context.Queryable<DictionaryDataEntity>().Where(d => d.DictionaryTypeId == typeEntity.Id && d.DeleteMark == null).ToListAsync(); // 证件类型

        typeEntity = await _repository.Context.Queryable<DictionaryTypeEntity>().Where(x => (x.Id == "6a6d6fb541b742fbae7e8888528baa16" || x.EnCode == "6a6d6fb541b742fbae7e8888528baa16") && x.DeleteMark == null).FirstAsync();
        List<DictionaryDataEntity>? _educationList = await _repository.Context.Queryable<DictionaryDataEntity>().Where(d => d.DictionaryTypeId == typeEntity.Id && d.DeleteMark == null).ToListAsync(); // 文化程度

        #endregion

        // 用户关系数据
        List<UserRelationEntity>? userRelationList = new List<UserRelationEntity>();
        foreach (UserListImportDataInput? item in userInputList)
        {
            List<string>? orgIds = new List<string>(); // 多组织 , 号隔开
            List<string>? posIds = new List<string>(); // 多岗位 , 号隔开

            UserEntity? uentity = new UserEntity();
            uentity.Id = SnowflakeIdHelper.NextId();
            if (string.IsNullOrEmpty(uentity.HeadIcon)) uentity.HeadIcon = "001.png";
            uentity.Secretkey = Guid.NewGuid().ToString();
            uentity.Password = MD5Encryption.Encrypt(MD5Encryption.Encrypt(CommonConst.DEFAULTPASSWORD) + uentity.Secretkey); // 初始化密码
            uentity.ManagerId = _userRepositoryList.Find(x => x.Account == item.managerId?.Split('/').LastOrDefault())?.Id; // 寻找主管

            // 寻找角色
            if (item.roleId.IsNotEmptyOrNull() && item.roleId.Split(";").Any())
                uentity.RoleId = string.Join(",", _roleRepositoryList.Where(r => item.roleId.Split(";").Contains(r.FullName)).Select(x => x.Id).ToList());

            // 寻找组织
            string[]? userOidList = item.organizeId.Split(";");
            if (userOidList.Any())
            {
                foreach (string? oinfo in userOidList)
                {
                    if (organizeDic.ContainsValue(oinfo)) orgIds.Add(organizeDic.Where(x => x.Value == oinfo).FirstOrDefault().Key);
                }
            }
            else
            {
                // 如果未找到组织，列入错误列表
                errorList.Add(item);
                continue;
            }

            // 如果未找到组织，列入错误列表
            if (!orgIds.Any())
            {
                errorList.Add(item);
                continue;
            }

            // 寻找岗位
            item.positionId?.Split(';').ToList().ForEach(it =>
            {
                string[]? pinfo = it.Split("/");
                string? pid = _positionRepositoryList.Find(x => x.FullName == pinfo.FirstOrDefault() && x.EnCode == pinfo.LastOrDefault())?.Id;
                if (pid.IsNotEmptyOrNull()) posIds.Add(pid); // 多岗位
            });

            // 性别
            if (_genderList.Find(x => x.FullName == item.gender) != null) uentity.Gender = _genderList.Find(x => x.FullName == item.gender).EnCode.ParseToInt();
            else uentity.Gender = _genderList.Find(x => x.FullName == "保密").EnCode.ParseToInt();

            uentity.Nation = _nationList.Find(x => x.FullName == item.nation)?.Id; // 民族
            uentity.Education = _educationList.Find(x => x.FullName == item.education)?.Id; // 文化程度
            uentity.CertificatesType = _certificateTypeList.Find(x => x.FullName == item.certificatesType)?.Id; // 证件类型
            uentity.Account = item.account;
            uentity.Birthday = item.birthday.IsNotEmptyOrNull() ? item.birthday.ParseToDateTime() : null;
            uentity.CertificatesNumber = item.certificatesNumber;
            uentity.CreatorUserId = _userManager.UserId;
            uentity.CreatorTime = DateTime.Now;
            uentity.Description = item.description;
            uentity.Email = item.email;
            switch (item.enabledMark)
            {
                case "禁用":
                    uentity.EnabledMark = 0;
                    break;
                case "正常":
                    uentity.EnabledMark = 1;
                    break;
                case "锁定":
                default:
                    uentity.EnabledMark = 2;
                    break;
            }

            uentity.EntryDate = item.entryDate.IsNotEmptyOrNull() ? item.entryDate.ParseToDateTime() : null;
            uentity.Landline = item.landline;
            uentity.MobilePhone = item.mobilePhone;
            uentity.NativePlace = item.nativePlace;
            uentity.PostalAddress = item.postalAddress;
            uentity.RealName = item.realName;
            uentity.SortCode = item.sortCode.ParseToInt();
            uentity.TelePhone = item.telePhone;
            uentity.UrgentContacts = item.urgentContacts;
            uentity.UrgentTelePhone = item.urgentTelePhone;

            #region 多组织 优先选择有权限组织

            uentity.OrganizeId = string.Empty;

            foreach (string? it in orgIds)
            {
                List<string>? UserRoleList = await _userManager.GetUserOrgRoleIds(uentity.RoleId, it);

                // 如果该组织下有角色并且有角色权限 则为默认组织
                if (UserRoleList.Any() && _repository.Context.Queryable<AuthorizeEntity>().Where(x => x.ObjectType == "Role" && x.ItemType == "module" && UserRoleList.Contains(x.ObjectId)).Any())
                {
                    uentity.OrganizeId = it; // 多 组织 默认
                    break;
                }
            }

            if (uentity.OrganizeId.IsNullOrEmpty()) // 如果所选组织下都没有角色或者没有角色权限 默认取第一个
                uentity.OrganizeId = orgIds.FirstOrDefault();

            #endregion

            // 岗位多组织 匹配
            var opIds = await _repository.Context.Queryable<PositionEntity>().Where(x => x.DeleteMark == null && orgIds.Contains(x.OrganizeId)).Select(x => x.Id).ToListAsync();
            posIds = opIds.Intersect(posIds).ToList();

            if (uentity.OrganizeId.IsNotEmptyOrNull())
            {
                List<UserRelationEntity>? roleList = _userRelationService.CreateUserRelation(uentity.Id, uentity.RoleId, "Role"); // 角色关系
                List<UserRelationEntity>? positionList = _userRelationService.CreateUserRelation(uentity.Id, string.Join(",", posIds), "Position"); // 岗位关系
                List<UserRelationEntity>? organizeList = _userRelationService.CreateUserRelation(uentity.Id, string.Join(",", orgIds), "Organize"); // 组织关系
                userRelationList.AddRange(positionList);
                userRelationList.AddRange(roleList);
                userRelationList.AddRange(organizeList);
            }

            userList.Add(uentity);
        }

        if (userList.Any())
        {
            try
            {
                // 开启事务
                _db.BeginTran();

                // 新增用户记录
                UserEntity? newEntity = await _repository.Context.Insertable(userList).CallEntityMethod(m => m.Create()).ExecuteReturnEntityAsync();

                // 批量新增用户关系
                if (userRelationList.Count > 0) await _userRelationService.Create(userRelationList);

                _db.CommitTran();
            }
            catch (Exception)
            {
                _db.RollbackTran();
                errorList.AddRange(userInputList);
                userInputList = new List<UserListImportDataInput>();
            }
        }

        return new object[] { userList, errorList };
    }

    /// <summary>
    /// 递归排序 树形 List.
    /// </summary>
    /// <param name="list">.</param>
    /// <returns></returns>
    private List<UserSelectorOutput> OrderbyTree(List<UserSelectorOutput> list)
    {
        list.ForEach(item =>
        {
            var cList = item.children.ToObject<List<UserSelectorOutput>>();
            if (cList != null)
            {
                cList = cList.OrderBy(x => x.sortCode).ToList();
                item.children = cList.ToObject<List<object>>();
                if (cList.Any()) OrderbyTree(cList);
            }
        });

        return list;
    }

    /// <summary>
    /// 内部创建账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [NonAction]
    public async Task InnerCreate([FromBody] UserInCrInput input)
    {
        // 创建人，当前登录用户
        var creator = _userManager.User;
        //var orgids = input.organizeId.Split(',');
        //if (!_userManager.DataScope.Any(it => orgids.Contains(it.organizeId) && it.Add) && !_userManager.IsAdministrator)
        //    throw Oops.Oh(ErrorCode.D1013);
        if (await _repository.AnyAsync(u => u.Account == input.account && u.DeleteMark == null)) throw Oops.Oh(ErrorCode.D1003);
        UserEntity? entity = input.Adapt<UserEntity>();
        entity.Creator();
        if (!string.IsNullOrEmpty(input.id))
        {
            entity.Id = input.id;
        }
        #region 用户表单

        entity.IsAdministrator = 0;
        entity.EntryDate = DateTime.Now;
        entity.Birthday = DateTime.Now;
        entity.QuickQuery = PinyinHelper.PinyinString(input.realName);
        entity.Secretkey = Guid.NewGuid().ToString();
        entity.Password = MD5Encryption.Encrypt(MD5Encryption.Encrypt(input.password) + entity.Secretkey);
        //string? headIcon = input.headIcon.Split('/').ToList().Last();
        //if (string.IsNullOrEmpty(headIcon))
        //    headIcon = "001.png";
        entity.HeadIcon = creator?.HeadIcon ?? "001.png";
        if (string.IsNullOrEmpty(input.organizeId) && creator != null)
        {
            entity.OrganizeId = creator.OrganizeId;
        }
        if (!input.gender.HasValue)
        {
            entity.Gender = 3;
        }


        entity.Origin = input.origin ?? 0;

        //#region 多组织 优先选择有权限组织

        //// 多组织
        //string[]? orgList = entity.OrganizeId.Split(",");
        //entity.OrganizeId = string.Empty;

        //foreach (string? item in orgList)
        //{
        //    List<string>? roleList = await _userManager.GetUserOrgRoleIds(entity.RoleId, item);

        //    // 如果该组织下有角色并且有角色权限 则为默认组织
        //    if (roleList.Any() && _repository.Context.Queryable<AuthorizeEntity>().Where(x => x.ObjectType == "Role" && x.ItemType == "module" && roleList.Contains(x.ObjectId)).Any())
        //    {
        //        // 多 组织 默认
        //        entity.OrganizeId = item;
        //        break;
        //    }
        //}

        //// 如果所选组织下都没有角色或者没有角色权限 默认取第一个
        //if (entity.OrganizeId.IsNullOrEmpty()) entity.OrganizeId = input.organizeId.Split(",").FirstOrDefault();

        //#endregion

        //string[]? positionIds = entity.PositionId?.Split(",");
        //List<string>? pIdList = await _repository.Context.Queryable<PositionEntity>().Where(x => x.OrganizeId == entity.OrganizeId && positionIds.Contains(x.Id)).Select(x => x.Id).ToListAsync();
        //entity.PositionId = pIdList.FirstOrDefault(); // 多 岗位 默认取当前组织第一个

        #endregion

        try
        {
            // 开启事务
            _db.BeginTran();

            // 新增用户记录
            await _repository.Context.Insertable(entity).ExecuteCommandAsync();

            //// 将临时文件迁移至正式文件
            //FileHelper.MoveFile(Path.Combine(FileVariable.TemporaryFilePath, headIcon), Path.Combine(FileVariable.UserAvatarFilePath, headIcon));

            List<UserRelationEntity>? userRelationList = new List<UserRelationEntity>();
            userRelationList.AddRange(_userRelationService.CreateUserRelation(entity.Id, input.roleId, "Role"));
            if (!string.IsNullOrEmpty(input.positionId))
            {
                userRelationList.AddRange(_userRelationService.CreateUserRelation(entity.Id, input.positionId, "Position"));
            }           
            userRelationList.AddRange(_userRelationService.CreateUserRelation(entity.Id, entity.OrganizeId, "Organize"));
            //userRelationList.AddRange(_userRelationService.CreateUserRelation(entity.Id, input.groupId, "Group"));

            if (userRelationList.Count > 0) await _userRelationService.Create(userRelationList); // 批量新增用户关系
            _db.CommitTran();
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw Oops.Oh(ErrorCode.D5001);
        }

        #region 第三方同步

        try
        {
            SysConfigOutput? sysConfig = await _sysConfigService.GetInfo();
            List<UserEntity>? userList = new List<UserEntity>();
            userList.Add(entity);
            if (sysConfig.dingSynIsSynUser)
                await _synThirdInfoService.SynUser(2, 3, sysConfig, userList);
            if (sysConfig.qyhIsSynUser)
                await _synThirdInfoService.SynUser(1, 3, sysConfig, userList);
        }
        catch (Exception)
        {
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public async Task InnerCreateOrUpdate(UserInCrInput entity)
    {
        var user = await _repository.Context.Queryable<UserEntity>().ClearFilter().InSingleAsync(entity.id);
        //不存在则创建，存在则更新账号和密码以及用户名
        if (user == null)
        {
            // 创建客户账号
            await InnerCreate(entity);
        }
        else
        {
            List<UserRelationEntity>? userRelationList = new List<UserRelationEntity>();
            _repository.Context.Tracking(user);
            if (user.DeleteMark == 1)
            {
                user.DeleteMark = null;
                user.DeleteTime = null;
                user.DeleteUserId = string.Empty;

                userRelationList.AddRange(_userRelationService.CreateUserRelation(user.Id, user.RoleId, "Role"));
                userRelationList.AddRange(_userRelationService.CreateUserRelation(user.Id, user.OrganizeId, "Organize"));

                if (userRelationList.Count > 0) await _userRelationService.Create(userRelationList); // 批量新增用户关系
            }

            user.Account = entity.account;
            user.Password = MD5Encryption.Encrypt(MD5Encryption.Encrypt(entity.password) + user.Secretkey);
            user.RealName = entity.realName;
            user.MobilePhone = entity.mobilePhone;
            await _repository.Context.Updateable(user).ExecuteCommandAsync();
        }
       
    }

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

    internal class ErpCustomer
    {
        public string Id { get; set; }

        /// <summary>
        /// 客户账号
        /// </summary>
        public string F_LoginId { get; set; }

        /// <summary>
        /// 负责人电话（客户账号）
        /// </summary>
        public string F_Admintel { get; set; }
    }
    #endregion
}