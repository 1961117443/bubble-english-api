namespace AI.BubbleEnglish.Infrastructure.Jobs;

using System.Globalization;
using System.Text.RegularExpressions;

public record SrtItem(int Index, double StartSeconds, double EndSeconds, string Text);

public static class SrtParser
{
    private static readonly Regex TimeRegex = new(@"(?<start>\d\d:\d\d:\d\d,\d\d\d)\s-->\s(?<end>\d\d:\d\d:\d\d,\d\d\d)", RegexOptions.Compiled);

    public static List<SrtItem> Parse(string srtContent)
    {
        var lines = srtContent.Replace("\r", "").Split('\n');
        var items = new List<SrtItem>();
        int i = 0;
        while (i < lines.Length)
        {
            // skip empties
            while (i < lines.Length && string.IsNullOrWhiteSpace(lines[i])) i++;
            if (i >= lines.Length) break;

            if (!int.TryParse(lines[i].Trim(), out var idx)) { i++; continue; }
            i++;
            if (i >= lines.Length) break;

            var m = TimeRegex.Match(lines[i]);
            if (!m.Success) { i++; continue; }
            var start = ParseTime(m.Groups["start"].Value);
            var end = ParseTime(m.Groups["end"].Value);
            i++;

            var textLines = new List<string>();
            while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
            {
                textLines.Add(lines[i].Trim());
                i++;
            }

            var text = string.Join(" ", textLines);
            items.Add(new SrtItem(idx, start, end, text));
        }

        return items;
    }

    private static double ParseTime(string t)
    {
        // 00:00:01,234
        var parts = t.Split(new[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4) return 0;
        var h = int.Parse(parts[0], CultureInfo.InvariantCulture);
        var m = int.Parse(parts[1], CultureInfo.InvariantCulture);
        var s = int.Parse(parts[2], CultureInfo.InvariantCulture);
        var ms = int.Parse(parts[3], CultureInfo.InvariantCulture);
        return h * 3600 + m * 60 + s + ms / 1000.0;
    }
}
