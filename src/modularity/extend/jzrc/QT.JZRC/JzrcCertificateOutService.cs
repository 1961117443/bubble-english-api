using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.JZRC.Entitys.Dto.JzrcCertificateOut;
using QT.JZRC.Entitys;
using QT.JZRC.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.JZRC;

/// <summary>
/// 业务实现：建筑人才档案寄件.
/// </summary>
[ApiDescriptionSettings(Tag = "JZRC", Name = "JzrcCertificateOut", Order = 200)]
[Route("api/JZRC/[controller]")]
public class JzrcCertificateOutService : IJzrcCertificateOutService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<JzrcCertificateOutEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="JzrcCertificateOutService"/>类型的新实例.
    /// </summary>
    public JzrcCertificateOutService(
        ISqlSugarRepository<JzrcCertificateOutEntity> jzrcCertificateOutRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = jzrcCertificateOutRepository;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取建筑人才档案寄件.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<JzrcCertificateOutInfoOutput>();
    }

    /// <summary>
    /// 获取建筑人才档案寄件列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] JzrcCertificateOutListQueryInput input)
    {
        List<DateTime> queryOutTime = input.outTime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startOutTime = queryOutTime?.First();
        DateTime? endOutTime = queryOutTime?.Last();
        var data = await _repository.Context.Queryable<JzrcCertificateOutEntity>()
            .WhereIF(queryOutTime != null, it => SqlFunc.Between(it.OutTime, startOutTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endOutTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.expressNo), it => it.ExpressNo.Contains(input.expressNo))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.ExpressNo.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new JzrcCertificateOutListOutput
            {
                id = it.Id,
                certificateId = it.CertificateId,
                outTime = it.OutTime,
                expressNo = it.ExpressNo,
                remark = it.Remark,
                certificateIdName = SqlFunc.Subqueryable<JzrcTalentCertificateEntity>().Where(ddd => ddd.Id == it.CertificateId).Select(ddd => ddd.CertificateName),
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<JzrcCertificateOutListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建建筑人才档案寄件.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] JzrcCertificateOutCrInput input)
    {
        var entity = input.Adapt<JzrcCertificateOutEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新建筑人才档案寄件.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] JzrcCertificateOutUpInput input)
    {
        var entity = input.Adapt<JzrcCertificateOutEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除建筑人才档案寄件.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<JzrcCertificateOutEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
}