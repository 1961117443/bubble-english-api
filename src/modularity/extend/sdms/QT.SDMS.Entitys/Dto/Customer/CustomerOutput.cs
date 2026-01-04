
namespace QT.SDMS.Entitys.Dto.Customer;

public class CustomerOutput: CustomerUpInput
{
    public int phoneVerifyStatus { get; set; }

    public int emailVerifyStatus { get; set; }
}