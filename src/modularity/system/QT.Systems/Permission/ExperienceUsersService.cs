using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QT.Common.Configuration;
using QT.Common.Const;
using QT.Common.Core.Filter;
using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Logging.Attributes;
using QT.Systems.Dto.Crm;
using QT.Systems.Entitys.Crm;
using QT.Systems.Entitys.Dto.Crm.ExperienceUsers;
using QT.Systems.Entitys.Dto.Permission.ExperienceUsers;
using QT.Systems.Entitys.Dto.SysConfig;
using QT.Systems.Entitys.Dto.User;
using QT.Systems.Entitys.Dto.UsersCurrent;
using QT.Systems.Entitys.Enum;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.Permission;
using SqlSugar;

namespace QT.Systems;

/// <summary>
///  业务实现：用户信息.
/// </summary>
[ApiDescriptionSettings(Tag = "Permission", Name = "ExperienceUsers", Order = 163)]
[Route("api/permission/[controller]")]
[ProhibitOperation(ProhibitOperationEnum.Allow)]
public class ExperienceUsersService:IDynamicApiController
{
    private readonly ISqlSugarRepository<UserEntity> _repository;
    private readonly IUserRelationService _userRelationService;
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="UsersService"/>类型的新实例.
    /// </summary>
    public ExperienceUsersService(
        ISqlSugarRepository<UserEntity> userRepository, IUserRelationService userRelationService,IUserManager userManager)
    {
        _repository = userRepository;
        _userRelationService = userRelationService;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ExperienceUserInfoOutput> GetInfo(string id, [FromServices] IUsersCurrentService _usersCurrentService)
    {
        UserEntity? entity = await _repository.FirstOrDefaultAsync(u => u.Id == id);
        SysConfigEntity? config = await _repository.Context.Queryable<SysConfigEntity>().Where(x => x.Key.Equals("lockType") && x.Category.Equals("SysConfig")).FirstAsync();
        string? configLockType = config?.Value;
        entity.EnabledMark = configLockType.IsNotEmptyOrNull() && configLockType == "2" && entity.EnabledMark == 2 && entity.UnLockTime < DateTime.Now ? 1 : entity.EnabledMark;
        ExperienceUserInfoOutput? output = entity.Adapt<ExperienceUserInfoOutput>();
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

            // 判断角色是否删除
            if (entity.RoleId.IsNotEmptyOrNull())
            {
                var roleIds = entity.RoleId.Split(",", true).Where(x => allRelationList.Any(a => a.ObjectType == "Role" && a.ObjectId == x)).ToList();

                output.roleId = string.Join(",", roleIds);
            }
        }

        output.userCommunicationList = await _repository.Context.Queryable<CrmUserCommunicationEntity>()
            .Where(w => w.UserId == output.id).OrderByDescending(w => w.CommunicationTime)
            .Select(w => new CrmUserCommunicationInfoOutput
            {
                managerIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == w.ManagerId).Select(ddd => ddd.RealName),
            }, true)
            .ToListAsync();

