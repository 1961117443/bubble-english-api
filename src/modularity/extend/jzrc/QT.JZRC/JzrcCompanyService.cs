using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.ClayObject;
using QT.Common.Configuration;
using QT.Common.Models.NPOI;
using QT.DataEncryption;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.JZRC.Entitys.Dto.JzrcCompany;
using QT.JZRC.Entitys.Dto.JzrcCompanyCommunication;
using QT.JZRC.Entitys.Dto.JzrcCompanyJob;
using QT.JZRC.Entitys.Dto.JzrcCompanyQuality;
using QT.JZRC.Entitys.Dto.JzrcCompanyTalent;
using QT.JZRC.Entitys;
using QT.JZRC.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Microsoft.AspNetCore.Http;
using QT.Common.Core.Manager.Files;
using QT.Common.Helper;
using System.Data;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Web;
using QT.Common.Const;
using QT.JZRC.Entitys.Dto.AppService.Login;
using QT.JZRC.Entitys.Dto.AppService;
using QT.JZRC.Entitys.Dto.JzrcTalentCertificate;
using System.Drawing;
using NPOI.POIFS.Crypt.Dsig;
using QT.JZRC.Entitys.Dto.JzrcTalentCommunication;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.JZRC.Entitys.Dto.JzrcContract;
using QT.JZRC.Entitys.Dto.JzrcCompanyDemand;

namespace QT.JZRC;

