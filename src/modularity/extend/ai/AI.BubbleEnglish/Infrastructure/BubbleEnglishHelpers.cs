using System.Globalization;

namespace AI.BubbleEnglish.Infrastructure;

internal static class BubbleEnglishHelpers
{
    public static int CalcAgeFromBirthYearMonth(string birthYearMonth, DateTime? now = null)
    {
        now ??= DateTime.Now;
        if (!DateTime.TryParseExact(birthYearMonth + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var birth))
            return 0;

        var n = now.Value;
        var age = n.Year - birth.Year;
        if (n.Month < birth.Month) age--;
        return Math.Max(0, age);
    }

    public static string? AgeBandFromAge(int age)
    {
        if (age <= 0) return null;
        if (age <= 4) return "3-4";
        if (age <= 6) return "5-6";
        if (age <= 8) return "7-8";
        return "8+";
    }
}
