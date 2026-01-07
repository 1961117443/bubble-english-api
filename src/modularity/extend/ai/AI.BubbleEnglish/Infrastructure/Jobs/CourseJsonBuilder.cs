namespace AI.BubbleEnglish.Infrastructure.Jobs;

using AI.BubbleEnglish.Infrastructure.Ai;
using System.Text.Json;

public static class CourseJsonBuilder
{
    public static string BuildCourseJsonV2(
        string courseId,
        string title,
        string cover,
        IEnumerable<AiUnitItem> words,
        IEnumerable<AiUnitItem> sentences)
    {
        var units = new List<object>();
        int wIndex = 0;
        foreach (var w in words)
        {
            units.Add(new
            {
                id = $"w-{wIndex++}",
                type = "word",
                text = w.Text,
                meaning = w.Meaning,
                assets = new { image = "", audio = new { url = "", source = "tts" } },
                minAge = w.MinAge
            });
        }

        int sIndex = 0;
        foreach (var s in sentences)
        {
            units.Add(new
            {
                id = $"s-{sIndex++}",
                type = "sentence",
                text = s.Text,
                meaning = s.Meaning,
                assets = new { image = "", audio = new { url = "", source = "video" } },
                minAge = s.MinAge
            });
        }

        var course = new
        {
            version = 2,
            id = courseId,
            title,
            cover,
            units,
            flow = new
            {
                byUnitType = new
                {
                    word = new[] { "listen", "play" },
                    sentence = new[] { "listen", "speak" }
                },
                reward = new { stars = 2, coin = 8 }
            }
        };

        return JsonSerializer.Serialize(course, new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });
    }
}
