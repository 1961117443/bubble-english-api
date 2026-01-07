namespace AI.BubbleEnglish.Infrastructure.Jobs;

using AI.BubbleEnglish.Entitys;
using AI.BubbleEnglish.Infrastructure.Ai;
using AI.BubbleEnglish.Infrastructure.Storage;
using global::Quartz;
using Quartz;
using SqlSugar;
using System.Text.Json;

/// <summary>
/// AI analysis job: take video.SourceText, ask LLM to generate words/sentences with minAge, then upsert Units and draft CourseJson(v2).
/// </summary>
public class AiAnalyzeJob : IJob
{
    public const string JobKeyName = "bubble.ai.analyze";

    private readonly ISqlSugarClient _db;
    private readonly IAiChatClientFactory _aiFactory;
    private readonly IBubbleStorageService _storage;

    public AiAnalyzeJob(ISqlSugarClient db, IAiChatClientFactory aiFactory, IBubbleStorageService storage)
    {
        _db = db;
        _aiFactory = aiFactory;
        _storage = storage;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var ct = context.CancellationToken;
        var jobId = context.MergedJobDataMap.GetLong("aiJobId");
        if (jobId <= 0) return;

        var job = await _db.Queryable<BubbleAiJobEntity>().SingleAsync(x => x.Id == jobId);
        if (job == null) return;

        job.Status = "processing";
        job.StartedAt = DateTime.Now;
        await _db.Updateable(job).UpdateColumns(x => new { x.Status, x.StartedAt }).ExecuteCommandAsync();

        try
        {
            var v = await _db.Queryable<BubbleVideoEntity>().SingleAsync(x => x.Id == job.VideoId);
            if (v == null) throw new InvalidOperationException("视频不存在");
            if (string.IsNullOrWhiteSpace(v.SourceText))
                throw new InvalidOperationException("SourceText 为空，请先跑 ASR/字幕提取");

            var provider = (job.Provider ?? string.Empty).Trim();
            var client = _aiFactory.Get(provider);

            var system = "You are an English-learning content designer for children aged 3-6. Output STRICT JSON.";
            var userPrompt = BuildPrompt(v.Title, v.SourceText, job.Prompt);

            var req = new AiChatRequest
            {
                Model = job.Model,
                SystemPrompt = system,
                UserPrompt = userPrompt,
                Temperature = 0.2,
                MaxTokens = 2000,
                JsonOutput = true
            };

            var res = await client.CompleteAsync(req, ct);
            job.PromptSnapshot = userPrompt;
            job.OutputJson = res.Content;

            var parsed = ParseAiOutput(res.Content);

            // Units: insert for this video
            long? vidLong = null;
            if (long.TryParse(v.Id, out var tmp)) vidLong = tmp;

            // naive insert (first version). If repeated, allow duplicates by design for audit.
            var now = DateTime.Now;
            var unitsToInsert = new List<BubbleUnitEntity>();
            foreach (var w in parsed.Words)
            {
                unitsToInsert.Add(new BubbleUnitEntity
                {
                    VideoId = vidLong,
                    UnitType = "word",
                    Text = w.Text,
                    Meaning = w.Meaning ?? string.Empty,
                    MinAge = w.MinAge <= 0 ? 3 : w.MinAge,
                    ImageUrl = string.Empty,
                    AudioUrl = string.Empty,
                    Status = "draft",
                    CreateTime = now,
                    UpdateTime = now
                });
            }
            foreach (var s in parsed.Sentences)
            {
                unitsToInsert.Add(new BubbleUnitEntity
                {
                    VideoId = vidLong,
                    UnitType = "sentence",
                    Text = s.Text,
                    Meaning = s.Meaning ?? string.Empty,
                    MinAge = s.MinAge <= 0 ? 5 : s.MinAge,
                    ImageUrl = string.Empty,
                    AudioUrl = string.Empty,
                    Status = "draft",
                    CreateTime = now,
                    UpdateTime = now
                });
            }
            if (unitsToInsert.Count > 0)
                await _db.Insertable(unitsToInsert).ExecuteCommandAsync();

            // Build CourseJson v2 (draft)
            var cover = string.IsNullOrWhiteSpace(v.CoverUrl) ? "" : v.CoverUrl;
            var courseJson = CourseJsonBuilder.BuildCourseJsonV2(
                courseId: $"video-{v.Id}",
                title: string.IsNullOrWhiteSpace(parsed.CourseTitle) ? v.Title : parsed.CourseTitle,
                cover: cover,
                words: parsed.Words,
                sentences: parsed.Sentences
            );

            var course = new BubbleCourseEntity
            {
                CourseKey = $"video-{v.Id}-job-{job.Id}",
                Title = string.IsNullOrWhiteSpace(parsed.CourseTitle) ? v.Title : parsed.CourseTitle,
                Cover = cover,
                Description = parsed.Description ?? string.Empty,
                CourseJson = courseJson,
                Sort = 0,
                IsPublish = 0,
                CreateTime = now
            };
            await _db.Insertable(course).ExecuteReturnIdentityAsync();

            job.Status = "success";
            job.FinishedAt = DateTime.Now;
            job.ErrorMessage = string.Empty;
            await _db.Updateable(job).UpdateColumns(x => new { x.Status, x.PromptSnapshot, x.OutputJson, x.ErrorMessage, x.FinishedAt }).ExecuteCommandAsync();

            // update video
            v.Status = "done";
            v.UpdateTime = DateTime.Now;
            await _db.Updateable(v).UpdateColumns(x => new { x.Status, x.UpdateTime }).ExecuteCommandAsync();
        }
        catch (Exception ex)
        {
            job.Status = "failed";
            job.ErrorMessage = ex.ToString();
            job.FinishedAt = DateTime.Now;
            await _db.Updateable(job).UpdateColumns(x => new { x.Status, x.ErrorMessage, x.FinishedAt }).ExecuteCommandAsync();
            throw;
        }
    }

