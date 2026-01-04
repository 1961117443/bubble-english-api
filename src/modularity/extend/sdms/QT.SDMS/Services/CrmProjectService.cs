//using Microsoft.AspNetCore.Mvc;
//using QT.Common.Const;
//using QT.Common.Core;
//using QT.Common.Core.Manager;
//using QT.Common.Core.Security;
//using QT.Common.Extension;
//using QT.DynamicApiController;
//using QT.Iot.Application.Dto.CrmProject;
//using QT.Iot.Application.Entity;
//using QT.Systems.Interfaces.System;
//using SqlSugar;
//using Yitter.IdGenerator;

//namespace QT.Iot.Application.Service;

///// <summary>
///// 业务实现：项目管理.
///// </summary>
//[ApiDescriptionSettings("营销管理", Tag = "项目管理", Name = "CrmProject", Order = 300)]
//[Route("api/iot/crm/project")]
//public class CrmProjectService : QTBaseService<CrmProjectEntity, CrmProjectCrInput, CrmProjectUpInput, CrmProjectOutput, CrmProjectListQueryInput, CrmProjectListOutput>, IDynamicApiController
//{
//    private readonly IBillRullService _billRullService;

//    public CrmProjectService(ISqlSugarRepository<CrmProjectEntity> repository, ISqlSugarClient context, IUserManager userManager, IBillRullService billRullService) : base(repository, context, userManager)
//    {
//        _billRullService = billRullService;
//    }

//    protected override async Task<SqlSugarPagedList<CrmProjectListOutput>> GetPageList([FromQuery] CrmProjectListQueryInput input)
//    {
//        //List<DateTime> queryInTime = input.orderDate?.Split(',').ToObject<List<DateTime>>();
//        //DateTime? startInTime = queryInTime?.First();
//        //DateTime? endInTime = queryInTime?.Last();

//        return await _repository.Context.Queryable<CrmProjectEntity>()
//            .WhereIF(!_userManager.IsAdministrator, it=> SqlFunc.Subqueryable<CrmProjectUserEntity>().Where(ddd=>ddd.Pid == it.Id && ddd.Uid == _userManager.UserId).Any())
//            //.WhereIF(!string.IsNullOrEmpty(input.keyword), it => it.No.Contains(input.keyword))
//            //.WhereIF(queryInTime != null, it => SqlFunc.Between(it.OrderDate, startInTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endInTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
//            .Select<CrmProjectListOutput>(it => new CrmProjectListOutput
//            {
//                //userIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == it.UserId).Select(ddd => ddd.RealName)
//                id = it.Id,
//                name = it.Name,
//                adminName = it.AdminName,
//                adminTel = it.AdminTel,
//                remark = it.Remark,
//            }, true)
//            .ToPagedListAsync(input.currentPage, input.pageSize);
//    }


//    /// <summary>
//    /// 获取关联用户
//    /// </summary>
//    /// <returns></returns>
//    [HttpGet("actions/{id}/userRelation")]
//    public async Task<dynamic> UserRelationList(string id)
//    {
//        var list = await _repository.Context.Queryable<CrmProjectUserEntity>().Where(it => it.Pid == id).ToListAsync();
//        var userList = list.Select(x => x.Uid).ToList();

//        return new
//        {
//            id = id,
//            userList
//        };
//    }

//    /// <summary>
//    /// 保存关联用户关系
//    /// </summary>
//    /// <returns></returns>
//    [HttpPost("actions/{id}/userRelation")]
//    [SqlSugarUnitOfWork]
//    public async Task UserRelationList(string id, [FromBody] List<string> userList)
//    {
//        // 当前关联的集合
//        var list = await _repository.Context.Queryable<CrmProjectUserEntity>().Where(it => it.Pid == id).ToListAsync();
         
//        List<CrmProjectUserEntity> items = new List<CrmProjectUserEntity>();
//        foreach (var item in userList)
//        {
//            var dbEntity = list.Find(x => x.Uid == item);
//            if (dbEntity != null)
//            {
//                // 已经包含
//                list.Remove(dbEntity);
//            }
//            else
//            {
//                items.Add(new CrmProjectUserEntity
//                {
//                    Id = SnowflakeIdHelper.NextId(),
//                    Pid = id,
//                    Uid = item,
//                });
//            }
//        }

//        if (list.IsAny())
//        {
//            await _repository.Context.Deleteable<CrmProjectUserEntity>(list).ExecuteCommandAsync();
//        }

//        if (items.IsAny())
//        {
//            await _repository.Context.Insertable<CrmProjectUserEntity>(items).ExecuteCommandAsync();
//        }
//    }
//}