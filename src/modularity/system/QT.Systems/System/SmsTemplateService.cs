using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Extras.Thirdparty.Sms;
using QT.FriendlyException;
using QT.Systems.Entitys.Dto.SmsTemplate;
using QT.Systems.Entitys.Dto.SysConfig;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.System;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.Systems;

/// <summary>
/// base_sms_template服务.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "SmsTemplate", Order = 200)]
[Route("api/system/[controller]")]
public class SmsTemplateService : ISmsTemplateService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<SmsTemplateEntity> _repository;

    /// <summary>
    /// 系统配置服务.
    /// </summary>
    private readonly ISysConfigService _sysConfigService;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="SmsTemplateService"/>类型的新实例.
    /// </summary>
    public SmsTemplateService(
        ISqlSugarRepository<SmsTemplateEntity> smsTemplateRepository,
        ISysConfigService sysConfigService,
        IUserManager userManager)
    {
        _repository = smsTemplateRepository;
        _sysConfigService = sysConfigService;
        _userManager = userManager;
    }

    #region Get

    /// <summary>
    /// 获取base_sms_template列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] SmsTemplateListQueryInput input)
    {
        var data = await _repository.Context.Queryable<SmsTemplateEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.CreatorUserId == b.Id))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), a => a.FullName.Contains(input.keyword) || a.TemplateId.Contains(input.keyword))
            .Where(a => a.DeleteMark == null)
            .OrderBy(a => a.CreatorTime, OrderByType.Desc).OrderByIF(!string.IsNullOrEmpty(input.keyword), a => a.LastModifyTime, OrderByType.Desc)
            .Select((a, b) => new SmsTemplateListOutput
            {
                id = a.Id,
                company = SqlFunc.IIF(a.Company == 1, "阿里", "腾讯"),
                templateId = a.TemplateId,
                signContent = a.SignContent,
                enabledMark = a.EnabledMark,
                fullName = a.FullName,
                enCode = a.EnCode,
                creatorTime = a.CreatorTime,
                lastModifyTime = a.LastModifyTime,
                creatorUser = SqlFunc.MergeString(b.RealName, "/", b.Account),
            }).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<SmsTemplateListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 获取base_sms_template列表.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<dynamic> GetSelector([FromQuery] PageInputBase input)
    {
        var data = await _repository.Context.Queryable<SmsTemplateEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.CreatorUserId == b.Id))
             .WhereIF(!string.IsNullOrEmpty(input.keyword), a => a.FullName.Contains(input.keyword) || a.TemplateId.Contains(input.keyword))
             .Where(a => a.DeleteMark == null && a.EnabledMark == 1)
             .OrderBy(a => a.CreatorTime, OrderByType.Desc)
             .Select((a, b) => new SmsTemplateListOutput
             {
                 id = a.Id,
                 company = SqlFunc.IIF(a.Company == 1, "阿里", "腾讯"),
                 templateId = a.TemplateId,
                 signContent = a.SignContent,
                 enabledMark = a.EnabledMark,
                 fullName = a.FullName,
                 enCode = a.EnCode,
                 creatorTime = a.CreatorTime,
                 lastModifyTime = a.LastModifyTime,
                 creatorUser = SqlFunc.MergeString(b.RealName, "/", b.Account),
             }).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<SmsTemplateListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 获取base_sms_template.
    /// </summary>
    /// <param name="id">参数.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(p => p.Id == id && p.DeleteMark == null)).Adapt<SmsTemplateInfoOutput>();
    }

    /// <summary>
    /// 获取模板字段.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("getTemplate/{id}")]
    public async Task<dynamic> GetTemplate(string id)
    {
        var sysconfig = await _sysConfigService.GetInfo();
        var entity = await _repository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null);
        var smsModel = new SmsParameterInfo()
        {
            keyId = entity.Company == 1 ? sysconfig.aliAccessKey : sysconfig.tencentSecretId,
            keySecret = entity.Company == 1 ? sysconfig.aliSecret : sysconfig.tencentSecretKey,
            domain = entity.Endpoint,
            templateId = entity.TemplateId,
            region = entity.Region,
        };

        if (entity.Company == 1)
            return SmsUtil.GetTemplateByAli(smsModel);
        else
            return SmsUtil.GetTemplateByTencent(smsModel);
    }

    #endregion

    #region Post

    /// <summary>
    /// 新建base_sms_template.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] SmsTemplateCrInput input)
    {
        if (await _repository.AnyAsync(x => x.EnCode == input.enCode && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1004);
        var entity = input.Adapt<SmsTemplateEntity>();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新base_sms_template.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] SmsTemplateUpInput input)
    {
        if (await _repository.AnyAsync(x => x.Id != id && x.EnCode == input.enCode && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1004);
        var entity = input.Adapt<SmsTemplateEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandHasChangeAsync();
        if (!isOk) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除base_sms_template.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _repository.FirstOrDefaultAsync(p => p.Id.Equals(id));
        _ = entity ?? throw Oops.Oh(ErrorCode.COM1005);
        var isOk = await _repository.Context.Updateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 修改单据规则状态.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/State")]
    public async Task ActionsState_Api(string id)
    {
        var isOk = await _repository.Context.Updateable<SmsTemplateEntity>().SetColumns(it => new SmsTemplateEntity()
        {
            EnabledMark = SqlFunc.IIF(it.EnabledMark == 1, 0, 1),
            LastModifyUserId = _userManager.UserId,
            LastModifyTime = SqlFunc.GetDate()
        }).Where(it => it.Id.Equals(id)).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1003);

    }

    /// <summary>
    /// 获取模板字段.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("getTemplate")]
    public async Task<dynamic> GetTemplate([FromBody] SmsTemplateCrInput input)
    {
        var sysconfig = await _sysConfigService.GetInfo();
        var smsModel = new SmsParameterInfo()
        {
            keyId = input.company == 1 ? sysconfig.aliAccessKey : sysconfig.tencentSecretId,
            keySecret = input.company == 1 ? sysconfig.aliSecret : sysconfig.tencentSecretKey,
            domain = input.endpoint,
            templateId = input.templateId,
            region = input.region,
        };

        if (input.company == 1)
            return SmsUtil.GetTemplateByAli(smsModel);
        else
            return SmsUtil.GetTemplateByTencent(smsModel);
    }

    /// <summary>
    /// 测试发送.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("testSent")]
    public async Task SendTest([FromBody] SmsTemplateSendTestInput input)
    {
        var sysconfig = await _sysConfigService.GetInfo();
        var smsModel = new SmsParameterInfo()
        {
            keyId = input.company == 1 ? sysconfig.aliAccessKey : sysconfig.tencentSecretId,
            keySecret = input.company == 1 ? sysconfig.aliSecret : sysconfig.tencentSecretKey,
            region = input.region,
            domain = input.endpoint,
            templateId = input.templateId,
            signName = input.signContent
        };
        var msg = string.Empty;
        if (input.company == 1)
        {
            smsModel.mobileAli = input.phoneNumbers;
            smsModel.templateParamAli = input.parameters.ToJsonString();
            msg = SmsUtil.SendSmsByAli(smsModel);
        }
        else
        {
            smsModel.mobileTx = new string[] { input.phoneNumbers };
            List<string> mList = new List<string>();
            foreach (string data in input.parameters.Values)
            {
                mList.Add(data);
            }
            smsModel.appId = sysconfig.tencentAppId;
            smsModel.templateParamTx = mList.ToArray();
            msg = SmsUtil.SendSmsByTencent(smsModel);
        }

        if (msg.Equals("短信发送失败"))
            throw Oops.Oh(msg);
    }
    #endregion

    /// <summary>
    /// 获取短信模板字段.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<string>> GetSmsTemplateFields(string id)
    {
        var entity = await _repository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null);
        var sysconfig = await _sysConfigService.GetInfo();
        var smsModel = new SmsParameterInfo()
        {
            keyId = entity.Company == 1 ? sysconfig.aliAccessKey : sysconfig.tencentSecretId,
            keySecret = entity.Company == 1 ? sysconfig.aliSecret : sysconfig.tencentSecretKey,
            region = entity.Region,
            domain = entity.Endpoint,
            templateId = entity.TemplateId
        };

        if (entity.Company == 1)
            return SmsUtil.GetTemplateByAli(smsModel);
        else
            return SmsUtil.GetTemplateByTencent(smsModel);
    }

    /// <summary>
    /// 工作流发送短信.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="sysconfig"></param>
    /// <param name="phoneNumbers"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    [NonAction]
    public async Task FlowTaskSend(string id, SysConfigOutput sysconfig, List<string> phoneNumbers, Dictionary<string, string> parameters)
    {
        var entity = await _repository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null);
        var smsModel = new SmsParameterInfo()
        {
            keyId = entity.Company == 1 ? sysconfig.aliAccessKey : sysconfig.tencentSecretId,
            keySecret = entity.Company == 1 ? sysconfig.aliSecret : sysconfig.tencentSecretKey,
            region = entity.Region,
            domain = entity.Endpoint,
            templateId = entity.TemplateId,
            signName = entity.SignContent
        };
        if (entity.Company == 1)
        {
            smsModel.mobileAli = string.Join(",", phoneNumbers);
            smsModel.templateParamAli = parameters.ToJsonString();
            SmsUtil.SendSmsByAli(smsModel);
        }
        else
        {
            smsModel.mobileTx = phoneNumbers.ToArray();
            List<string> mList = new List<string>();
            var fields = await GetSmsTemplateFields(id);
            foreach (string item in fields)
            {
                if (parameters.ContainsKey(item))
                    mList.Add(parameters[item]);
            }

            smsModel.templateParamTx = mList.ToArray();
            SmsUtil.SendSmsByTencent(smsModel);
        }
    }
}