       output.loginLogData =  await _repository.Context.Queryable<SysLoginLog>()
            .Where(s => s.Category == 1 && s.UserId == output.id).OrderBy(o => o.CreatorTime, OrderByType.Desc)
            .Take(999)
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
            }).ToListAsync();

        output.miniProgramQRCode = await _usersCurrentService.GetMiniProgramQRCode(output.id);
        return output;
    }

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

        SqlSugarPagedList<ExperienceUserListOutput>? data = await _repository.Context.Queryable<UserEntity>()
                .WhereIF(!pageInput.keyword.IsNullOrEmpty(), a => a.Account.Contains(pageInput.keyword) || a.RealName.Contains(pageInput.keyword))
                //.WhereIF(input.hideCustomer, (a) => SqlFunc.Subqueryable<ErpCustomer>().AS("erp_customer").Where(xxx => (xxx.F_LoginId == a.Account || xxx.F_Admintel == a.Account)).NotAny())
                .Where(a => a.DeleteMark == null)
                //.WhereIF(input.origin.HasValue, (a) => a.Origin == input.origin)
                // 非超级管理员，不显示sysadmin账号
                .WhereIF(_userManager.Account != CommonConst.SUPPER_ADMIN_ACCOUNT, x => x.Account != CommonConst.SUPPER_ADMIN_ACCOUNT)
                .WhereIF(!_userManager.IsAdministrator, a=>  a.Sid == _userManager.UserId)
                .OrderBy(a => a.SortCode).OrderBy(a => a.LastLogTime, OrderByType.Desc)
                .Select(a => new ExperienceUserListOutput
                {
                    id = a.Id,
                    account = a.Account,
                    realName = a.RealName,
                    creatorTime = a.CreatorTime,
                    gender = a.Gender,
                    mobilePhone = a.MobilePhone,
                    sortCode = a.SortCode,
                    enabledMark = SqlFunc.IIF(configLockType == ErrorStrategy.Delay && a.EnabledMark == 2 && a.UnLockTime < DateTime.Now, 1, a.EnabledMark),
                    expireTime = a.ExpireTime,
                    sid = a.Sid,
                    managerUserIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == a.Sid).Select(ddd => ddd.RealName),
                    lastLogTime = a.LastLogTime
                })
                .ToPagedListAsync(input.currentPage, input.pageSize);

        return PageResult<ExperienceUserListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 获取列表树.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("tree/List")]
    public async Task<dynamic> GetTreeList([FromQuery] UserListQuery input)
    {
        PageInputBase? pageInput = input.Adapt<PageInputBase>();

        List<string> childIdList = new List<string>();
        if (!_userManager.IsAdministrator)
        {
            //根据当前账号找出所有下级
            var childList = await _repository.Context.Queryable<UserEntity>().Select(x => new UserEntity
            {
                Id = x.Id,
                Sid = x.Sid
            }).ToChildListAsync(x => x.Sid, _userManager.UserId,false);

            childIdList = childList.Select(x => x.Id).ToList();
        }

        // 获取配置文件 账号锁定类型
        SysConfigEntity? config = await _repository.Context.Queryable<SysConfigEntity>().Where(x => x.Key.Equals("lockType") && x.Category.Equals("SysConfig")).FirstAsync();
        ErrorStrategy configLockType = (ErrorStrategy)Enum.Parse(typeof(ErrorStrategy), config?.Value);

        var data = await _repository.Context.Queryable<UserEntity>()
                //.WhereIF(!pageInput.keyword.IsNullOrEmpty(), a => a.Account.Contains(pageInput.keyword) || a.RealName.Contains(pageInput.keyword))
                .Where(a => a.DeleteMark == null)
                //.WhereIF(input.origin.HasValue, (a) => a.Origin == input.origin)
                // 非超级管理员，不显示sysadmin账号
                .WhereIF(_userManager.Account != CommonConst.SUPPER_ADMIN_ACCOUNT, x => x.Account != CommonConst.SUPPER_ADMIN_ACCOUNT)
                .WhereIF(!_userManager.IsAdministrator, a => a.Sid == _userManager.UserId || childIdList.Contains(a.Id))
                .OrderBy(a => a.SortCode).OrderBy(a => a.LastLogTime, OrderByType.Desc)
                .Select(a => new ExperienceUserTreeListOutput
                {
                    id = a.Id,
                    account = a.Account ?? "",
                    realName = a.RealName ?? "",
                    creatorTime = a.CreatorTime,
                    gender = a.Gender,
                    mobilePhone = a.MobilePhone,
                    sortCode = a.SortCode,
                    enabledMark = SqlFunc.IIF(configLockType == ErrorStrategy.Delay && a.EnabledMark == 2 && a.UnLockTime < DateTime.Now, 1, a.EnabledMark),
                    expireTime = a.ExpireTime,
                    sid = SqlFunc.IsNull(a.Sid, ""),
                    managerUserIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == a.Sid).Select(ddd => ddd.RealName) ?? "",
                    lastLogTime = a.LastLogTime
                })
                .ToListAsync();

        // 整理sid
        data.ForEach(x =>
        {
            if (x.sid.IsNotEmptyOrNull() && !data.Any(u => u.id == x.sid))
            {
                x.sid = "";
            }
        });

        //.ToPagedListAsync(input.currentPage, input.pageSize);
        //var tree = data.ToList();
        if (input.keyword.IsNotEmptyOrNull())
        {
            data = data.TreeWhere(x => x.account.Contains(input.keyword) || x.realName.Contains(input.keyword) || x.managerUserIdName.Contains(input.keyword), x => x.id, x => x.sid);
        }
        var parentId = _userManager.IsAdministrator ? "" : _userManager.UserId;
        //var data = data.ToTree(x => x.id, parentId);
        //return PageResult<ExperienceUserListOutput>.SqlSugarPageResult(data);
        return new { list = data.ToTree(x => x.id, parentId) };
    }

    /// <summary>
    /// 获取下拉框.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="category">菜单分类（参数有Web,App），默认显示所有分类.</param>
    /// <returns></returns>
    [HttpGet("selector/{id}")]
    public async Task<dynamic> GetSelector(string id)
    {
        var treeList = await _repository.AsQueryable()
            .Select((a) => new ExperienceUserTreeListOutput
            {
                id = a.Id,
                realName = a.RealName,
                sid = a.Sid
            })
            .ToListAsync();

       
        // 整理sid
        treeList.ForEach(x =>
        {
            if (x.sid.IsNotEmptyOrNull() && !treeList.Any(u => u.id == x.sid))
            {
                x.sid = "";
            }
        });

        if (!id.Equals("0"))
            treeList.RemoveAll(x => x.id == id);


        return new { list = treeList.ToTree(x=>x.id, "") };
    }

    #region 体验用户操作
    /// <summary>
    /// 更新用户的过期时间
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPut("Actions/{id}/ExpiredTime")]
    [OperateLog("体验用户", "更改过期时间")]
    public async Task SetExpiredTime(string id, [FromBody] ExperienceUserUpExpirationTime input)
    {
        var user = await _repository.Context.Queryable<UserEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        user.ExpireTime = input.expireTime;

        await _repository.Context.Updateable<UserEntity>(user).UpdateColumns(x => x.ExpireTime).ExecuteCommandAsync();
    }

    /// <summary>
    /// 更新用户的业务经理
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPut("Actions/{id}/Manager")]
    [OperateLog("体验用户", "更改业务经理")]
    public async Task SetManager(string id, [FromBody] ExperienceUserListOutput input)
    {
        var user = await _repository.Context.Queryable<UserEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        if (user.Id == input.sid)
        {
            throw Oops.Oh("业务经理不能选择自己！");
        }

        user.Sid = input.sid;

        await _repository.Context.Updateable<UserEntity>(user).UpdateColumns(x => x.Sid).ExecuteCommandAsync();
    }
    #endregion

    #region 沟通记录

    /// <summary>
    /// 添加沟通记录
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("Actions/{id}/Communication")]
    public async Task JzrcCompanyCommunication(string id, [FromBody] CrmUserCommunicationCrInput input)
    {
        var user = await _repository.AsQueryable().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);
        //// 当前用户非客户经理，禁止添加
        //if (company.ManagerId != _userManager.UserId)
        //{
        //    throw Oops.Oh(ErrorCode.D1013);
        //}

        var entity = input.Adapt<CrmUserCommunicationEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.ManagerId = _userManager.UserId;
        entity.UserId = id;
        if (!entity.CommunicationTime.HasValue)
        {
            entity.CommunicationTime = DateTime.Now;
        }

        await _repository.Context.Insertable<CrmUserCommunicationEntity>(entity).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取公司的沟通记录
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("Actions/{id}/Communication")]
    public async Task<List<CrmUserCommunicationInfoOutput>> CrmUserCommunicationList(string id)
    {
        var jzrcCompanyCommunicationList = await _repository.Context.Queryable<CrmUserCommunicationEntity>().Where(w => w.UserId == id)
            .OrderByDescending(w => w.CommunicationTime).ToListAsync();
        return jzrcCompanyCommunicationList.Adapt<List<CrmUserCommunicationInfoOutput>>();
    }
    #endregion

    /// <summary>
    /// 账号申请延期
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("Actions/{id}/AccountDelay")]
    [AllowAnonymous]
    public async Task AccountDelay(string id)
    {
        // 判断账号是否已存在申请
        if (!await _repository.Context.Queryable<CrmUserDelayApplyEntity>().AnyAsync(x => x.UserId == id && x.Status == 0))
        {
            CrmUserDelayApplyEntity crmUserDelayApplyEntity = new CrmUserDelayApplyEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                CreatorUserId = id,
                Status = 0,
                UserId = id
            };
            await _repository.Context.Insertable<CrmUserDelayApplyEntity>(crmUserDelayApplyEntity).ExecuteCommandAsync();
        }
    }


    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Update(string id, [FromBody] ExperienceUserUpInput input)
    {
        UserEntity? oldUserEntity = await _repository.FirstOrDefaultAsync(it => it.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);
 
        _repository.Context.Tracking(oldUserEntity);

        input.Adapt(oldUserEntity);

        await _repository.Context.Updateable(oldUserEntity).ExecuteCommandAsync();
    }
}