/// <summary>
/// 业务实现：企业信息.
/// </summary>
[ApiDescriptionSettings(ModuleConst.JZRC, Tag = "JZRC", Name = "JzrcCompany", Order = 200)]
[Route("api/JZRC/[controller]")]
[Injection(Named =nameof(AppLoginUserRole.Company))]
public class JzrcCompanyService : IJzrcCompanyService, IDynamicApiController, ITransient, IJzrcAppLogin
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<JzrcCompanyEntity> _repository;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly IFileManager _fileManager;

    /// <summary>
    /// 初始化一个<see cref="JzrcCompanyService"/>类型的新实例.
    /// </summary>
    public JzrcCompanyService(
        ISqlSugarRepository<JzrcCompanyEntity> jzrcCompanyRepository,
        ISqlSugarClient context,
        IUserManager userManager, IFileManager fileManager)
    {
        _repository = jzrcCompanyRepository;
        _db = context.AsTenant();
        _userManager = userManager;
        _fileManager = fileManager;
    }

    /// <summary>
    /// 获取企业信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<JzrcCompanyInfoOutput>();

        output.jzrcCompanyCommunicationList = await _repository.Context.Queryable<JzrcCompanyCommunicationEntity>()
            .Where(w => w.CompanyId == output.id).OrderByDescending(w=>w.CommunicationTime)
            .Select(w=> new JzrcCompanyCommunicationInfoOutput
            {
                managerIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == w.ManagerId).Select(ddd => ddd.RealName),
            },true)
            .ToListAsync();
        //output.jzrcCompanyCommunicationList = jzrcCompanyCommunicationList.Adapt<List<JzrcCompanyCommunicationInfoOutput>>();

        var jzrcCompanyJobList = await _repository.Context.Queryable<JzrcCompanyJobEntity>().Where(w => w.CompanyId == output.id).ToListAsync();
        output.jzrcCompanyJobList = jzrcCompanyJobList.Adapt<List<JzrcCompanyJobInfoOutput>>();

        var jzrcCompanyQualityList = await _repository.Context.Queryable<JzrcCompanyQualityEntity>().Where(w => w.CompanyId == output.id).ToListAsync();
        output.jzrcCompanyQualityList = jzrcCompanyQualityList.Adapt<List<JzrcCompanyQualityInfoOutput>>();

        //var jzrcCompanyTalentList = await _repository.Context.Queryable<JzrcCompanyTalentEntity>().Where(w => w.CompanyId == output.id).ToListAsync();
        //output.jzrcCompanyTalentList = jzrcCompanyTalentList.Adapt<List<JzrcCompanyTalentInfoOutput>>();

        output.jzrcCompanyTalentList = await _repository.Context.Queryable<JzrcCompanyJobTalentEntity>().Where(it => it.CompanyId == output.id)
            .OrderBy(it=>it.JobId)
            .OrderByDescending(it => it.Amount)
            .OrderBy(it => it.CreatorTime)
            .Select(it => new JzrcCompanyTalentInfoOutput
            {
                talentIdName = SqlFunc.Subqueryable<JzrcTalentEntity>().Where(ddd => ddd.Id == it.TalentId).Select(ddd => ddd.Name),
                status = it.Status,
                creatorTime = it.CreatorTime,
                amount = it.Amount,
                jobIdName = SqlFunc.Subqueryable<JzrcCompanyJobEntity>().Where(ddd => ddd.Id == it.JobId).Select(ddd => ddd.JobTitle),
            })
            .ToListAsync();

        output.jzrcCompanyContractList = await _repository.Context.Queryable<JzrcContractEntity>()
            .Where(it => it.CompanyId == id)
            .Select(it => new JzrcContractListOutput
            {
                id = it.Id,
                no = it.No,
                talentId = it.TalentId,
                companyId = it.CompanyId,
                certificateId = it.CertificateId,
                jobId = it.JobId,
                signTime = it.SignTime,
                amount = it.Amount,
                talentIdName = SqlFunc.Subqueryable<JzrcTalentEntity>().Where(ddd => ddd.Id == it.TalentId).Select(ddd => ddd.Name),
                companyIdName = SqlFunc.Subqueryable<JzrcCompanyEntity>().Where(ddd => ddd.Id == it.CompanyId).Select(ddd => ddd.CompanyName),
            }).ToListAsync();

        output.jzrcCompanyDemandList = await _repository.Context.Queryable<JzrcCompanyDemandEntity>()
            .Where(it => it.CompanyId == id)
            .Select(it => new JzrcCompanyDemandListOutput
            {
                id = it.Id,
                companyId = it.CompanyId,
                content = it.Content,
                companyIdName = SqlFunc.Subqueryable<JzrcCompanyEntity>().Where(ddd => ddd.Id == it.CompanyId).Select(ddd => ddd.CompanyName),
                creatorTime = it.CreatorTime,
            }).ToListAsync();
        return output;
    }

    /// <summary>
    /// 获取企业信息列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] JzrcCompanyListQueryInput input)
    {
        var regions = input.region?.Split(",", true) ?? new string[0];
        List<DateTime> querySettlementDate = input.settlementDate?.Split(',').ToObject<List<DateTime>>();
        DateTime? startSettlementDate = querySettlementDate?.First();
        DateTime? endSettlementDate = querySettlementDate?.Last();
        var data = await _repository.Context.Queryable<JzrcCompanyEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.managerId), it => it.ManagerId == input.managerId)
            .WhereIF(!string.IsNullOrEmpty(input.companyName), it => it.CompanyName.Contains(input.companyName))
            .WhereIF(!string.IsNullOrEmpty(input.contactPerson), it => it.ContactPerson.Contains(input.contactPerson))
            .WhereIF(regions.IsAny(),it=> regions.Contains(it.Province))
            .WhereIF(!string.IsNullOrEmpty(input.contactPhoneNumber), it => it.ContactPhoneNumber.Contains(input.contactPhoneNumber))
            .WhereIF(input.signed == 2, it => (it.ManagerId ?? "") == "")
            .WhereIF(input.signed == 1, it => SqlFunc.Subqueryable<JzrcMemberEntity>().Where(ddd => ddd.RelationId == it.Id && ddd.Role == AppLoginUserRole.Company).Any())
            .WhereIF(input.signed == 0, it => SqlFunc.Subqueryable<JzrcMemberEntity>().Where(ddd => ddd.RelationId == it.Id && ddd.Role == AppLoginUserRole.Company).NotAny())
            .WhereIF(querySettlementDate != null, it => SqlFunc.Between(it.SettlementDate, startSettlementDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endSettlementDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.CompanyName.Contains(input.keyword)
                || it.ContactPerson.Contains(input.keyword)
                || it.ContactPhoneNumber.Contains(input.keyword)
                || it.UnifiedSocialCreditCode.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new JzrcCompanyListOutput
            {
                id = it.Id,
                companyName = it.CompanyName,
                contactPerson = it.ContactPerson,
                contactPhoneNumber = it.ContactPhoneNumber,
                establishmentDate = it.EstablishmentDate,
                registeredCapital = it.RegisteredCapital,
                unifiedSocialCreditCode = it.UnifiedSocialCreditCode,
                number = it.Number,
                industry = it.Industry,
                major = it.Major,
                settlementDate = it.SettlementDate,
                region = SqlFunc.Subqueryable<ProvinceEntity>().Where(ddd => ddd.Id == it.Province).Select(ddd => ddd.FullName),
                managerIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == it.ManagerId).Select(ddd => ddd.RealName),
                signed = SqlFunc.Subqueryable<JzrcMemberEntity>().Where(ddd => ddd.RelationId == it.Id && ddd.Role == AppLoginUserRole.Company).Any(),
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<JzrcCompanyListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建企业信息.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] JzrcCompanyCrInput input)
    {
        var entity = input.Adapt<JzrcCompanyEntity>();
        entity.Id = SnowflakeIdHelper.NextId();

        // 判断统一信用代码是否存在
        if (!string.IsNullOrEmpty(entity.UnifiedSocialCreditCode) && await _repository.Context.Queryable<JzrcCompanyEntity>().AnyAsync(x => x.UnifiedSocialCreditCode == entity.UnifiedSocialCreditCode))
        {
            throw Oops.Oh("统一社会信用代码已存在，不允许重复添加！");
        }

        try
        {
            // 开启事务
            _db.BeginTran();

            var newEntity = await _repository.Context.Insertable<JzrcCompanyEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            /*
            var jzrcCompanyCommunicationEntityList = input.jzrcCompanyCommunicationList.Adapt<List<JzrcCompanyCommunicationEntity>>();
            if (jzrcCompanyCommunicationEntityList != null)
            {
                foreach (var item in jzrcCompanyCommunicationEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.CompanyId = newEntity.Id;
                }

                await _repository.Context.Insertable<JzrcCompanyCommunicationEntity>(jzrcCompanyCommunicationEntityList).ExecuteCommandAsync();
            }

            var jzrcCompanyJobEntityList = input.jzrcCompanyJobList.Adapt<List<JzrcCompanyJobEntity>>();
            if (jzrcCompanyJobEntityList != null)
            {
                foreach (var item in jzrcCompanyJobEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.CompanyId = newEntity.Id;
                }

                await _repository.Context.Insertable<JzrcCompanyJobEntity>(jzrcCompanyJobEntityList).ExecuteCommandAsync();
            }
            */
            var jzrcCompanyQualityEntityList = input.jzrcCompanyQualityList.Adapt<List<JzrcCompanyQualityEntity>>();
            if (jzrcCompanyQualityEntityList != null)
            {
                foreach (var item in jzrcCompanyQualityEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.CompanyId = newEntity.Id;
                }

                await _repository.Context.Insertable<JzrcCompanyQualityEntity>(jzrcCompanyQualityEntityList).ExecuteCommandAsync();
            }
            /*
            var jzrcCompanyTalentEntityList = input.jzrcCompanyTalentList.Adapt<List<JzrcCompanyTalentEntity>>();
            if (jzrcCompanyTalentEntityList != null)
            {
                foreach (var item in jzrcCompanyTalentEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.CompanyId = newEntity.Id;
                }

                await _repository.Context.Insertable<JzrcCompanyTalentEntity>(jzrcCompanyTalentEntityList).ExecuteCommandAsync();
            }
            */

            // 关闭事务
            _db.CommitTran();
        }
        catch (Exception)
        {
            // 回滚事务
            _db.RollbackTran();

            throw Oops.Oh(ErrorCode.COM1000);
        }
    }

    /// <summary>
    /// 获取企业信息无分页列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    private async Task<dynamic> GetNoPagingList([FromQuery] JzrcCompanyListQueryInput input)
    {
        List<DateTime> querySettlementDate = input.settlementDate?.Split(',').ToObject<List<DateTime>>();
        DateTime? startSettlementDate = querySettlementDate?.First();
        DateTime? endSettlementDate = querySettlementDate?.Last();
        return await _repository.Context.Queryable<JzrcCompanyEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.companyName), it => it.CompanyName.Contains(input.companyName))
            .WhereIF(!string.IsNullOrEmpty(input.contactPerson), it => it.ContactPerson.Contains(input.contactPerson))
            .WhereIF(!string.IsNullOrEmpty(input.contactPhoneNumber), it => it.ContactPhoneNumber.Contains(input.contactPhoneNumber))
            .WhereIF(querySettlementDate != null, it => SqlFunc.Between(it.SettlementDate, startSettlementDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endSettlementDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.CompanyName.Contains(input.keyword)
                || it.ContactPerson.Contains(input.keyword)
                || it.ContactPhoneNumber.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new JzrcCompanyListOutput
            {
                id = it.Id,
                companyName = it.CompanyName,
                contactPerson = it.ContactPerson,
                contactPhoneNumber = it.ContactPhoneNumber,
                establishmentDate = it.EstablishmentDate,
                registeredCapital = it.RegisteredCapital,
                unifiedSocialCreditCode = it.UnifiedSocialCreditCode,
                number = it.Number,
                industry = it.Industry,
                major = it.Major,
                settlementDate = it.SettlementDate,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToListAsync();
    }

    /// <summary>
    /// 导出企业信息.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Actions/Export")]
    public async Task<dynamic> Export([FromQuery] JzrcCompanyListQueryInput input)
    {
        var exportData = new List<JzrcCompanyListOutput>();
        if (input.dataType == 0)
            exportData = Clay.Object(await GetList(input)).Solidify<PageResult<JzrcCompanyListOutput>>().list;
        else
            exportData = await GetNoPagingList(input);
        List<ParamsModel> paramList = "[{\"value\":\"企业名称\",\"field\":\"companyName\"},{\"value\":\"企业联系人\",\"field\":\"contactPerson\"},{\"value\":\"联系电话\",\"field\":\"contactPhoneNumber\"},{\"value\":\"成立时间\",\"field\":\"establishmentDate\"},{\"value\":\"注册资金\",\"field\":\"registeredCapital\"},{\"value\":\"统一社会信用代码\",\"field\":\"unifiedSocialCreditCode\"},{\"value\":\"公司人数\",\"field\":\"number\"},{\"value\":\"所属行业\",\"field\":\"industry\"},{\"value\":\"专业\",\"field\":\"major\"},{\"value\":\"入驻时间\",\"field\":\"settlementDate\"},]".ToList<ParamsModel>();
        ExcelConfig excelconfig = new ExcelConfig();
        excelconfig.FileName = "企业信息.xls";
        excelconfig.HeadFont = "微软雅黑";
        excelconfig.HeadPoint = 10;
        excelconfig.IsAllSizeColumn = true;
        excelconfig.ColumnModel = new List<ExcelColumnModel>();
        foreach (var item in input.selectKey.Split(',').ToList())
        {
            var isExist = paramList.Find(p => p.field == item);
            if (isExist != null)
                excelconfig.ColumnModel.Add(new ExcelColumnModel() { Column = isExist.field, ExcelColumn = isExist.value });
        }

        var addPath = FileVariable.TemporaryFilePath + excelconfig.FileName;
        ExcelExportHelper<JzrcCompanyListOutput>.Export(exportData, excelconfig, addPath);
        var fileName = _userManager.UserId + "|" + addPath + "|xls";
        return new
        {
            name = excelconfig.FileName,
            url = "/api/File/Download?encryption=" + DESCEncryption.Encrypt(fileName, "QT")
        };
    }

    /// <summary>
    /// 批量删除企业信息.
    /// </summary>
    /// <param name="ids">主键数组.</param>
    /// <returns></returns>
    [HttpPost("batchRemove")]
    public async Task BatchRemove([FromBody] List<string> ids)
    {
        var entitys = await _repository.Context.Queryable<JzrcCompanyEntity>()
            .In(it => it.Id, ids)
            .Where(it => SqlFunc.Subqueryable<JzrcMemberEntity>().Where(ddd => ddd.RelationId == it.Id && ddd.Role == AppLoginUserRole.Company).NotAny()) // 过滤掉已入驻的数据
            .ToListAsync();
        if (entitys.Count > 0)
        {
            try
            {
                // 开启事务
                _db.BeginTran();

                foreach (var entity in entitys)
                {
                    _repository.Context.Tracking(entity);
                    entity.Delete();
                }
                await _repository.Context.Updateable(entitys).ExecuteCommandAsync();

                //// 批量删除企业信息
                //await _repository.Context.Deleteable<JzrcCompanyEntity>().In(it => it.Id, ids).ExecuteCommandAsync();

                //// 清空企业沟通记录表数据
                //await _repository.Context.Deleteable<JzrcCompanyCommunicationEntity>().In(u => u.CompanyId, entitys.Select(s => s.Id).ToArray()).ExecuteCommandAsync();

                //// 清空企业招聘职位表数据
                //await _repository.Context.Deleteable<JzrcCompanyJobEntity>().In(u => u.CompanyId, entitys.Select(s => s.Id).ToArray()).ExecuteCommandAsync();

                //// 清空企业资质信息表数据
                //await _repository.Context.Deleteable<JzrcCompanyQualityEntity>().In(u => u.CompanyId, entitys.Select(s => s.Id).ToArray()).ExecuteCommandAsync();

                //// 清空企业签约人才表数据
                //await _repository.Context.Deleteable<JzrcCompanyTalentEntity>().In(u => u.CompanyId, entitys.Select(s => s.Id).ToArray()).ExecuteCommandAsync();

                // 关闭事务
                _db.CommitTran();
            }
            catch (Exception)
            {
                // 回滚事务
                _db.RollbackTran();
                throw Oops.Oh(ErrorCode.COM1002);
            }
        }
    }

    /// <summary>
    /// 更新企业信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] JzrcCompanyUpInput input)
    {
        var entity = input.Adapt<JzrcCompanyEntity>();
        // 判断统一信用代码是否存在
        if (!string.IsNullOrEmpty(entity.UnifiedSocialCreditCode) && await _repository.Context.Queryable<JzrcCompanyEntity>().AnyAsync(x => x.Id != entity.Id && x.UnifiedSocialCreditCode == entity.UnifiedSocialCreditCode))
        {
            throw Oops.Oh("统一社会信用代码已存在，不允许重复添加！");
        }
        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.Context.Updateable<JzrcCompanyEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

            //新增企业资质信息新数据
            await _repository.Context.CUDSaveAsnyc<JzrcCompanyQualityEntity>(it => it.CompanyId == entity.Id, input.jzrcCompanyQualityList, it => it.CompanyId = entity.Id);
            //// 清空企业沟通记录原有数据
            //await _repository.Context.Deleteable<JzrcCompanyCommunicationEntity>().Where(it => it.CompanyId == entity.Id).ExecuteCommandAsync();

            //// 新增企业沟通记录新数据
            //var jzrcCompanyCommunicationEntityList = input.jzrcCompanyCommunicationList.Adapt<List<JzrcCompanyCommunicationEntity>>();
            //if (jzrcCompanyCommunicationEntityList != null)
            //{
            //    foreach (var item in jzrcCompanyCommunicationEntityList)
            //    {
            //        item.Id ??= SnowflakeIdHelper.NextId();
            //        item.CompanyId = entity.Id;
            //    }

            //    await _repository.Context.Insertable<JzrcCompanyCommunicationEntity>(jzrcCompanyCommunicationEntityList).ExecuteCommandAsync();
            //}

            //// 清空企业招聘职位原有数据
            //await _repository.Context.Deleteable<JzrcCompanyJobEntity>().Where(it => it.CompanyId == entity.Id).ExecuteCommandAsync();

            //// 新增企业招聘职位新数据
            //var jzrcCompanyJobEntityList = input.jzrcCompanyJobList.Adapt<List<JzrcCompanyJobEntity>>();
            //if (jzrcCompanyJobEntityList != null)
            //{
            //    foreach (var item in jzrcCompanyJobEntityList)
            //    {
            //        item.Id ??= SnowflakeIdHelper.NextId();
            //        item.CompanyId = entity.Id;
            //    }

            //    await _repository.Context.Insertable<JzrcCompanyJobEntity>(jzrcCompanyJobEntityList).ExecuteCommandAsync();
            //}

            //// 清空企业资质信息原有数据
            //await _repository.Context.Deleteable<JzrcCompanyQualityEntity>().Where(it => it.CompanyId == entity.Id).ExecuteCommandAsync();

            //// 新增企业资质信息新数据
            //var jzrcCompanyQualityEntityList = input.jzrcCompanyQualityList.Adapt<List<JzrcCompanyQualityEntity>>();
            //if (jzrcCompanyQualityEntityList != null)
            //{
            //    foreach (var item in jzrcCompanyQualityEntityList)
            //    {
            //        item.Id ??= SnowflakeIdHelper.NextId();
            //        item.CompanyId = entity.Id;
            //    }

            //    await _repository.Context.Insertable<JzrcCompanyQualityEntity>(jzrcCompanyQualityEntityList).ExecuteCommandAsync();
            //}

            //// 清空企业签约人才原有数据
            //await _repository.Context.Deleteable<JzrcCompanyTalentEntity>().Where(it => it.CompanyId == entity.Id).ExecuteCommandAsync();

            //// 新增企业签约人才新数据
            //var jzrcCompanyTalentEntityList = input.jzrcCompanyTalentList.Adapt<List<JzrcCompanyTalentEntity>>();
            //if (jzrcCompanyTalentEntityList != null)
            //{
            //    foreach (var item in jzrcCompanyTalentEntityList)
            //    {
            //        item.Id ??= SnowflakeIdHelper.NextId();
            //        item.CompanyId = entity.Id;
            //    }

            //    await _repository.Context.Insertable<JzrcCompanyTalentEntity>(jzrcCompanyTalentEntityList).ExecuteCommandAsync();
            //}

            // 关闭事务
            _db.CommitTran();
        }
        catch (Exception)
        {
            // 回滚事务
            _db.RollbackTran();
            throw Oops.Oh(ErrorCode.COM1001);
        }
    }

    /// <summary>
    /// 删除企业信息.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _repository.Context.Queryable<JzrcCompanyEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        //如果已入驻，不允许删除
        if (await _repository.Context.Queryable<JzrcMemberEntity>().Where(it=>it.RelationId == entity.Id && it.Role == AppLoginUserRole.Company).AnyAsync())
        {
            throw Oops.Oh("该企业已入驻，不允许删除！");
        }

        _repository.Context.Tracking(entity);
        entity.Delete();
        await _repository.Context.Updateable(entity).ExecuteCommandAsync();

        //if (!await _repository.Context.Queryable<JzrcCompanyEntity>().AnyAsync(it => it.Id == id))
        //{
        //    throw Oops.Oh(ErrorCode.COM1005);
        //}

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        //    var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        //    await _repository.Context.Deleteable<JzrcCompanyEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

        //    // 清空企业沟通记录表数据
        //    await _repository.Context.Deleteable<JzrcCompanyCommunicationEntity>().Where(it => it.CompanyId.Equals(entity.Id)).ExecuteCommandAsync();

        //    // 清空企业招聘职位表数据
        //    await _repository.Context.Deleteable<JzrcCompanyJobEntity>().Where(it => it.CompanyId.Equals(entity.Id)).ExecuteCommandAsync();

        //    // 清空企业资质信息表数据
        //    await _repository.Context.Deleteable<JzrcCompanyQualityEntity>().Where(it => it.CompanyId.Equals(entity.Id)).ExecuteCommandAsync();

        //    // 清空企业签约人才表数据
        //    await _repository.Context.Deleteable<JzrcCompanyTalentEntity>().Where(it => it.CompanyId.Equals(entity.Id)).ExecuteCommandAsync();

        //    // 关闭事务
        //    _db.CommitTran();
        //}
        //catch (Exception)
        //{
        //    // 回滚事务
        //    _db.RollbackTran();

        //    throw Oops.Oh(ErrorCode.COM1002);
        //}
    }

    #region 导入数据
    /// <summary>
    /// 模板下载.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/TemplateDownload")]
    public dynamic TemplateDownload()
    {
        // 初始化 一条空数据 
        var dataList = new List<JzrcCompanyImportDataTemplate>() { new JzrcCompanyImportDataTemplate() { } };

        ExcelConfig excelconfig = ExcelConfig.Default("企业信息导入模板.xls");
        foreach (KeyValuePair<string, string> item in Common.Extension.Extensions.GetPropertityToMaps<JzrcCompanyImportDataTemplate>())
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel!.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName!);
        ExcelExportHelper<JzrcCompanyImportDataTemplate>.Export(dataList, excelconfig, addPath);

        return new { name = excelconfig.FileName, url = "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "QT") };
    }

    /// <summary>
    /// 导入数据.
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    [HttpPost("Actions/Import")]
    [SqlSugarUnitOfWork]
    public async Task<dynamic> ImportData(IFormFile file)
    {
        var _filePath = _fileManager.GetPathByType(string.Empty);
        var _fileName = DateTime.Now.ToString("yyyyMMdd") + "_" + SnowflakeIdHelper.NextId() + Path.GetExtension(file.FileName);
        var stream = file.OpenReadStream();
        await _fileManager.UploadFileByType(stream, _filePath, _fileName);

        Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<JzrcCompanyImportDataInput>();

        string? filePath = FileVariable.TemporaryFilePath;
        string? savePath = Path.Combine(filePath, _fileName);

        // 得到数据
        var excelData = ExcelImportHelper.ToDataTable(file.OpenReadStream(), ExcelImportHelper.IsXls(_fileName));
        if (excelData != null)
        {
            foreach (DataColumn item in excelData.Columns)
            {
                var key = FileEncode.Where(x => x.Value == item.ToString()).FirstOrDefault().Key;
                if (!string.IsNullOrEmpty(key))
                {
                    item.ColumnName = key;
                    //excelData.Columns[item.ToString()].ColumnName = ;
                }
            }

        }

        var list = _repository.Context.Utilities.DataTableToList<JzrcCompanyImportDataInput>(excelData);
        var provinceList = await _repository.Context.Queryable<ProvinceEntity>().Select(m => new ProvinceEntity
        {
            Id = m.Id,
            FullName = m.FullName
        }).Where(m => m.ParentId == "-1" && m.DeleteMark == null && m.EnabledMark == 1).ToListAsync();
        //检查企业名称是否为空
        foreach (var item in list)
        {
            if (string.IsNullOrWhiteSpace(item.companyName))
            {
                throw Oops.Oh("企业名称不能为空！");
            }
            if (!DateTime.TryParse(item.approvalDate, out var dt))
            {
                item.approvalDate = null;
            }
            if (!DateTime.TryParse(item.establishmentDate, out var dt1))
            {
                item.establishmentDate = null;
            }

            // 区域转换
            item.province = provinceList.Find(w => w.FullName == item.province)?.Id ?? "";
        }


        //转化为实体 ErpProductEntity\ErpProductmodelEntity 
        var insertEntities = list.Where(x => string.IsNullOrEmpty(x.id)).Adapt<List<JzrcCompanyEntity>>();
        var updateEntities = list.Where(x => !string.IsNullOrEmpty(x.id)).Adapt<List<JzrcCompanyEntity>>();

        if (updateEntities.Any())
        {
            await updateEntities.PagingLoop(async (items, index) =>
            {
                var tempIdList = items.Select(x => x.Id).ToList();

                var dbEntitys = await _repository.Where(x => tempIdList.Contains(x.Id)).ToListAsync();



                for (int i = items.Count - 1; i > -1; i--)
                {
                    var item = items[i];
                    var entity = dbEntitys.Find(x => x.Id == item.Id);
                    if (entity == null)
                    {
                        insertEntities.Add(entity);
                    }
                    else
                    {
                        _repository.Context.Tracking(item);
                        item.Adapt(entity);
                        items.RemoveAt(i);
                    }
                }
                if (items.IsAny())
                {
                    await _repository.Context.Updateable(items).ExecuteCommandAsync();
                }

            });
        }

        //更新数据库
        if (insertEntities.Any())
        {
            insertEntities.ForEach(x => x.Id ??= SnowflakeIdHelper.NextId());
            await _repository.Context.Insertable(insertEntities).ExecuteCommandAsync();
        }

        return new { np = insertEntities.Count, up = updateEntities.Count };
    }

    #endregion


    /// <summary>
    /// 分配客户经理
    /// </summary>
    /// <returns></returns>
    [HttpPut("Actions/Allot")]
    public async Task<int> AllotManager([FromBody] JzrcCompanyAllotManagerInput input)
    {
        if (input.radio == 1 && !input.ids.IsAny())
        {
            throw Oops.Oh("请选择数据！");
        }
        List<string> regions = new List<string>();
        if (input.radio == 2)
        {
            if (string.IsNullOrEmpty(input.province))
            {
                throw Oops.Oh("请选择区域！");
            }
            if (input.number <0 )
            {
                throw Oops.Oh("数量不能小于等于0！");
            }
            regions.Add(input.province);
            var arr = input.province.Split(',').ToArray();
            regions.AddRange(arr);
            var list1 = await _repository.Context.Queryable<ProvinceEntity>().In(it => it.Id, arr).Select(it => it.FullName).ToListAsync();
            regions.AddRange(list1);
        }
        int num = input.radio == 1 ? input.ids.Length : Math.Min(input.number, 1000);

        // 使用分布式锁
        using (var l = RedisHelper.Lock($"JzrcCompanyService:AllotManager:{_userManager.TenantId}", 5))
        {
            var qur = input.radio == 1 ? _repository.Where(x => input.ids.Contains(x.Id)) : _repository.Where(x => regions.Contains(x.Province) && string.IsNullOrEmpty(x.ManagerId));
            var list = await qur.Take(num).ToListAsync();

            list.ForEach(x =>
            {
                x.ManagerId = input.managerId;
            });

            var result = await _repository.Context.Updateable<JzrcCompanyEntity>(list)
                .UpdateColumns(nameof(JzrcCompanyEntity.ManagerId))
                .ExecuteCommandAsync();

            return result;
        }

    }

    /// <summary>
    /// 添加沟通记录
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("Actions/{id}/Communication")]
    public async Task JzrcCompanyCommunication(string id, [FromBody] JzrcCompanyCommunicationCrInput input)
    {
        var company = await _repository.AsQueryable().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);
        // 当前用户非客户经理，禁止添加
        if (company.ManagerId != _userManager.UserId)
        {
            throw Oops.Oh(ErrorCode.D1013);
        }

        var entity = input.Adapt<JzrcCompanyCommunicationEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.ManagerId = _userManager.UserId;
        entity.CompanyId = id;
        if (!entity.CommunicationTime.HasValue)
        {
            entity.CommunicationTime = DateTime.Now;
        }

        await _repository.Context.Insertable<JzrcCompanyCommunicationEntity>(entity).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取公司的沟通记录
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("Actions/{id}/Communication")]
    public async Task<List<JzrcCompanyCommunicationInfoOutput>> JzrcCompanyCommunicationList(string id)
    {
        var jzrcCompanyCommunicationList = await _repository.Context.Queryable<JzrcCompanyCommunicationEntity>().Where(w => w.CompanyId == id).OrderByDescending(w => w.CommunicationTime).ToListAsync();
        return jzrcCompanyCommunicationList.Adapt<List<JzrcCompanyCommunicationInfoOutput>>();
    }

    /// <summary>
    /// 获取企业发布的岗位列表
    /// </summary>
    /// <param name="talentId"></param>
    /// <returns></returns>
    [HttpGet("Actions/{id}/JobList")]
    public async Task<List<JzrcCompanyJobListOutput>> GetJobList(string id)
    {
        var data =  await _repository.Context.Queryable<JzrcCompanyJobEntity>()
            .Where(it=>it.CompanyId == id)
            .Select(it => new JzrcCompanyJobListOutput
            {
                id = it.Id,
                companyId = it.CompanyId,
                jobTitle = it.JobTitle,
                candidateType = it.CandidateType,
                number = it.Number,
                jobSalary = it.JobSalary,
                requiredStart = it.RequiredStart,
                requiredEnd = it.RequiredEnd,
                region = it.Region,
                certificateCategoryId = it.CertificateCategoryId,
                certificateLevel = it.CertificateLevel,
                status = it.Status ?? 0,
                price = it.Price,
                companyIdName = SqlFunc.Subqueryable<JzrcCompanyEntity>().Where(ddd => ddd.Id == it.CompanyId).Select(ddd => ddd.CompanyName)
            }).ToListAsync();

        return data;
    }

    #region client 前台服务
    ///// <summary>
    ///// 前台登录
    ///// </summary>
    ///// <param name="loginInput"></param>
    ///// <returns></returns>
    ///// <exception cref="NotImplementedException"></exception>
    //public async Task<AppLoginUser> Login(LoginDto loginInput)
    //{
    //    var entity = await _repository.SingleAsync(x => x.ContactPhoneNumber == loginInput.UserName) ?? throw Oops.Oh("用户不存在！");

    //    AppLoginUser appLoginUser = new AppLoginUser
    //    {
    //        Id = entity.Id,
    //        Account = entity.ContactPhoneNumber,
    //        RealName = entity.ContactPerson,
    //        Role = AppLoginUserRole.Company
    //    };

    //    return appLoginUser;
    //}

    ///// <summary>
    ///// 前台登录
    ///// </summary>
    ///// <param name="loginInput"></param>
    ///// <returns></returns>
    ///// <exception cref="NotImplementedException"></exception>
    //[NonAction]
    //public async Task<AppLoginUser?> GetByAccount(string account)
    //{
    //    var entity = await _repository.SingleAsync(x => x.ContactPhoneNumber == account);

    //    if (entity == null)
    //    {
    //        return null;
    //    }
    //    AppLoginUser appLoginUser = new AppLoginUser
    //    {
    //        Id = entity.Id,
    //        Account = entity.ContactPhoneNumber,
    //        RealName = entity.ContactPerson,
    //        Role = AppLoginUserRole.Company
    //    };

    //    return appLoginUser;
    //}

    /// <summary>
    /// 创建或者获取企业信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<AppLoginUser?> GetOrCreateAsync(AppLoginCrInput input)
    {
        if (input.phone.IsNullOrEmpty())
        {
            throw Oops.Oh("手机号码不能为空！");
        }
        if (input.name.IsNullOrEmpty())
        {
            throw Oops.Oh("姓名不能为空！");
        }
        // 分布式锁
        using (var l = RedisHelper.Lock($"AppLoginUser:{input.phone}", 5))
        {
            var entity = await _repository.SingleAsync(x => x.ContactPhoneNumber == input.phone);
            if (entity == null)
            {
                entity = new JzrcCompanyEntity
                {
                    Id = SnowflakeIdHelper.NextId(),
                    ContactPhoneNumber = input.phone,
                    ContactPerson = input.name
                };
                await _repository.Context.Insertable(entity).IgnoreColumns(true).ExecuteCommandAsync();
            }

            return new AppLoginUser
            {
                Id = entity.Id,
                Account = entity.ContactPhoneNumber,
                RealName = entity.ContactPerson,
                Role = AppLoginUserRole.Company
            };
        }
    }
    #endregion
}