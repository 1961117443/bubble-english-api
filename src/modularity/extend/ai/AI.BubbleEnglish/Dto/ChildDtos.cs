namespace AI.BubbleEnglish.Dto;

using QT.DependencyInjection;

[SuppressSniffer]
public class ChildCreateInput
{
    public string name { get; set; }
    /// <summary>出生年月（YYYY-MM）</summary>
    public string? birthYearMonth { get; set; }
    public string? avatar { get; set; }
}

[SuppressSniffer]
public class ChildOutput
{
    public long id { get; set; }
    public string name { get; set; }
    public string? birthYearMonth { get; set; }
    public int? ageYears { get; set; }
    public string? ageBand { get; set; }
    public string? avatar { get; set; }
}
