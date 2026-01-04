using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Logistics.Entitys.Dto.LogDeliveryStoreroom;
using QT.Logistics.Entitys;
using QT.Logistics.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Logistics.Entitys.Dto.LogStoreroom;
using Spire.Pdf.Lists;
using QT.Systems.Entitys.Permission;

namespace QT.Logistics;

/// <summary>
/// 业务实现：配送点仓库.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "配送点仓库管理", Name = "LogDeliveryStoreroom", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogDeliveryStoreroomService : ILogDeliveryStoreroomService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogDeliveryStoreroomEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="LogDeliveryStoreroomService"/>类型的新实例.
    /// </summary>
    public LogDeliveryStoreroomService(
        ISqlSugarRepository<LogDeliveryStoreroomEntity> logDeliveryStoreroomRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = logDeliveryStoreroomRepository;
        _userManager = userManager;

        
    }

    #region 增删改查
    /// <summary>
    /// 获取配送点仓库.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var point = await GetUserPoint();
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id && x.PointId == point.Id)).Adapt<LogDeliveryStoreroomInfoOutput>();
    }

    /// <summary>
    /// 获取配送点仓库列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogDeliveryStoreroomListQueryInput input)
    {
        var point = await GetUserPoint();
        var data = await _repository.Context.Queryable<LogDeliveryStoreroomEntity>()
            .Where(it=>it.PointId == point.Id)
            .WhereIF(!string.IsNullOrEmpty(input.pid), it => it.PId == input.pid)
            .WhereIF(input.category.HasValue,it=>it.Category == input.category)
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.code), it => it.Code.Contains(input.code))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                || it.Code.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogDeliveryStoreroomListOutput
            {
                id = it.Id,
                name = it.Name,
                code = it.Code,
                description = it.Description,
                category = it.Category ?? -1,
                pidName = SqlFunc.Subqueryable<LogDeliveryStoreroomEntity>().Where(x=>x.Id == it.PId).Select(x=>x.Name)
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogDeliveryStoreroomListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建配送点仓库.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogDeliveryStoreroomCrInput input)
    {
        var entity = input.Adapt<LogDeliveryStoreroomEntity>();
        var point = await GetUserPoint();
        entity.PointId = point.Id;
        // 同一层级不允许重复
        if (await _repository.Where(it => it.PointId == entity.PointId && it.PId == entity.PId && it.Category == entity.Category && it.Code == entity.Code).AnyAsync())
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }
        if (await _repository.Where(it => it.PointId == entity.PointId && it.PId == entity.PId && it.Category == entity.Category && it.Name == entity.Name).AnyAsync())
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }
        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新配送点仓库.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogDeliveryStoreroomUpInput input)
    {
        var entity = await _repository.AsQueryable().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);        
        _repository.Context.Tracking(entity);
        input.Adapt(entity);
        // 同一层级不允许重复
        if (await _repository.Where(it => it.PointId == entity.PointId && it.PId == entity.PId && it.Category == entity.Category && it.Code == entity.Code && it.Id != entity.Id).AnyAsync())
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }
        if (await _repository.Where(it => it.PointId == entity.PointId && it.PId == entity.PId && it.Category == entity.Category && it.Name == entity.Name && it.Id != entity.Id).AnyAsync())
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除配送点仓库.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        // 判断是否有下级
        if (await _repository.Where(it => it.PId == id).AnyAsync())
        {
            throw Oops.Oh(ErrorCode.D1007);
        }
        var isOk = await _repository.Context.Deleteable<LogDeliveryStoreroomEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
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
        var point = await GetUserPoint();
        var data = await _repository.Context.Queryable<LogDeliveryStoreroomEntity>()
            .Where(it=>it.PointId == point.Id)
            .Where(it => it.Category == category).ToListAsync();

        var treeList = data.Adapt<List<LogStoreroomTreeListOutput>>();

        return new { list = treeList };
    }

    /// <summary>
    /// 下拉选择 属性选择.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/Selector/Tree")]
    public async Task<dynamic> TreeSelector()
    {
        var point = await GetUserPoint();
        var data = await _repository.Context.Queryable<LogDeliveryStoreroomEntity>()
            .Where(it => it.PointId == point.Id)
            .ToListAsync();

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

        return new { list = data.Adapt<List<LogStoreroomTreeListOutput>>().ToTree("0") };
    }

    /// <summary>
    /// 下拉选择 属性选择.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/TreeData")]
    public async Task<dynamic> TreeData()
    {
        var point = await GetUserPoint();
        var data = await _repository.Context.Queryable<LogDeliveryStoreroomEntity>()
            .Where(it=> it.PointId == point.Id)
            .Where(it => it.Category != 3)
            .ToListAsync();

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

        return new { list = data.Adapt<List<LogStoreroomTreeListOutput>>().ToTree("0") };
    }

    #region 私有方法
    /// <summary>
    /// 获取当前用户绑定的配送点
    /// </summary>
    /// <returns></returns>
    private async Task<LogDeliveryPointEntity> GetUserPoint()
    {
        //当前用户绑定的配送点
        var entity = await _repository.Context.Queryable<LogDeliveryPointEntity>().Where(it => it.AdminId == _userManager.UserId).FirstAsync();

        if (entity == null)
        {
            // 查找账号的直属主管
            //_userManager.User.ManagerId
            var list = await _repository.Context.Queryable<UserEntity>().ToParentListAsync(x => x.ManagerId, _userManager.UserId);
            if (list.IsAny())
            {
                var idList = list.Select(x => x.Id).ToArray();
                entity = await _repository.Context.Queryable<LogDeliveryPointEntity>().Where(it => idList.Contains(it.AdminId)).FirstAsync();
            }
        }


        if (entity!=null)
        {
            return entity;
        }     

            
        throw Oops.Oh("当前用户未绑定配送点！");
    } 
    #endregion
}