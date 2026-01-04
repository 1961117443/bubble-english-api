using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Logistics.Entitys.Dto.LogMember;
using QT.Logistics.Entitys;
using QT.Logistics.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.DataEncryption;

namespace QT.Logistics;

/// <summary>
/// 业务实现：会员信息.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "会员信息管理", Name = "LogMember", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogMemberService : ILogMemberService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogMemberEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly ILogDeliveryPointService _logDeliveryPointService;

    /// <summary>
    /// 初始化一个<see cref="LogMemberService"/>类型的新实例.
    /// </summary>
    public LogMemberService(
        ISqlSugarRepository<LogMemberEntity> logMemberRepository,
        ISqlSugarClient context,
        IUserManager userManager,
        ILogDeliveryPointService logDeliveryPointService)
    {
        _repository = logMemberRepository;
        _userManager = userManager;
        _logDeliveryPointService = logDeliveryPointService;
    }

    /// <summary>
    /// 获取会员信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LogMemberInfoOutput>();
    }

    /// <summary>
    /// 获取会员信息列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogMemberListQueryInput input)
    {
        string pointId = string.Empty;
        if (input.scope == "point")
        {
            var point = await _logDeliveryPointService.GetCurrentUserPoint();
            pointId = point != null ? point.Id : $"NOTEXISTS_{_userManager.UserId}";
        }
        var data = await _repository.Context.Queryable<LogMemberEntity>()
            .WhereIF(!string.IsNullOrEmpty(pointId),it=> SqlFunc.Subqueryable<LogDeliveryMemberEntity>().Where(x=>x.MemberId == it.Id && x.PointId == pointId).Any())
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.cardNumber), it => it.CardNumber.Contains(input.cardNumber))
            .WhereIF(!string.IsNullOrEmpty(input.phoneNumber), it => it.PhoneNumber.Contains(input.phoneNumber))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                || it.CardNumber.Contains(input.keyword)
                || it.PhoneNumber.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogMemberListOutput
            {
                id = it.Id,
                name = it.Name,
                gender = it.Gender,
                cardNumber = it.CardNumber,
                birthDate = it.BirthDate,
                phoneNumber = it.PhoneNumber,
                //password = it.Password,
                email = it.Email,
                address = it.Address,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogMemberListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建会员信息.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogMemberCrInput input)
    {
        var entity = input.Adapt<LogMemberEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        if (!string.IsNullOrEmpty(input.password))
        {
            //密码md5加密
            entity.Password = MD5Encryption.Encrypt(input.password);
        }
        
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);

        // 判断当前用户是否绑定配送点
        var point = await _logDeliveryPointService.GetCurrentUserPoint();
        if (point !=null)
        {
            // 插入关联关系
            LogDeliveryMemberEntity logDeliveryMemberEntity = new LogDeliveryMemberEntity()
            {
                Id = SnowflakeIdHelper.NextId(),
                MemberId = entity.Id,
                PointId = point.Id
            };
            await _repository.Context.Insertable<LogDeliveryMemberEntity>(logDeliveryMemberEntity).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 更新会员信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogMemberUpInput input)
    {
        var entity = input.Adapt<LogMemberEntity>();
        //密码md5加密
        if (await _repository.AnyAsync(it=>it.Id == entity.Id && it.Password !=entity.Password))
        {
            if (!string.IsNullOrEmpty(input.password))
            {
                entity.Password = MD5Encryption.Encrypt(input.password);
            }
        }        
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除会员信息.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<LogMemberEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }


    /// <summary>
    /// 下拉选择
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/Selector")]
    public async Task<dynamic> Selector()
    {
        var data = await _repository.AsQueryable().Select(it => new { id = it.Id, name = it.Name }).ToListAsync();

        return new { list = data };
    }
}