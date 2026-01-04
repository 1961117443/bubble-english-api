using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Extras.Thirdparty.DingDing;
using QT.Extras.Thirdparty.Email;
using QT.Extras.Thirdparty.Sms;
using QT.Extras.Thirdparty.WeChat;
using QT.FriendlyException;
using QT.Message.Interfaces.Message;
using QT.Systems.Entitys.Dto.MessageTemplate;
using QT.Systems.Entitys.Dto.SysConfig;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.Permission;
using QT.Systems.Interfaces.System;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System.Web;

namespace QT.Systems;

/// <summary>
/// base_message_template服务.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "MessageTemplate", Order = 200)]
[Route("api/system/[controller]")]
public class MessageTemplateService : IMessageTemplateService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<MessageTemplateEntity> _repository;

    /// <summary>
    /// 短信模板服务.
    /// </summary>
    private readonly ISmsTemplateService _smsTemplateService;

    /// <summary>
    /// 系统配置服务.
    /// </summary>
    private readonly ISysConfigService _sysConfigService;

    /// <summary>
    /// 消息服务.
    /// </summary>
    private readonly IMessageService _messageService;

    /// <summary>
    /// 用户服务.
    /// </summary>
    private readonly IUsersService _usersService;

    /// <summary>
    /// 第三方同步服务.
    /// </summary>
    private readonly ISynThirdInfoService _synThirdInfoService;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="MessageTemplateService"/>类型的新实例.
    /// </summary>
    public MessageTemplateService(
        ISqlSugarRepository<MessageTemplateEntity> messageTemplateRepository,
        ISmsTemplateService smsTemplateService,
        ISysConfigService sysConfigService,
        IMessageService messageService,
        IUsersService usersService,
        ISynThirdInfoService synThirdInfoService,
        IUserManager userManager)
    {
        _repository = messageTemplateRepository;
        _smsTemplateService = smsTemplateService;
        _sysConfigService = sysConfigService;
        _messageService = messageService;
        _usersService = usersService;
        _synThirdInfoService = synThirdInfoService;
        _userManager = userManager;
    }

    #region Get

    /// <summary>
    /// 获取base_message_template列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] MessageTemplateListQueryInput input)
    {
        var data = await _repository.Context.Queryable<MessageTemplateEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.CreatorUserId == b.Id))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), a => a.Category.Contains(input.keyword) || a.FullName.Contains(input.keyword) || a.Title.Contains(input.keyword))
            .Where(a => a.DeleteMark == null)
            .OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .Select((a, b) => new MessageTemplateListOutput
            {
                id = a.Id,
                category = SqlFunc.IF(a.Category.Equals("1")).Return("普通").ElseIF(a.Category.Equals("2")).Return("重要").End("紧急"),
                fullName = a.FullName,
                enCode = a.EnCode,
                title = a.Title,
                content = a.Content,
                _noticeMethod = SqlFunc.MergeString(SqlFunc.IIF(a.IsDingTalk == 1, "阿里钉钉,", string.Empty),
                    SqlFunc.IIF(a.IsEmail == 1, "电子邮箱,", string.Empty), SqlFunc.IIF(a.IsSms == 1, "短信,", string.Empty),
                    SqlFunc.IIF(a.IsStationLetter == 1, "站内信,", string.Empty), SqlFunc.IIF(a.IsWeCom == 1, "企业微信,", string.Empty)),
                enabledMark = a.EnabledMark,
                creatorTime = a.CreatorTime,
                lastModifyTime = a.LastModifyTime,
                creatorUser = SqlFunc.MergeString(b.RealName, "/", b.Account),
            }).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<MessageTemplateListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 获取base_message_template列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<dynamic> GetSelector([FromQuery] PageInputBase input)
    {
        var data = await _repository.Context.Queryable<MessageTemplateEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.CreatorUserId == b.Id))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), a => a.Category.Contains(input.keyword) || a.FullName.Contains(input.keyword) || a.Title.Contains(input.keyword))
            .Where(a => a.DeleteMark == null && a.EnabledMark == 1)
            .OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .Select((a, b) => new MessageTemplateSeletorOutput
            {
                id = a.Id,
                category = SqlFunc.IF(a.Category.Equals("1")).Return("普通").ElseIF(a.Category.Equals("2")).Return("重要").End("紧急"),
                fullName = a.FullName,
                enCode = a.EnCode,
                title = a.Title,
                content = a.Content,
                templateJson = a.TemplateJson,
                creatorTime = a.CreatorTime,
                lastModifyTime = a.LastModifyTime,
                creatorUser = SqlFunc.MergeString(b.RealName, "/", b.Account),
            }).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<MessageTemplateSeletorOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 获取base_message_template.
    /// </summary>
    /// <param name="id">参数.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo_Api(string id)
    {
        return await _repository.Context.Queryable<MessageTemplateEntity, SmsTemplateEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.SmsId == b.Id))
            .Where((a, b) => a.Id == id && a.DeleteMark == null && b.DeleteMark == null).Select((a, b) => new MessageTemplateInfoOutput()
            {
                id = a.Id,
                category = a.Category,
                isDingTalk = a.IsDingTalk,
                isEmail = a.IsEmail,
                isSms = a.IsSms,
                isStationLetter = a.IsStationLetter,
                isWecom = a.IsWeCom,
                fullName = a.FullName,
                enCode = a.EnCode,
                enabledMark = a.EnabledMark,
                title = a.Title,
                smsTemplateName = b.FullName,
                content = a.Content,
                smsId = a.SmsId,
                templateJson = a.TemplateJson,
            }).FirstAsync();
    }

    /// <summary>
    /// 获取base_message_template.
    /// </summary>
    /// <param name="id">参数.</param>
    /// <returns></returns>
    [HttpGet("getTemplate/{id}")]
    public async Task<dynamic> GetTemplate(string id)
    {
        var entity = await _repository.FirstOrDefaultAsync(p => p.Id == id && p.DeleteMark == null);
        var smsFields = await _smsTemplateService.GetSmsTemplateFields(entity.SmsId);
        var dic = entity.TemplateJson.ToObject<Dictionary<string, string>>();
        foreach (var item in smsFields)
        {
            dic[item] = string.Empty;
        }

        return dic;
    }
    #endregion

    #region Post

    /// <summary>
    /// 新建base_message_template.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] MessageTemplateCrInput input)
    {
        if (await _repository.AnyAsync(x => (x.EnCode == input.enCode || x.FullName == input.fullName) && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1004);
        var entity = input.Adapt<MessageTemplateEntity>();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新base_message_template.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] MessageTemplateUpInput input)
    {
        if (await _repository.AnyAsync(x => x.Id != id && (x.EnCode == input.enCode || x.FullName == input.fullName) && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1004);
        var entity = input.Adapt<MessageTemplateEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除base_message_template.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        if (!await _repository.AnyAsync(p => p.Id.Equals(id) && p.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1005);
        var isOk = await _repository.Context.Updateable<MessageTemplateEntity>().SetColumns(it => new MessageTemplateEntity()
        {
            DeleteMark = 1,
            DeleteUserId = _userManager.UserId,
            DeleteTime = SqlFunc.GetDate()
        }).Where(it => it.Id.Equals(id)).ExecuteCommandHasChangeAsync();
        if (!isOk) throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 修改单据规则状态.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/State")]
    public async Task ActionsState_Api(string id)
    {
        var isOk = await _repository.Context.Updateable<MessageTemplateEntity>().SetColumns(it => new MessageTemplateEntity()
        {
            EnabledMark = SqlFunc.IIF(it.EnabledMark == 1, 0, 1),
            LastModifyUserId = _userManager.UserId,
            LastModifyTime = SqlFunc.GetDate()
        }).Where(it => it.Id.Equals(id)).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1003);
    }

    #endregion

    #region PublicMethod

    /// <summary>
    /// 获取信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<MessageTemplateEntity> GetInfo(string id)
    {
        return await _repository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null);
    }

    /// <summary>
    /// 发送通知.
    /// </summary>
    /// <param name="typeList">推送方式</param>
    /// <param name="messageTemplateEntity">标题</param>
    /// <param name="userList">接收用户</param>
    /// <param name="parameters"></param>
    /// <param name="bodyDic"></param>
    /// <returns></returns>
    [NonAction]
    public async Task SendNodeMessage(List<string> typeList, MessageTemplateEntity messageTemplateEntity, List<string> userList, Dictionary<string, string> parameters, Dictionary<string, object> bodyDic)
    {
        var sysconfig = await _sysConfigService.GetInfo();
        var titile = messageTemplateEntity.Title;
        if (typeList.IsNotEmptyOrNull())
        {
            foreach (var item in typeList)
            {
                switch (item)
                {
                    case "1":
                        await _messageService.SentMessage(userList, titile, messageTemplateEntity.Content, bodyDic);
                        break;
                    case "2":
                        EmailSend(titile, userList, messageTemplateEntity.Content, sysconfig);
                        break;
                    case "3":
                        await SmsSend(messageTemplateEntity, userList, parameters, sysconfig);
                        break;
                    case "4":
                        var dingIds = await _synThirdInfoService.GetThirdIdList(userList, 2, 3);
                        if (dingIds.Count > 0)
                        {
                            var dingMsg = new { msgtype = "text", text = new { content = titile } }.ToJsonString();
                            DingWorkMessageParameter dingWorkMsgModel = new DingWorkMessageParameter()
                            {
                                toUsers = string.Join(",", dingIds),
                                agentId = sysconfig.dingAgentId,
                                msg = dingMsg
                            };
                            new DingUtil(sysconfig.dingSynAppKey, sysconfig.dingSynAppSecret).SendWorkMsg(dingWorkMsgModel);
                        }

                        break;
                    case "5":
                        var qyIds = await _synThirdInfoService.GetThirdIdList(userList, 1, 3);
                        var weChat = new WeChatUtil(sysconfig.qyhCorpId, sysconfig.qyhAgentSecret);
                        if (qyIds.Count > 0)
                        {
                            await weChat.SendText(sysconfig.qyhAgentId, titile, string.Join(",", qyIds));
                        }

                        break;
                }
            }
        }
    }

    #endregion

    #region PrivateMethod

    /// <summary>
    /// 邮箱.
    /// </summary>
    /// <param name="titile"></param>
    /// <param name="userList"></param>
    /// <param name="context"></param>
    /// <param name="sysconfig"></param>
    /// <returns></returns>
    private void EmailSend(string titile, List<string> userList, string context, SysConfigOutput sysconfig)
    {
        var emailList = new List<string>();
        foreach (var item in userList)
        {
            var user = _usersService.GetInfoByUserId(item);
            if (user.IsNotEmptyOrNull() && user.Email.IsNotEmptyOrNull())
            {
                emailList.Add(user.Email);
            }
        }

        var mailModel = new MailInfo();
        mailModel.To = string.Join(",", emailList);
        mailModel.Subject = titile;
        mailModel.BodyText = HttpUtility.HtmlDecode(context);
        MailUtil.Send(
            new MailParameterInfo
            {
                AccountName = sysconfig.emailSenderName,
                Account = sysconfig.emailAccount,
                Password = sysconfig.emailPassword,
                SMTPHost = sysconfig.emailSmtpHost,
                SMTPPort = sysconfig.emailSmtpPort.ParseToInt()
            }, mailModel);
    }

    /// <summary>
    /// 短信.
    /// </summary>
    /// <param name="messageTemplateEntity"></param>
    /// <param name="userList"></param>
    /// <param name="parameters"></param>
    /// <param name="sysconfig"></param>
    private async Task SmsSend(MessageTemplateEntity messageTemplateEntity, List<string> userList, Dictionary<string, string> parameters, SysConfigOutput sysconfig)
    {
        try
        {
            var telList = new List<string>();
            foreach (var item in userList)
            {
                var user = _usersService.GetInfoByUserId(item);
                if (user.IsNotEmptyOrNull() && user.MobilePhone.IsNotEmptyOrNull())
                {
                    telList.Add("+86" + user.MobilePhone);
                }
            }

            await _smsTemplateService.FlowTaskSend(messageTemplateEntity.SmsId, sysconfig, telList, parameters);
        }
        catch (Exception ex)
        {
        }
    }

    #endregion
}