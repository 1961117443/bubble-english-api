using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.JZRC.Entitys.Dto.JzrcTalentCertificate;
using QT.JZRC.Entitys;
using QT.JZRC.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Systems.Entitys.System;

namespace QT.JZRC;

/// <summary>
/// 业务实现：建筑人才证书信息.
/// </summary>
[ApiDescriptionSettings(Tag = "JZRC", Name = "JzrcTalentCertificate", Order = 200)]
[Route("api/JZRC/[controller]")]
public class JzrcTalentCertificateService : IJzrcTalentCertificateService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<JzrcTalentCertificateEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="JzrcTalentCertificateService"/>类型的新实例.
    /// </summary>
    public JzrcTalentCertificateService(
        ISqlSugarRepository<JzrcTalentCertificateEntity> jzrcTalentCertificateRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = jzrcTalentCertificateRepository;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取建筑人才证书信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<JzrcTalentCertificateInfoOutput>();
    }

    /// <summary>
    /// 获取建筑人才证书信息列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] JzrcTalentCertificateListQueryInput input)
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
        var data = await _repository.Context.Queryable<JzrcTalentCertificateEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.certificateName), it => it.CertificateName.Contains(input.certificateName))
            .WhereIF(!string.IsNullOrEmpty(input.region), it => it.Region.Equals(input.region))
            .WhereIF(!string.IsNullOrEmpty(input.categoryId), it => it.CategoryId.Equals(input.categoryId))
            .WhereIF(talentList.IsAny(), it=>  talentList.Contains(it.TalentId))
            .WhereIF(input.talentId.IsNotEmptyOrNull(), it => it.TalentId == input.talentId)
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.CertificateName.Contains(input.keyword)
                || it.Region.Contains(input.keyword)
                || it.CategoryId.Contains(input.keyword)
                || SqlFunc.Subqueryable<JzrcTalentEntity>().Where(ddd => ddd.Id == it.TalentId && ddd.Name.Contains(input.keyword)).Any()
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new JzrcTalentCertificateListOutput
            {
                id = it.Id,
                talentId = it.TalentId,
                certificateName = it.CertificateName,
                level = it.Level,
                acquisitionTime = it.AcquisitionTime,
                issuingOrganization = it.IssuingOrganization,
                validityPeriod = it.ValidityPeriod,
                remark = it.Remark,
                region = it.Region,
                categoryId = it.CategoryId,
                talentIdName = SqlFunc.Subqueryable<JzrcTalentEntity>().Where(ddd => ddd.Id == it.TalentId).Select(ddd => ddd.Name),
                categoryIdName = SqlFunc.Subqueryable<JzrcCertificateCategoryEntity>().Where(ddd => ddd.Id == it.CategoryId).Select(ddd => ddd.Name),
                regionName = SqlFunc.Subqueryable<ProvinceEntity>().Where(ddd => ddd.Id == it.Region).Select(ddd => ddd.FullName),
                certificateNo = it.CertificateNo
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<JzrcTalentCertificateListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建建筑人才证书信息.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] JzrcTalentCertificateCrInput input)
    {
        var entity = input.Adapt<JzrcTalentCertificateEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新建筑人才证书信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] JzrcTalentCertificateUpInput input)
    {
        var entity = input.Adapt<JzrcTalentCertificateEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除建筑人才证书信息.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<JzrcTalentCertificateEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
}