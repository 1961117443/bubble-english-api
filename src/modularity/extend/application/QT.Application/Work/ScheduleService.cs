using QT.Common.Core.Manager;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Extend.Entitys;
using QT.Extend.Entitys.Dto.Schedule;
using QT.FriendlyException;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Common.Contracts;
using Serilog;
using QT.Message.Entitys;
using QT.Message.Handlers;
using QT.Common.Const;
using QT.Common.Core.Security;
using Microsoft.AspNetCore.Identity;
using Npgsql.TypeHandlers.DateTimeHandlers;
using QT.Common.Core.Manager.Tenant;
using QT.Common.Security;

namespace QT.Extend
{
    /// <summary>
    /// 项目计划
    /// </summary>
    [ApiDescriptionSettings(Tag = "Extend", Name = "Schedule", Order = 600)]
    [Route("api/extend/[controller]")]
    public class ScheduleService : IDynamicApiController, ITransient
    {
        private readonly ISqlSugarRepository<ScheduleEntity> _scheduleRepository;
        private readonly IUserManager _userManager;

        public ScheduleService(
            ISqlSugarRepository<ScheduleEntity> scheduleRepository,
            IUserManager userManager)
        {
            _scheduleRepository = scheduleRepository;
            _userManager = userManager;
        }

        #region GET

        /// <summary>
        /// 列表.
        /// </summary>
        /// <param name="input">参数</param>
        /// <returns></returns>
        [HttpGet("")]
        public async Task<dynamic> GetList([FromQuery] ScheduleListQuery input)
        {
            var data = await _scheduleRepository.AsQueryable()
                .Where(x => x.CreatorUserId == _userManager.UserId && x.StartTime >= input.startTime.ParseToDateTime() && x.EndTime <= input.endTime.ParseToDateTime() && x.DeleteMark == null).OrderBy(x => x.StartTime, OrderByType.Desc).ToListAsync();
            var output = data.Adapt<List<ScheduleListOutput>>();
            return new { list = output };
        }

        /// <summary>
        /// 信息.
        /// </summary>
        /// <param name="id">主键值</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<dynamic> GetInfo(string id)
        {
            return (await _scheduleRepository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null)).Adapt<ScheduleInfoOutput>();
        }

        /// <summary>
        /// app.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet("AppList")]
        public async Task<dynamic> GetAppList([FromQuery] ScheduleListQuery input)
        {
            var days = new Dictionary<string, int>();
            var data = await _scheduleRepository.AsQueryable()
                .Where(x => x.CreatorUserId == _userManager.UserId && x.StartTime >= input.startTime.ParseToDateTime() && x.EndTime <= input.endTime.ParseToDateTime() && x.DeleteMark == null)
                .OrderBy(x => x.StartTime, OrderByType.Desc).ToListAsync();
            var output = data.Adapt<List<ScheduleListOutput>>();
            foreach (var item in GetAllDays(input.startTime.ParseToDateTime(), input.endTime.ParseToDateTime()))
            {
                var _startTime = item.ToString("yyyy-MM-dd") + " 23:59";
                var _endTime = item.ToString("yyyy-MM-dd") + " 00:00";
                var count = output.FindAll(m => m.startTime <= _startTime.ParseToDateTime() && m.endTime >= _endTime.ParseToDateTime()).Count;
                days.Add(item.ToString("yyyyMMdd"), count);
            }
            var today_startTime = input.dateTime + " 23:59";
            var today_endTime = input.dateTime + " 00:00";
            return new
            {
                signList = days,
                todayList = output.FindAll(m => m.startTime <= today_startTime.ParseToDateTime() && m.endTime >= today_endTime.ParseToDateTime())
            };
        }

        #endregion

        #region POST

