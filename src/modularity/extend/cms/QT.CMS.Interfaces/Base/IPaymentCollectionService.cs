
namespace QT.CMS.Interfaces;

public interface IPaymentCollectionService
{
    Task<bool> ConfirmAsync(string tradeNo);
}