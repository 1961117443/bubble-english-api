using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QT.Common.Configuration;
using QT.Common.Const;
using QT.Common.Core.Manager.Tenant;
using QT.Common.Core.Security;
using QT.Common.Extension;
using QT.Common.Options;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.EventBus;
using QT.EventHandler;
using QT.Systems.Entitys.System;
using QT.TaskScheduler.Entitys.Dto.TaskScheduler;
using QT.TaskScheduler.Interfaces.TaskScheduler;
using Serilog;
using SqlSugar;

namespace QT.TaskScheduler.Listener;

/// <summary>
/// 定时任务监听器.
/// </summary>
public class SpareTimeListener : ISpareTimeListener, ISingleton
{
    private readonly IEventPublisher _eventPublisher;
    public IServiceProvider Services { get; }
    private readonly ConnectionStringsOptions _connectionStrings;

    /// <summary>
    /// 构造函数.
    /// </summary>
    public SpareTimeListener(IEventPublisher eventPublisher, IServiceProvider services, IOptions<ConnectionStringsOptions> connectionOptions)
    {
        _eventPublisher = eventPublisher;
        Services = services;
        _connectionStrings = connectionOptions.Value;
        subscribe();
    }

    /// <summary>
    /// 监听所有任务.
    /// </summary>
    /// <param name="executer"></param>
    /// <returns></returns>
    public async Task OnListener(SpareTimerExecuter executer)
    {
        switch (executer.Status)
        {
            // 执行开始通知
            case 0:
                // Console.WriteLine($"{executer.Timer.WorkerName} 任务开始通知");
                break;

            // 任务执行之前通知
            case 1:
                // Console.WriteLine($"{executer.Timer.WorkerName} 执行之前通知");
                break;

            // 执行成功通知
            case 2:

            // 任务执行失败通知
            case 3:
                await RecoreTaskLog(executer);
                break;

            // 任务执行停止通知
            case -1:
                // Console.WriteLine($"{executer.Timer.WorkerName} 执行停止通知");
                await StopTaskLog(executer);
                break;

            // 任务执行取消通知
            case -2:
                // Console.WriteLine($"{executer.Timer.WorkerName} 执行取消通知");
                break;
            case 99:
                //Console.WriteLine($"{executer.Timer.WorkerName} 检查执行节点");
                await CheckRunningNode(executer);
                break;
        }
    }

    /// <summary>
    /// 记录日志
    /// </summary>
    /// <param name="executer"></param>
    /// <returns></returns>
    private async Task RecoreTaskLog(SpareTimerExecuter executer)
    {
        if (executer.Timer.Description.IsNotEmptyOrNull())
        {
            var arr = executer.Timer.Description.Split("/");
            var TenantId = arr[0];
            var TenantDbName = arr.Length > 1 ? arr[1] : "";

            // 是否忽略日志
            var ignoreLog = arr.Contains("TaskLog=0") || executer.Timer.Description.IndexOf("TaskLog=0") > -1;

            if (executer.Timer.Type == SpareTimeTypes.Cron)
            {
                using var scope = Services.CreateScope();
                var _repository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<TimeTaskEntity>>();
                if (KeyVariable.MultiTenancy)
                {
                    if (TenantId == null) return;
                    if (TenantManager.Login(TenantId, _repository.Context))
                    {
                        _repository.Context.ChangeDatabase(TenantId);
                    }
                    else
                    {
                        return;
                    }
                    //_repository.Context.AddConnection(new ConnectionConfig()
                    //{
                    //    DbType = (DbType)Enum.Parse(typeof(DbType), _connectionStrings.DBType),
                    //    ConfigId = TenantId, // 设置库的唯一标识
                    //    IsAutoCloseConnection = true,
                    //    ConnectionString = string.Format(_connectionStrings.DefaultConnection, TenantDbName)
                    //});
                    //_repository.Context.ChangeDatabase(TenantId);
                }

                var taskEntity = await _repository.FirstOrDefaultAsync(x => x.Id == executer.Timer.WorkerName);

                if (taskEntity != null)
                {
                    var nextRunTime = ((DateTimeOffset)SpareTime.GetCronNextOccurrence(taskEntity.ExecuteCycleJson)).DateTime;

                    await _eventPublisher.PublishAsync(new TaskEventSource("Task:UpdateTask", TenantId, TenantDbName, new TimeTaskEntity()
                    {
                        Id = taskEntity.Id,
                        NextRunTime = nextRunTime,
                    }));
                }
            }

            if (!ignoreLog)
            {
                await _eventPublisher.PublishAsync(new TaskLogEventSource("Log:CreateTaskLog", TenantId, TenantDbName, new TimeTaskLogEntity()
                {
                    Id = SnowflakeIdHelper.NextId(),
                    TaskId = executer.Timer.WorkerName,
                    RunTime = DateTime.Now.AddSeconds(10),
                    RunResult = executer.Status == 2 ? 0 : 1,
                    Description = executer.Status == 2 ? "执行成功" : "执行失败,失败原因:" + executer.Timer.Exception.ToJsonString()
                }));
            }
        }
    }


