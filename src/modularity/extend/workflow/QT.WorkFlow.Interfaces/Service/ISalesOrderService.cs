namespace QT.WorkFlow.Interfaces.Service;

/// <summary>
/// 销售订单




/// </summary>
public interface ISalesOrderService
{
    /// <summary>
    /// 工作流表单操作.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="obj"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    Task Save(string id, object obj, int type);
}