    private static string BuildPrompt(string title, string sourceText, string? extra)
    {
        var basePrompt = $@"
Video title: {title}

SourceText (English transcript):
{sourceText}

Task:
1) Extract 6-10 WORD units for age 3-4 (minAge=3). Use simple nouns/adjectives.
2) Extract 2-4 SENTENCE units for age 5-6 (minAge=5). Short sentences suitable for kids.
3) For each unit provide meaning in Chinese.

Output STRICT JSON with this schema:
{{
  ""courseTitle"": """",
  ""description"": """",
  ""words"": [{{""text"":""panda"",""meaning"":""熊猫"",""minAge"":3}}],
  ""sentences"": [{{""text"":""Pandas are black and white."",""meaning"":""熊猫是黑白相间的。"",""minAge"":5}}]
}}
";
        if (!string.IsNullOrWhiteSpace(extra))
            basePrompt += "\n\nAdditional requirements:\n" + extra.Trim();
        return basePrompt;
    }

    private static AiParsedOutput ParseAiOutput(string json)
    {
        // tolerate models that wrap in ```json
        json = json.Trim();
        if (json.StartsWith("```"))
        {
            var idx = json.IndexOf('{');
            var last = json.LastIndexOf('}');
            if (idx >= 0 && last > idx) json = json.Substring(idx, last - idx + 1);
        }

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var outp = new AiParsedOutput
        {
            CourseTitle = root.TryGetProperty("courseTitle", out var ct) ? ct.GetString() : null,
            Description = root.TryGetProperty("description", out var d) ? d.GetString() : null
        };

        if (root.TryGetProperty("words", out var words) && words.ValueKind == JsonValueKind.Array)
        {
            foreach (var w in words.EnumerateArray())
            {
                outp.Words.Add(new AI.BubbleEnglish.Infrastructure.Ai.AiUnitItem(
                    w.TryGetProperty("text", out var t) ? (t.GetString() ?? "") : "",
                    w.TryGetProperty("meaning", out var m) ? m.GetString() : null,
                    w.TryGetProperty("minAge", out var a) ? a.GetInt32() : 3
                ));
            }
        }

        if (root.TryGetProperty("sentences", out var ss) && ss.ValueKind == JsonValueKind.Array)
        {
            foreach (var s in ss.EnumerateArray())
            {
                outp.Sentences.Add(new AI.BubbleEnglish.Infrastructure.Ai.AiUnitItem(
                    s.TryGetProperty("text", out var t) ? (t.GetString() ?? "") : "",
                    s.TryGetProperty("meaning", out var m) ? m.GetString() : null,
                    s.TryGetProperty("minAge", out var a) ? a.GetInt32() : 5
                ));
            }
        }

        // basic cleanup
        outp.Words = outp.Words.Where(x => !string.IsNullOrWhiteSpace(x.Text)).DistinctBy(x => x.Text.ToLowerInvariant()).ToList();
        outp.Sentences = outp.Sentences.Where(x => !string.IsNullOrWhiteSpace(x.Text)).DistinctBy(x => x.Text.ToLowerInvariant()).ToList();

        return outp;
    }

    private class AiParsedOutput
    {
        public string? CourseTitle { get; set; }
        public string? Description { get; set; }
        public List<AI.BubbleEnglish.Infrastructure.Ai.AiUnitItem> Words { get; set; } = new();
        public List<AI.BubbleEnglish.Infrastructure.Ai.AiUnitItem> Sentences { get; set; } = new();
    }
}