        /// <summary>
        /// 新建.
        /// </summary>
        /// <param name="input">实体对象</param>
        /// <returns></returns>
        [HttpPost("")]
        public async Task Create([FromBody] ScheduleCrInput input)
        {
            var entity = input.Adapt<ScheduleEntity>();
            var isOk = await _scheduleRepository.Context.Insertable(entity).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
            if (isOk < 1)
                throw Oops.Oh(ErrorCode.COM1000);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="id">主键值</param>
        /// <param name="input">实体对象</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task Update(string id, [FromBody] ScheduleUpInput input)
        {
            var entity = input.Adapt<ScheduleEntity>();
            var isOk = await _scheduleRepository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
            if (isOk < 1)
                throw Oops.Oh(ErrorCode.COM1001);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id">主键值</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task Delete(string id)
        {
            var entity = await _scheduleRepository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null);
            if (entity == null)
                throw Oops.Oh(ErrorCode.COM1005);
            var isOk = await _scheduleRepository.Context.Context.Updateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();
            if (isOk < 1)
                throw Oops.Oh(ErrorCode.COM1002);
        }

        #endregion

        #region PrivateMethod

        /// <summary>
        /// 获取固定日期范围内的所有日期，以数组形式返回.
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        private DateTime[] GetAllDays(DateTime startTime, DateTime endTime)
        {
            var listDay = new List<DateTime>();
            DateTime dtDay = new DateTime();
            //循环比较，取出日期；
            for (dtDay = startTime; dtDay.CompareTo(endTime) <= 0; dtDay = dtDay.AddDays(1))
            {
                listDay.Add(dtDay);
            }
            return listDay.ToArray();
        }

        #endregion
    }
}

/// <summary>
/// 项目计划 定时任务
/// </summary>
public class ScheduleOneMinuteActuator : IOneMinuteActuator, IScoped
{
    private readonly ISqlSugarRepository<ScheduleEntity> _repository;
    private readonly IMHandler _iMHandler;

    public ScheduleOneMinuteActuator(ISqlSugarRepository<ScheduleEntity> repository, IMHandler iMHandler)
    {
        _repository = repository;
        _iMHandler = iMHandler;
    }
    public async Task Execute()
    {
        //Log.Information($"TestOneMinuteActuator:{_repository.Context.CurrentConnectionConfig.ConfigId}");
        //await Task.CompletedTask;

        // 找出当前租户需要提醒的数据
        var list = await _repository.Where(x => x.Early > 0)
            .Where(x => DateTime.Now <= x.StartTime && SqlFunc.DateDiff(DateType.Hour, DateTime.Now, x.StartTime.Value) < x.Early)
            .Where(x => SqlFunc.Subqueryable<MessageEntity>().Where(ddd => ddd.Id == x.Id).NotAny())
            .ToListAsync();

        if (list.IsAny())
        {
            foreach (var item in list)
            {
                MessageEntity messageEntity = new()
                {
                    Id = item.Id,
                    Title = item.Content,
                    BodyText = item.Content,
                    ToUserIds = item.CreatorUserId,
                    Type = 1,
                    EnabledMark = 1,
                    CreatorTime = DateTime.Now,
                    CreatorUserId = CommonConst.SUPPER_ADMIN_ID,
                    LastModifyTime = DateTime.Now,
                    LastModifyUserId = CommonConst.SUPPER_ADMIN_ID,
                };

                List<MessageReceiveEntity> receiveEntityList = messageEntity.ToUserIds.Split(",")
                .Select(x => new MessageReceiveEntity()
                {
                    Id = SnowflakeIdHelper.NextId(),
                    MessageId = messageEntity.Id,
                    UserId = x,
                    IsRead = 0,
                    BodyText = messageEntity.BodyText,
                }).ToList();

                _repository.Context.Insertable<MessageEntity>(messageEntity).IgnoreColumns(ignoreNullColumn: true).AddQueue();
                _repository.Context.Insertable<MessageReceiveEntity>(receiveEntityList).IgnoreColumns(ignoreNullColumn: true).AddQueue();

                var isOk = await _repository.Context.SaveQueuesAsync();
                if (isOk > 0)
                {
                    // 发送推送消息
                    foreach (var receive in receiveEntityList)
                    {
                        await _iMHandler.SendMessageToUserAsync(string.Format("{0}-{1}", TenantScoped.TenantId, receive.UserId), 
                            new { method = "messagePush", messageType = 2, userId = messageEntity.CreatorUserId, toUserId = new string[] { receive.UserId }, title = messageEntity.Title, unreadNoticeCount = 1 }.ToJsonString());
                    }
                }
            }
        }
    }
}