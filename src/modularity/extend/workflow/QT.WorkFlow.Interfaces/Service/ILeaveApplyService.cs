using System.Threading.Tasks;

namespace QT.WorkFlow.Interfaces.Service;

/// <summary>
/// 请假申请




/// </summary>
public interface ILeaveApplyService
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
