namespace AI.BubbleEnglish.Infrastructure.Jobs;

using System.Text.RegularExpressions;

public static class SrtAligner
{
    public static (double start, double end)? FindTimeRangeForSentence(string sentence, IReadOnlyList<SrtItem> items)
    {
        if (string.IsNullOrWhiteSpace(sentence) || items.Count == 0) return null;
        var target = Normalize(sentence);
        if (target.Length == 0) return null;

        // pass1: direct contains over concatenated windows
        for (int i = 0; i < items.Count; i++)
        {
            string acc = "";
            double start = items[i].StartSeconds;
            for (int j = i; j < Math.Min(items.Count, i + 6); j++)
            {
                acc = (acc + " " + items[j].Text).Trim();
                var norm = Normalize(acc);
                if (norm.Contains(target) || target.Contains(norm))
                {
                    var end = items[j].EndSeconds;
                    return (start, end);
                }
            }
        }

        // pass2: best overlap score
        double best = 0;
        (double start, double end)? bestRange = null;
        for (int i = 0; i < items.Count; i++)
        {
            var norm = Normalize(items[i].Text);
            if (norm.Length == 0) continue;
            var score = OverlapScore(target, norm);
            if (score > best)
            {
                best = score;
                bestRange = (items[i].StartSeconds, items[i].EndSeconds);
            }
        }
        return best >= 0.35 ? bestRange : null;
    }

    private static string Normalize(string s)
    {
        s = s.ToLowerInvariant();
        s = Regex.Replace(s, "[^a-z0-9 ]", " ");
        s = Regex.Replace(s, "\\s+", " ").Trim();
        return s;
    }

    private static double OverlapScore(string a, string b)
    {
        var aw = a.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var bw = b.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (aw.Length == 0 || bw.Length == 0) return 0;
        var set = new HashSet<string>(bw);
        int hit = aw.Count(set.Contains);
        return (double)hit / Math.Max(aw.Length, bw.Length);
    }
}
