using AI.BubbleEnglish.Dto;
using AI.BubbleEnglish.Entitys;
using AI.BubbleEnglish.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using QT;
using QT.Common.Const;
using QT.Common.Security;
using QT.DynamicApiController;
using QT.FriendlyException;
using SqlSugar;

namespace AI.BubbleEnglish.Controller;

/// <summary>
/// 孩子档案（多孩子）
/// </summary>
[ApiDescriptionSettings(ModuleConst.BubbleEnglish, Tag = "BubbleEnglish", Name = "Child", Order = 20)]
[Route("api/bubbleEnglish/parent")]
public class ChildController : IDynamicApiController
{
    private readonly ISqlSugarRepository<BubbleChildProfileEntity> _childRepo;
    private readonly ISqlSugarRepository<BubbleUserEntity> _userRepo;

    public ChildController(ISqlSugarRepository<BubbleChildProfileEntity> childRepo, ISqlSugarRepository<BubbleUserEntity> userRepo)
    {
        _childRepo = childRepo;
        _userRepo = userRepo;
    }

    private static long GetParentId()
    {
        var idObj = App.User?.FindFirst(ClaimConst.CLAINMUSERID)?.Value;
        if (long.TryParse(idObj, out var id)) return id;
        throw Oops.Oh("未登录");
    }

    /// <summary>
    /// 获取当前家长的孩子列表
    /// </summary>
    [HttpGet("children")]
    public async Task<List<ChildDto>> GetChildren()
    {
        var parentId = GetParentId();
        var list = await _childRepo.AsQueryable().Where(x => x.ParentId == parentId).OrderByDescending(x => x.Id).ToListAsync();
        return list.Select(ToDto).ToList();
    }

    /// <summary>
    /// 创建孩子档案
    /// </summary>
    [HttpPost("children")]
    public async Task<ChildDto> CreateChild([FromBody] ChildCreateReq req)
    {
        var parentId = GetParentId();
        if (string.IsNullOrWhiteSpace(req.Name)) throw Oops.Oh("孩子名称不能为空");

        var entity = new BubbleChildProfileEntity
        {
            ParentId = parentId,
            Name = req.Name.Trim(),
            Avatar = req.Avatar ?? string.Empty,
            BirthYearMonth = string.IsNullOrWhiteSpace(req.BirthYearMonth) ? null : req.BirthYearMonth,
            CreateTime = DateTime.Now,
        };

        if (!string.IsNullOrWhiteSpace(entity.BirthYearMonth))
        {
            entity.Age = BubbleEnglishHelpers.CalcAgeFromBirthYearMonth(entity.BirthYearMonth);
        }
        else
        {
            entity.Age = req.Age ?? 0;
        }

        entity.Id = await _childRepo.InsertReturnIdentityAsync(entity);
        return ToDto(entity);
    }

    /// <summary>
    /// 设置当前孩子（占位：目前只返回 ok，后续可写入 parent_profile）
    /// </summary>
    [HttpPost("current-child")]
    public Task<object> SetCurrentChild([FromBody] SetCurrentChildReq req)
    {
        // TODO: 将 currentChildId 写入 profile 表
        return Task.FromResult<object>(new { ok = true, childId = req.ChildId });
    }

    private static ChildDto ToDto(BubbleChildProfileEntity e)
    {
        var age = e.Age;
        if (!string.IsNullOrWhiteSpace(e.BirthYearMonth))
        {
            age = BubbleEnglishHelpers.CalcAgeFromBirthYearMonth(e.BirthYearMonth);
        }

        return new ChildDto
        {
            Id = e.Id,
            ParentId = e.ParentId,
            Name = e.Name,
            Age = age,
            BirthYearMonth = e.BirthYearMonth,
            AgeBand = BubbleEnglishHelpers.AgeBandFromAge(age),
            Avatar = string.IsNullOrWhiteSpace(e.Avatar) ? null : e.Avatar,
        };
    }
}
