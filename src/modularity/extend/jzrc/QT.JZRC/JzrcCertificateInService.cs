using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.JZRC.Entitys.Dto.JzrcCertificateIn;
using QT.JZRC.Entitys;
using QT.JZRC.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.JZRC;

/// <summary>
/// 业务实现：建筑人才档案收件.
/// </summary>
[ApiDescriptionSettings(Tag = "JZRC", Name = "JzrcCertificateIn", Order = 200)]
[Route("api/JZRC/[controller]")]
public class JzrcCertificateInService : IJzrcCertificateInService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<JzrcCertificateInEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="JzrcCertificateInService"/>类型的新实例.
    /// </summary>
    public JzrcCertificateInService(
        ISqlSugarRepository<JzrcCertificateInEntity> jzrcCertificateInRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = jzrcCertificateInRepository;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取建筑人才档案收件.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<JzrcCertificateInInfoOutput>();
    }

    /// <summary>
    /// 获取建筑人才档案收件列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] JzrcCertificateInListQueryInput input)
    {
        List<string> talentList = new List<string>();
        if (input.companyId.IsNotEmptyOrNull())
        {
            talentList = await _repository.Context.Queryable<JzrcCompanyJobTalentEntity>().Where(it => it.CompanyId == input.companyId && it.Status == 1).Select(it => it.TalentId).Take(500).ToListAsync();
            talentList.Add(input.companyId);
        }
        //if (input.talentId.IsNotEmptyOrNull())
        //{
        //    talentList.Add(input.talentId);
        //}

        List<DateTime> queryInTime = input.inTime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startInTime = queryInTime?.First();
        DateTime? endInTime = queryInTime?.Last();
        var data = await _repository.Context.Queryable<JzrcCertificateInEntity>()
            .InnerJoin<JzrcTalentCertificateEntity>((it,a)=>it.CertificateId == a.Id)
            .WhereIF(talentList.IsAny(), (it,a) => talentList.Contains(a.TalentId))
            .WhereIF(input.talentId.IsNotEmptyOrNull(), (it, a) => a.TalentId == input.talentId)
            .WhereIF(!string.IsNullOrEmpty(input.storeroomId), it => it.StoreroomId.Equals(input.storeroomId))
            .WhereIF(queryInTime != null, it => SqlFunc.Between(it.InTime, startInTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endInTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.expressNo), it => it.ExpressNo.Contains(input.expressNo))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.StoreroomId.Contains(input.keyword)
                || it.ExpressNo.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select((it,a) => new JzrcCertificateInListOutput
            {
                id = it.Id,
                certificateId = it.CertificateId,
                certificateIdName = SqlFunc.Subqueryable<JzrcTalentCertificateEntity>().Where(ddd => ddd.Id == it.CertificateId).Select(ddd => ddd.CertificateName),
                storeroomId = it.StoreroomId,
                inTime = it.InTime,
                expressNo = it.ExpressNo,
                remark = it.Remark,
                handledBy = it.HandledBy,
                inoutType = it.InoutType,
                storeroomIdName = SqlFunc.Subqueryable<JzrcStoreroomEntity>().Where(ddd => ddd.Id == it.StoreroomId).Select(ddd => ddd.Name),
                talentIdName = SqlFunc.Subqueryable<JzrcTalentEntity>().Where(ddd => ddd.Id == a.TalentId).Select(ddd => ddd.Name),
                talentId = a.TalentId,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);

        var list = data.list.Select(x => x.talentId).Distinct().ToList();

        var companys = await _repository.Context.Queryable<JzrcCompanyJobTalentEntity>().Where(it => list.Contains(it.TalentId) && it.Status == 1)
            .Select(it => new
            {
                talentId = it.TalentId,
                companyIdName = SqlFunc.Subqueryable<JzrcCompanyEntity>().Where(ddd => ddd.Id == it.CompanyId).Select(ddd => ddd.CompanyName),
            }).ToListAsync();

        foreach (var item in data.list)
        {
            item.companyIdName = companys.Find(x => x.talentId == item.talentId)?.companyIdName ?? "";
        }

        return PageResult<JzrcCertificateInListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建建筑人才档案收件.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] JzrcCertificateInCrInput input)
    {
        var entity = input.Adapt<JzrcCertificateInEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新建筑人才档案收件.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] JzrcCertificateInUpInput input)
    {
        var entity = input.Adapt<JzrcCertificateInEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除建筑人才档案收件.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<JzrcCertificateInEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
}