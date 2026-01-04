using Microsoft.Extensions.DependencyInjection;
using MiniExcelLibs;
using QT.Common.Extension;
using QT.EventBus;
using QT.Logistics.Entitys;
using QT.Systems.Entitys.Permission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Logistics.Handler;

/// <summary>
/// 物流订单事件订阅
/// </summary>
public class LogisticsOrderSubscriber: IEventSubscriber
{
    private readonly IServiceProvider _serviceProvider;

    public LogisticsOrderSubscriber(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 下单后，创建会员信息.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe("Logistics:Order:CreateMember")]
    public async Task CreateMember(EventHandlerExecutingContext context)
    {
        if (context.Source.Payload is LogOrderEntity logOrderEntity)
        {
            List<LogMemberEntity> list = new List<LogMemberEntity>();
            using (var scope = _serviceProvider.CreateScope())
            {
                var _repository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<LogMemberEntity>>();
                if (!string.IsNullOrEmpty(logOrderEntity.ShipperPhone) && ! await _repository.AnyAsync(it => it.PhoneNumber == logOrderEntity.ShipperPhone))
                {
                    list.Add(new LogMemberEntity
                    {
                        Id = SnowflakeIdHelper.NextId(),
                        Name = logOrderEntity.ShipperName,
                        PhoneNumber = logOrderEntity.ShipperPhone,
                        Address = logOrderEntity.ShipperAddress
                    });
                }

                if (!string.IsNullOrEmpty(logOrderEntity.RecipientPhone) && !await _repository.AnyAsync(it => it.PhoneNumber == logOrderEntity.RecipientPhone))
                {
                    list.Add(new LogMemberEntity
                    {
                        Id = SnowflakeIdHelper.NextId(),
                        Name = logOrderEntity.RecipientName,
                        PhoneNumber = logOrderEntity.RecipientPhone,
                        Address = logOrderEntity.RecipientAddress
                    });
                }
                if (list.IsAny())
                {
                    await _repository.Context.Insertable(list).ExecuteCommandAsync();
                }

                var members = await _repository
                    .Where(it => it.PhoneNumber == logOrderEntity.ShipperPhone || it.PhoneNumber == logOrderEntity.RecipientPhone)
                    .Select(it=>it.Id)
                    .ToListAsync();
                // 创建配送点会员关系
                if (members.IsAny())
                {
                    var ls = await _repository.Context.Queryable<LogDeliveryMemberEntity>()
                        .Where(it => members.Contains(it.MemberId) && it.PointId == logOrderEntity.SendPointId)
                        .Select(it => it.MemberId).ToListAsync();

                    var insertList = members.Except(ls).Select(it => new LogDeliveryMemberEntity
                    {
                        Id = SnowflakeIdHelper.NextId(),
                        MemberId = it,
                        PointId = logOrderEntity.SendPointId
                    }).ToList();
                    if (insertList.IsAny())
                    {
                        await _repository.Context.Insertable<LogDeliveryMemberEntity>(insertList).ExecuteCommandAsync();
                    }
                   
                }


                // 暂时不需要创建系统账号
                //if (list.IsAny())
                //{
                //    await _repository.Context.Insertable(list).ExecuteCommandAsync();

                //    var roleId = await _repository.Context.Queryable<RoleEntity>().Where(it => it.EnCode == "VIP").Select(it => it.Id).FirstAsync() ?? "";
                //    var orgId = await _repository.Context.Queryable<OrganizeEntity>().Where(it => it.ParentId == "-1" && it.Category == "company").Select(it => it.Id).FirstAsync() ?? "";
                //    List<UserRelationEntity> userRelationEntities = new List<UserRelationEntity>();
                //    // 创建会员用户账号
                //    foreach (var item in list)
                //    {
                //        if (!await _repository.Context.Queryable<UserEntity>().AnyAsync(it => it.Account == item.PhoneNumber))
                //        {
                //            UserEntity user = new UserEntity
                //            {
                //                Id = item.Id,
                //                Account = item.PhoneNumber,
                //                NickName = item.Name,
                //                RealName = item.Name,
                //                Gender = 3, //保密
                //                RoleId = roleId,
                //                LastRoleId = roleId,
                //                OrganizeId = orgId,
                //                EnabledMark = 1
                //            };
                //            await _repository.Context.Insertable<UserEntity>(user).ExecuteCommandAsync();

                //            if (!string.IsNullOrEmpty(roleId))
                //            {
                //                userRelationEntities.Add(new UserRelationEntity
                //                {
                //                    Id = SnowflakeIdHelper.NextId(),
                //                    ObjectId = roleId,
                //                    ObjectType = "Role",
                //                    UserId = item.Id
                //                });
                //            }
                //            if (!string.IsNullOrEmpty(orgId))
                //            {
                //                userRelationEntities.Add(new UserRelationEntity
                //                {
                //                    Id = SnowflakeIdHelper.NextId(),
                //                    ObjectId = orgId,
                //                    ObjectType = "Organize",
                //                    UserId = item.Id
                //                });
                //            }
                //        }
                //    }

                //    if (userRelationEntities.IsAny())
                //    {
                //        await _repository.Context.Insertable<UserRelationEntity>(userRelationEntities).ExecuteCommandAsync();
                //    }
                //}
            }
        }
    }


    /// <summary>
    /// 记录车辆位置信息.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe("Logistics:VehicleStatus:Create")]
    public async Task LogVehicleStatus(EventHandlerExecutingContext context)
    {
        if (context.Source.Payload is LogVehicleStatusEntity entity)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _repository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<LogVehicleStatusEntity>>();

                //判断同一个车，同一个配送点，5分钟之内是否有做过车辆记录，没有才记录当前数据
                DateTime dateTime = DateTime.Now.AddMinutes(-5); // 5分钟前
                var record = await _repository.Where(it => it.VId == entity.VId && it.PointId == entity.PointId && it.CollectionTime >= dateTime).FirstAsync();

                if (record == null)
                {
                    entity.Id = SnowflakeIdHelper.NextId();
                    entity.CollectionTime = DateTime.Now;
                    await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn:true).ExecuteCommandAsync();
                }
            }
        }
    }
}
