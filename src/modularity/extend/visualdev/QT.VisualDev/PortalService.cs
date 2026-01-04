using QT.Common.Core.Manager;
using QT.Common.Core.Manager.Files;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Models.User;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.Common;
using QT.VisualDev.Entitys;
using QT.VisualDev.Entitys.Dto.Portal;
using QT.VisualDev.Entitys.Dto.VisualDev;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.VisualDev;

/// <summary>
///  业务实现：门户设计.
/// </summary>
[ApiDescriptionSettings(Tag = "VisualDev", Name = "Portal", Order = 173)]
[Route("api/visualdev/[controller]")]
public class PortalService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<PortalEntity> _portalRepository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 文件服务.
    /// </summary>
    private readonly IFileManager _fileManager;

    /// <summary>
    /// 初始化一个<see cref="PortalService"/>类型的新实例.
    /// </summary>
    public PortalService(
        ISqlSugarRepository<PortalEntity> portalRepository,
        IUserManager userManager,
        IFileManager fileManager)
    {
        _portalRepository = portalRepository;
        _userManager = userManager;
        _fileManager = fileManager;
    }

    #region Get

    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns>返回列表.</returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] VisualDevListQueryInput input)
    {
        SqlSugarPagedList<PortalListOutput>? portalList = await _portalRepository.Context.Queryable<PortalEntity, UserEntity, UserEntity, DictionaryDataEntity>((a, b, c, d) => new JoinQueryInfos(JoinType.Left, b.Id == a.CreatorUserId, JoinType.Left, c.Id == a.LastModifyUserId, JoinType.Left, d.Id == a.Category))
           .WhereIF(!string.IsNullOrEmpty(input.keyword), a => a.FullName.Contains(input.keyword) || a.EnCode.Contains(input.keyword))
           .WhereIF(!string.IsNullOrEmpty(input.category), a => a.Category == input.category).Where(a => a.DeleteMark == null)
           .OrderBy(a => a.SortCode, OrderByType.Asc)
           .OrderBy(a => a.CreatorTime, OrderByType.Desc)
           .OrderBy(a => a.LastModifyTime, OrderByType.Desc)
           .Select((a, b, c, d) => new PortalListOutput
           {
               id = a.Id,
               fullName = a.FullName,
               enCode = a.EnCode,
               deleteMark = SqlFunc.ToString(a.DeleteMark),
               description = a.Description,
               category = d.FullName,
               creatorTime = a.CreatorTime,
               creatorUser = SqlFunc.MergeString(b.RealName, "/", b.Account),
               parentId = a.Category,
               lastModifyUser = SqlFunc.MergeString(c.RealName, SqlFunc.IIF(c.RealName == null, string.Empty, "/"), c.Account),
               lastModifyTime = SqlFunc.ToDate(a.LastModifyTime),
               enabledMark = a.EnabledMark,
               type = a.Type,
               sortCode = SqlFunc.ToString(a.SortCode)
           })
           .ToPagedListAsync(input.currentPage, input.pageSize);

        return PageResult<PortalListOutput>.SqlSugarPageResult(portalList);
    }

    /// <summary>
    /// 获取门户侧边框列表.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<dynamic> GetSelector([FromQuery] string type)
    {
        UserInfoModel? userInfo = await _userManager.GetUserInfo();
        List<PortalSelectOutput>? data = new List<PortalSelectOutput>();
        if ("1".Equals(type) && !userInfo.isAdministrator)
        {
            List<string>? roleId = await _portalRepository.Context.Queryable<RoleEntity>().In(r => r.Id, userInfo.roleIds).Where(r => r.EnabledMark == 1 && r.DeleteMark == null).Select(r => r.Id).ToListAsync();
            var items = await _portalRepository.Context.Queryable<AuthorizeEntity>().Where(a => roleId.Contains(a.ObjectId) && a.ItemType == "portal").GroupBy(it => new { it.ItemId }).Select(it => new { it.ItemId }).ToListAsync();
            if (items.Any())
            {
                data = await _portalRepository.AsQueryable().In(p => p.Id, items.Select(it => it.ItemId).ToArray())
                    .Where(p => p.EnabledMark == 1 && p.DeleteMark == null).OrderBy(p => p.SortCode)
                    .Select(s => new PortalSelectOutput
                    {
                        id = s.Id,
                        fullName = s.FullName,
                        parentId = s.Category
                    }).ToListAsync();
            }
        }
        else
        {
            data = await _portalRepository.AsQueryable()
                .Where(p => p.EnabledMark == 1 && p.DeleteMark == null).OrderBy(o => o.SortCode)
                .Select(s => new PortalSelectOutput
                {
                    id = s.Id,
                    fullName = s.FullName,
                    parentId = s.Category,
                }).ToListAsync();
        }

        List<string>? parentIds = data.Select(it => it.parentId).Distinct().ToList();
        List<PortalSelectOutput>? treeList = new List<PortalSelectOutput>();
        if (parentIds.Any())
        {
            treeList = await _portalRepository.Context.Queryable<DictionaryDataEntity>().In(it => it.Id, parentIds.ToArray())
                .Where(d => d.DeleteMark == null && d.EnabledMark == 1).OrderBy(o => o.SortCode)
                .Select(d => new PortalSelectOutput
                {
                    id = d.Id,
                    parentId = "0",
                    fullName = d.FullName
                }).ToListAsync();
        }

        return new { list = treeList.Union(data).ToList().ToTree("0") };
    }

    /// <summary>
    /// 获取门户信息.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        PortalEntity? data = await _portalRepository.AsQueryable().FirstAsync(p => p.Id == id);
        return data.Adapt<PortalInfoOutput>();
    }

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/auth")]
    public async Task<dynamic> GetInfoAuth(string id)
    {
        UserInfoModel? userInfo = await _userManager.GetUserInfo();
        if (userInfo.roleIds != null && !userInfo.isAdministrator)
        {
            List<string>? roleId = await _portalRepository.Context.Queryable<RoleEntity>().Where(r => userInfo.roleIds.Contains(r.Id)).Where(r => r.EnabledMark == 1 && r.DeleteMark == null).Select(r => r.Id).ToListAsync();
            var items = await _portalRepository.Context.Queryable<AuthorizeEntity>().Where(a => roleId.Contains(a.ObjectId)).Where(a => a.ItemType == "portal").GroupBy(it => new { it.ItemId }).Select(it => new { it.ItemId }).ToListAsync();
            if (items.Count == 0) return null;
            PortalEntity? entity = await _portalRepository.AsQueryable().Where(p => items.Select(it => it.ItemId).Contains(p.Id)).SingleAsync(p => p.Id == id && p.EnabledMark == 1 && p.DeleteMark == null);
            _ = entity ?? throw Oops.Oh(ErrorCode.COM1005);
            return entity.Adapt<PortalInfoAuthOutput>();
        }

        if (userInfo.isAdministrator)
        {
            PortalEntity? entity = await _portalRepository.AsQueryable().FirstAsync(p => p.Id == id && p.EnabledMark == 1 && p.DeleteMark == null);
            _ = entity ?? throw Oops.Oh(ErrorCode.COM1005);
            return entity.Adapt<PortalInfoAuthOutput>();
        }

        throw Oops.Oh(ErrorCode.D1900);
    }

    #endregion

    #region Post

    /// <summary>
    /// 门户导出.
    /// </summary>
    /// <param name="modelId"></param>
    /// <returns></returns>
    [HttpPost("{modelId}/Actions/ExportData")]
    public async Task<dynamic> ActionsExportData(string modelId)
    {
        // 模板实体
        PortalEntity? templateEntity = await _portalRepository.AsQueryable().FirstAsync(p => p.Id == modelId);
        string? jsonStr = templateEntity.ToJsonString();
        return await _fileManager.Export(jsonStr, templateEntity.FullName, ExportFileType.vp);
    }

    /// <summary>
    /// 门户导入.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("Model/Actions/ImportData")]
    public async Task ActionsImportData(IFormFile file)
    {
        string? fileType = Path.GetExtension(file.FileName).Replace(".", string.Empty);
        if (!fileType.ToLower().Equals(ExportFileType.vp.ToString())) throw Oops.Oh(ErrorCode.D3006);
        string? josn = _fileManager.Import(file);
        PortalEntity? templateEntity = null;
        try
        {
            templateEntity = josn.ToObject<PortalEntity>();
        }
        catch
        {
            throw Oops.Oh(ErrorCode.D3006);
        }

        if (templateEntity == null) throw Oops.Oh(ErrorCode.D3006);
        if (templateEntity != null && templateEntity.FormData.IsNotEmptyOrNull() && templateEntity.FormData.IndexOf("layouyId") <= 0)
            throw Oops.Oh(ErrorCode.D3006);
        if (templateEntity.Id.IsNotEmptyOrNull() && await _portalRepository.AnyAsync(it => it.Id == templateEntity.Id && it.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D1400);
        if (await _portalRepository.AnyAsync(it => it.EnCode == templateEntity.EnCode && it.FullName == templateEntity.FullName && it.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D1400);

        StorageableResult<PortalEntity>? stor = _portalRepository.Context.Storageable(templateEntity).Saveable().ToStorage(); // 存在更新不存在插入 根据主键
        await stor.AsInsertable.ExecuteCommandAsync(); // 执行插入
        await stor.AsUpdateable.ExecuteCommandAsync(); // 执行更新
    }

    /// <summary>
    /// 新建门户信息.
    /// </summary>
    /// <param name="input">实体对象.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] PortalCrInput input)
    {
        PortalEntity? entity = input.Adapt<PortalEntity>();
        if (string.IsNullOrEmpty(entity.Category)) throw Oops.Oh(ErrorCode.D1901);
        else if (string.IsNullOrEmpty(entity.FullName)) throw Oops.Oh(ErrorCode.D1902);
        else if (string.IsNullOrEmpty(entity.EnCode)) throw Oops.Oh(ErrorCode.D1903);
        else await _portalRepository.Context.Insertable(entity).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
    }

    /// <summary>
    /// 修改接口.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] PortalUpInput input)
    {
        PortalEntity? entity = input.Adapt<PortalEntity>();
        int isOk = await _portalRepository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除接口.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        PortalEntity? entity = await _portalRepository.AsQueryable().FirstAsync(p => p.Id == id && p.DeleteMark == null);
        _ = entity ?? throw Oops.Oh(ErrorCode.COM1005);
        int isOk = await _portalRepository.Context.Updateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 复制.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpPost("{id}/Actions/Copy")]
    public async Task ActionsCopy(string id)
    {
        PortalEntity? newEntity = new PortalEntity();
        string? random = new Random().NextLetterAndNumberString(5);
        PortalEntity? entity = await _portalRepository.AsQueryable().FirstAsync(p => p.Id == id && p.DeleteMark == null);
        newEntity.FullName = entity.FullName + "副本" + random;
        newEntity.EnCode = entity.EnCode + random;
        newEntity.Category = entity.Category;
        newEntity.FormData = entity.FormData;
        newEntity.Description = entity.Description;
        newEntity.EnabledMark = 0;
        newEntity.SortCode = entity.SortCode;
        newEntity.Type = entity.Type;
        newEntity.LinkType = entity.LinkType;
        newEntity.CustomUrl = entity.CustomUrl;

        try
        {
            int isOk = await _portalRepository.Context.Insertable(newEntity).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        }
        catch
        {
            if (entity.FullName.Length >= 100 || entity.EnCode.Length >= 50) throw Oops.Oh(ErrorCode.D1403); // 数据长度超过 字段设定长度
            else throw;
        }
    }

    /// <summary>
    /// 设置默认门户.
    /// </summary>
    /// <returns></returns>
    [HttpPut("{id}/Actions/SetDefault")]
    public async Task SetDefault(string id)
    {
        UserEntity? userEntity = _userManager.User;
        _ = userEntity ?? throw Oops.Oh(ErrorCode.D5002);
        if (userEntity != null)
        {
            userEntity.PortalId = id;
            int isOk = await _portalRepository.Context.Updateable<UserEntity>().SetColumns(it => new UserEntity()
            {
                PortalId = id,
                LastModifyTime = SqlFunc.GetDate(),
                LastModifyUserId = _userManager.UserId
            }).Where(it => it.Id == userEntity.Id).ExecuteCommandAsync();
            if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D5014);
        }
    }

    #endregion

    #region PublicMethod

    /// <summary>
    /// 获取默认.
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<string> GetDefault()
    {
        UserEntity? user = _userManager.User;
        if (!user.IsAdministrator.ParseToBool())
        {
            if (!string.IsNullOrEmpty(user.RoleId))
            {
                string[]? roleIds = user.RoleId.Split(',');
                List<string>? roleId = await _portalRepository.Context.Queryable<RoleEntity>().Where(r => roleIds.Contains(r.Id)).Where(r => r.EnabledMark == 1 && r.DeleteMark == null).Select(r => r.Id).ToListAsync();
                var items = await _portalRepository.Context.Queryable<AuthorizeEntity>().Where(a => roleId.Contains(a.ObjectId)).Where(a => a.ItemType == "portal").GroupBy(it => new { it.ItemId }).Select(it => new { it.ItemId }).ToListAsync();
                if (items.Count == 0) return string.Empty;
                List<string>? portalList = await _portalRepository.AsQueryable().In(p => p.Id, items.Select(it => it.ItemId).ToArray()).Where(p => p.EnabledMark == 1 && p.DeleteMark == null).OrderBy(o => o.SortCode).Select(s => s.Id).ToListAsync();
                return portalList.FirstOrDefault();
            }

            return string.Empty;
        }
        else
        {
            List<string>? portalList = await _portalRepository.AsQueryable().Where(p => p.EnabledMark == 1 && p.DeleteMark == null).OrderBy(o => o.SortCode).Select(s => s.Id).ToListAsync();
            return portalList.FirstOrDefault();
        }
    }

    #endregion
}