    /// <summary>
    /// 判断是否在当前节点执行
    /// </summary>
    /// <param name="executer"></param>
    /// <returns></returns>
    private async Task CheckRunningNode(SpareTimerExecuter executer)
    {
        executer.Timer.RunningOnCurrentNode = true;
        if (!string.IsNullOrEmpty(executer.Timer.WorkerName) && KeyVariable.CacheType == Common.Cache.CacheType.RedisCache)
        {
            using (var lok = RedisHelper.Lock(executer.Timer.WorkerName, 5))
            {
                var key = "ClusterSpareTime";

                if (!RedisHelper.HExists(key, executer.Timer.WorkerName))
                {
                    RedisHelper.HSet(key, executer.Timer.WorkerName, SnowflakeIdHelper.CurrentWorkerId());
                }
                else
                {
                    var workerid = RedisHelper.HGet(key, executer.Timer.WorkerName);
                    if (workerid != SnowflakeIdHelper.CurrentWorkerId())
                    {
                        //  判断绑定的机器码是否工作，如果不工作，用当前机器码代替，否则跳过不执行
                        var caCheKey = CommonConst.CACHEKEYWORKERID + workerid;

                        // 判断 caCheKey 是否存在并且有效
                        if (!RedisHelper.Exists(caCheKey))
                        {
                            RedisHelper.HSet(key, executer.Timer.WorkerName, SnowflakeIdHelper.CurrentWorkerId());
                        }
                        else
                        {
                            // 判断机器是否还有效
                            var uuid = Guid.NewGuid().ToString();
                            var msgid = RedisHelper.Publish($"helpcheck_{caCheKey}", uuid);
                            await Task.Delay(100);

                            if (RedisHelper.Exists($"{workerid}:{uuid}"))
                            {
                                // 服务在线，本服务不执行
                                //Console.WriteLine($"{executer.Timer.WorkerName} 执行之前通知，机器码不一致");
                                //Console.WriteLine($"{workerid} 在线，{SnowflakeIdHelper.CurrentWorkerId()} 不执行当前任务 {executer.Timer.WorkerName}");
                                executer.Timer.RunningOnCurrentNode = false;
                            }
                            else
                            {
                                // 服务不在线，删除绑定，重新绑定本机执行
                                RedisHelper.HSet(key, executer.Timer.WorkerName, SnowflakeIdHelper.CurrentWorkerId());
                                //Console.WriteLine($"{workerid} 服务不在线，删除绑定，重新绑定本机 {SnowflakeIdHelper.CurrentWorkerId()} 执行");
                            }

                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 订阅消息，集群定时任务处理
    /// </summary>
    private void subscribe()
    {
        Log.Information("定时任务监听器");
        if (KeyVariable.CacheType == Common.Cache.CacheType.RedisCache)
        {
            RedisHelper.Subscribe(("timer_task_add", msg =>
            {
                if (msg.Body.IsNotEmptyOrNull())
                {
                    var body = msg.Body.ToObject<TimeTaskSyncModel>();
                    if (body.WorkerId != SnowflakeIdHelper.CurrentWorkerId())
                    {
                        Console.WriteLine("新增定时任务:{0}", msg.Body);
                        TenantScoped.Create(body.TenantId, (factory, scope) =>
                        {
                            var service = scope.ServiceProvider.GetService<ITimeTaskService>();
                            service?.AddTimerJob(body.Data);
                        });
                    }
                }
            }
            ), ("timer_task_update", msg =>
            {
                if (msg.Body.IsNotEmptyOrNull())
                {
                    var body = msg.Body.ToObject<TimeTaskSyncModel>();
                    if (body.WorkerId != SnowflakeIdHelper.CurrentWorkerId())
                    {
                        Console.WriteLine("更新定时任务:{0}", msg.Body);
                        // 先从调度器里取消
                        SpareTime.Cancel(body.Data.Id);
                        // 再添加到任务调度里
                        if (body.Data.EnabledMark == 1)
                        {
                            TenantScoped.Create(body.TenantId, (factory, scope) =>
                            {
                                var service = scope.ServiceProvider.GetService<ITimeTaskService>();
                                service?.AddTimerJob(body.Data);
                            });
                        }
                    }
                }
            }
            ), ("timer_task_del", msg =>
            {
                if (msg.Body.IsNotEmptyOrNull())
                {
                    var body = msg.Body.ToObject<TimeTaskSyncModel>();
                    if (body.WorkerId != SnowflakeIdHelper.CurrentWorkerId())
                    {
                        Console.WriteLine("删除定时任务:{0}", msg.Body);
                        // 先从调度器里取消
                        SpareTime.Cancel(body.Data.Id);
                    }
                }
            }
            ), ("timer_task_stop", msg =>
            {
                if (msg.Body.IsNotEmptyOrNull())
                {
                    var body = msg.Body.ToObject<TimeTaskSyncModel>();
                    if (body.WorkerId != SnowflakeIdHelper.CurrentWorkerId())
                    {
                        Console.WriteLine("停止定时任务:{0}", msg.Body);
                        // 先从调度器里取消
                        SpareTime.Stop(body.Data.Id);
                    }
                }
            }
            ), ("timer_task_enable", msg =>
            {
                if (msg.Body.IsNotEmptyOrNull())
                {
                    var body = msg.Body.ToObject<TimeTaskSyncModel>();
                    if (body.WorkerId != SnowflakeIdHelper.CurrentWorkerId())
                    {
                        Console.WriteLine("启动定时任务:{0}", msg.Body);
                        var timer = SpareTime.GetWorker(body.Data.Id);
                        if (timer == null)
                        {
                            TenantScoped.Create(body.TenantId, (factory, scope) =>
                            {
                                var service = scope.ServiceProvider.GetService<ITimeTaskService>();
                                service?.AddTimerJob(body.Data);
                            });
                        }
                        else
                        {
                            // 如果 StartNow 为 flase , 执行 AddTimerJob 并不会启动任务
                            SpareTime.Start(body.Data.Id);
                        }
                    }
                }
            }
            ));
        }
    }

    /// <summary>
    /// 记录日志
    /// </summary>
    /// <param name="executer"></param>
    /// <returns></returns>
    private async Task StopTaskLog(SpareTimerExecuter executer)
    {
        if (executer.Timer.Description.IsNotEmptyOrNull())
        {
            var arr = executer.Timer.Description.Split("/");
            var TenantId = arr[0];
            var TenantDbName = arr.Length > 1 ? arr[1] : "";

            // 是否忽略日志
            var ignoreLog = arr.Contains("TaskLog=0") || executer.Timer.Description.IndexOf("TaskLog=0") > -1;

            // 异常停止，记录日志
            if (executer.Timer.Exception.Count > 0)
            {
                await _eventPublisher.PublishAsync(new TaskLogEventSource("Log:CreateTaskLog", TenantId, TenantDbName, new TimeTaskLogEntity()
                {
                    Id = SnowflakeIdHelper.NextId(),
                    TaskId = executer.Timer.WorkerName,
                    RunTime = DateTime.Now.AddSeconds(10),
                    RunResult = executer.Status == 2 ? 0 : 1,
                    Description = executer.Status == 2 ? "执行成功" : "执行失败,失败原因:" + executer.Timer.Exception.ToJsonString()
                }));
            }


        }
    }
}
