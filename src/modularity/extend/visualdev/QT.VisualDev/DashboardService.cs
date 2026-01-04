using Mapster;
using Microsoft.AspNetCore.Mvc;
using QT.Common.Core.Manager;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Message.Entitys;
using QT.Message.Interfaces.Message;
using QT.VisualDev.Entitys.Dto.Dashboard;
using QT.VisualDev.Entitys.Dto.Email;
using QT.VisualDev.Entitys.Entity;
using QT.WorkFlow.Entitys.Entity;
using QT.WorkFlow.Interfaces.Repository;
using SqlSugar;

namespace QT.VisualDev;

/// <summary>
///  业务实现：主页显示.
/// </summary>
[ApiDescriptionSettings(Tag = "VisualDev", Name = "Dashboard", Order = 174)]
[Route("api/visualdev/[controller]")]
public class DashboardService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<EmailReceiveEntity> _emailReceiveRepository;

    /// <summary>
    /// 流程任务.
    /// </summary>
    private readonly IFlowTaskRepository _flowTaskRepository;

    /// <summary>
    /// 系统消息服务.
    /// </summary>
    private readonly IMessageService _messageService;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="DashboardService"/>类型的新实例.
    /// </summary>
    public DashboardService(
        ISqlSugarRepository<EmailReceiveEntity> emailReceiveRepository,
        IFlowTaskRepository flowTaskRepository,
        IMessageService messageService,
        IUserManager userManager)
    {
        _emailReceiveRepository = emailReceiveRepository;
        _flowTaskRepository = flowTaskRepository;
        _messageService = messageService;
        _userManager = userManager;
    }

    #region Get

    /// <summary>
    /// 获取我的待办.
    /// </summary>
    [HttpGet("FlowTodoCount")]
    public async Task<dynamic> GetFlowTodoCount()
    {
        int flowCount = await _emailReceiveRepository.Context.Queryable<FlowDelegateEntity>().Where(x => x.CreatorUserId == _userManager.UserId && x.DeleteMark == null).CountAsync();
        List<FlowTaskEntity>? waitList = await _flowTaskRepository.GetWaitList();
        List<FlowTaskEntity>? trialList = await _flowTaskRepository.GetTrialList();
        return new FlowTodoCountOutput()
        {
            toBeReviewed = waitList.Count(),
            entrust = flowCount,
            flowDone = trialList.Count()
        };
    }

    /// <summary>
    /// 获取通知公告.
    /// </summary>
    [HttpGet("Notice")]
    public async Task<dynamic> GetNotice()
    {
        List<NoticeOutput> list = await _emailReceiveRepository.Context.Queryable<MessageEntity, MessageReceiveEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.Id == b.MessageId))
            .Where((a, b) => a.Type == 1 && a.DeleteMark == null && b.UserId == _userManager.UserId)
            .Select((a) => new NoticeOutput()
            {
                id = a.Id,
                fullName = a.Title,
                creatorTime = a.CreatorTime
            }).ToListAsync();

        return new { list = list };
    }

    /// <summary>
    /// 获取待办事项.
    /// </summary>
    [HttpGet("FlowTodo")]
    public async Task<dynamic> GetFlowTodo()
    {
        dynamic list = await _flowTaskRepository.GetPortalWaitList();
        return new { list = list };
    }

    /// <summary>
    /// 获取我的待办事项.
    /// </summary>
    [HttpGet("MyFlowTodo")]
    public async Task<dynamic> GetMyFlowTodo()
    {
        List<FlowTodoOutput> list = new List<FlowTodoOutput>();
        (await _flowTaskRepository.GetWaitList()).ForEach(l =>
        {
            list.Add(new FlowTodoOutput()
            {
                id = l.Id,
                fullName = l.FlowName,
                creatorTime = l.CreatorTime
            });
        });
        return new { list = list };
    }

    /// <summary>
    /// 获取未读邮件.
    /// </summary>
    [HttpGet("Email")]
    public async Task<dynamic> GetEmail()
    {
        List<EmailHomeOutput>? res = (await _emailReceiveRepository.AsQueryable().Where(x => x.Read == 0 && x.CreatorUserId == _userManager.UserId && x.DeleteMark == null)
            .OrderBy(x => x.CreatorTime, OrderByType.Desc).ToListAsync()).Adapt<List<EmailHomeOutput>>();
        return new { list = res };
    }

    #endregion
}
