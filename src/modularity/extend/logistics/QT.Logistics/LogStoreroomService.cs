using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Logistics.Entitys.Dto.LogStoreroom;
using QT.Logistics.Entitys;
using QT.Logistics.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Systems.Entitys.Dto.Module;

namespace QT.Logistics;

/// <summary>
/// 业务实现：仓库信息.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "仓库管理", Name = "LogStoreroom", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogStoreroomService : ILogStoreroomService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogStoreroomEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="LogStoreroomService"/>类型的新实例.
    /// </summary>
    public LogStoreroomService(
        ISqlSugarRepository<LogStoreroomEntity> logStoreroomRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = logStoreroomRepository;
        _userManager = userManager;
    }
    #region 增删改查

    /// <summary>
    /// 获取仓库信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LogStoreroomInfoOutput>();
    }

    /// <summary>
    /// 获取仓库信息列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogStoreroomListQueryInput input)
    {
        var data = await _repository.Context.Queryable<LogStoreroomEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.pid), it => it.PId == input.pid)
            .WhereIF(input.category.HasValue, it => it.Category == input.category)
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.code), it => it.Code.Contains(input.code))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                || it.Code.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogStoreroomListOutput
            {
                id = it.Id,
                name = it.Name,
                address = it.Address,
                description = it.Description,
                adminId = it.AdminId,
                adminTel = it.AdminTel,
                area = it.Area,
                code = it.Code,
                status = it.Status ?? 0,
                category = it.Category ?? -1,
                pidName = SqlFunc.Subqueryable<LogStoreroomEntity>().Where(x => x.Id == it.PId).Select(x => x.Name)
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogStoreroomListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建仓库信息.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogStoreroomCrInput input)
    {
        var entity = input.Adapt<LogStoreroomEntity>();

        if (await _repository.Where(it => it.Category == entity.Category && it.Code == entity.Code).AnyAsync())
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }
        if (await _repository.Where(it => it.Category == entity.Category && it.Name == entity.Name).AnyAsync())
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }


        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新仓库信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogStoreroomUpInput input)
    {
        var entity = input.Adapt<LogStoreroomEntity>();

        if (await _repository.Where(it => it.Category == entity.Category && it.Code == entity.Code && it.Id !=entity.Id).AnyAsync())
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }
        if (await _repository.Where(it => it.Category == entity.Category && it.Name == entity.Name && it.Id != entity.Id).AnyAsync())
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }

        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除仓库信息.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        // 判断是否有下级
        if (await _repository.Context.Queryable<LogStoreroomEntity>().Where(it => it.PId == id).AnyAsync())
        {
            throw Oops.Oh(ErrorCode.D1007);
        }
        var entity = await _repository.Context.Queryable<LogStoreroomEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);
        var isOk = await _repository.Context.Deleteable<LogStoreroomEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }


    #endregion

    /// <summary>
    /// 下拉选择
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/Selector/{category}")]
    public async Task<dynamic> Selector(int category)
    {
        var data = await _repository.Context.Queryable<LogStoreroomEntity>().Where(it => it.Category == category && it.Status == 1).ToListAsync();

        var treeList = data.Adapt<List<LogStoreroomTreeListOutput>>();

        return new { list = treeList };
    }

    /// <summary>
    /// 下拉选择 树形下拉
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/Selector/Tree")]
    public async Task<dynamic> TreeSelector()
    {
        var data = await _repository.Context.Queryable<LogStoreroomEntity>().Where(it => it.Status == 1).ToListAsync();

        data.ForEach(x =>
        {
            string prefix = string.Empty;
            switch (x.Category)
            {
                case 0:
                    prefix = "【仓库】";
                    break;
                case 1:
                    prefix = "【库区】";
                    break;
                case 2:
                    prefix = "【货柜】";
                    break;
                    case 3:
                    prefix = "【柜层】";
                    break;
                default:
                    break;
            }
            x.Name = $"{prefix}{x.Name}";
        });

        var treeList = data.Adapt<List<LogStoreroomTreeListOutput>>();

        return new { list = treeList.ToTree("0") };
    }

    /// <summary>
    /// 下拉选择 树形下拉
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/TreeData")]
    public async Task<dynamic> TreeData()
    {
        var data = await _repository.Context.Queryable<LogStoreroomEntity>()
            .Where(it => it.Status == 1 && it.Category !=3).ToListAsync();

        data.ForEach(x =>
        {
            string prefix = string.Empty;
            switch (x.Category)
            {
                case 0:
                    prefix = "【仓库】";
                    break;
                case 1:
                    prefix = "【库区】";
                    break;
                case 2:
                    prefix = "【货柜】";
                    break;
                case 3:
                    prefix = "【柜层】";
                    break;
                default:
                    break;
            }
            x.Name = $"{prefix}{x.Name}";
        });

        var treeList = data.Adapt<List<LogStoreroomTreeListOutput>>();

        return new { list = treeList.ToTree("0") };
    }
}