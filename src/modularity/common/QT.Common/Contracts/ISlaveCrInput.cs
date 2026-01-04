namespace QT.Common.Contracts;

public interface ISlaveCrInput<TDV>
{
    public List<TDV> items { get; set; }
}