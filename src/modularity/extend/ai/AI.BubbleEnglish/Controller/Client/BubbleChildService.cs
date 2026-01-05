namespace AI.BubbleEnglish;

/// <summary>
/// 孩子档案（多孩子）
/// </summary>
[ApiDescriptionSettings(ModuleConst.BubbleEnglish, Tag = "BubbleEnglish", Name = "Parent", Order = 1010)]
[Route("api/bubble/client/[controller]")]
public class BubbleChildService : IDynamicApiController, ITransient
{
    private readonly SqlSugarClient _db;
    private readonly IUserManager _userManager;

    public BubbleChildService(ISqlSugarClient context, IUserManager userManager)
    {
        _db = (SqlSugarClient)context;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取当前家长的孩子列表
    /// </summary>
    [HttpGet("children")]
    public async Task<List<ChildOutput>> GetChildren()
    {
        string uid = _userManager.UserId;
        var list = await _db.Queryable<BubbleChildProfileEntity>()
            .Where(x => x.ParentId == uid)
            .OrderByDescending(x => x.CreateTime)
            .ToListAsync();

        return list.Select(ToOutput).ToList();
    }

    /// <summary>
    /// 创建孩子档案
    /// </summary>
    [HttpPost("children")]
    [Consumes("application/json")]
    public async Task<ChildOutput> CreateChild([FromBody] ChildCreateInput input)
    {
        if (string.IsNullOrWhiteSpace(input.name)) throw Oops.Oh("孩子昵称不能为空");

        string uid = _userManager.UserId;
        var entity = new BubbleChildProfileEntity
        {
            ParentId = uid,
            Name = input.name.Trim(),
            BirthYearMonth = string.IsNullOrWhiteSpace(input.birthYearMonth) ? null : input.birthYearMonth.Trim(),
            Avatar = input.avatar ?? string.Empty,
            Age = 0,
            CreateTime = DateTime.Now
        };

        entity.Age = CalcAgeYears(entity.BirthYearMonth) ?? entity.Age;

        var id = await _db.Insertable(entity).ExecuteReturnIdentityAsync();
        entity.Id = id;
        return ToOutput(entity);
    }

    /// <summary>
    /// 设置当前默认孩子（占位：仅返回 ok；如需全局默认请写入 bubble_user.current_child_id 或 profile 表）
    /// </summary>
    [HttpPost("current-child")]
    [Consumes("application/json")]
    public async Task<dynamic> SetCurrentChild([FromBody] dynamic input)
    {
        await Task.CompletedTask;
        return new { ok = true };
    }

    private static ChildOutput ToOutput(BubbleChildProfileEntity e)
    {
        int? ageYears = CalcAgeYears(e.BirthYearMonth);
        return new ChildOutput
        {
            id = e.Id,
            name = e.Name,
            birthYearMonth = e.BirthYearMonth,
            ageYears = ageYears,
            ageBand = ToAgeBand(ageYears),
            avatar = e.Avatar
        };
    }

    private static int? CalcAgeYears(string? birthYearMonth)
    {
        if (string.IsNullOrWhiteSpace(birthYearMonth)) return null;
        if (birthYearMonth.Length < 7) return null;

        // YYYY-MM
        if (!int.TryParse(birthYearMonth.Substring(0, 4), out int y)) return null;
        if (!int.TryParse(birthYearMonth.Substring(5, 2), out int m)) return null;
        if (m < 1 || m > 12) return null;

        var birth = new DateTime(y, m, 1);
        var now = DateTime.Now;
        int age = now.Year - birth.Year;
        if (now.Month < birth.Month) age--;
        if (age < 0) age = 0;
        return age;
    }

    private static string? ToAgeBand(int? ageYears)
    {
        if (!ageYears.HasValue) return null;
        int a = ageYears.Value;
        if (a <= 2) return "2-";
        if (a <= 4) return "3-4";
        if (a <= 6) return "5-6";
        if (a <= 8) return "7-8";
        return "9+";
    }
}
