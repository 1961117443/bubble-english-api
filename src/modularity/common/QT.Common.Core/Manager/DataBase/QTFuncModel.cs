using QT.VisualDev.Entitys.Dto.VisualDevModelData;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Common.Core.Manager;
public class QTFuncModel
{
    /// <summary>
    /// 操作类型
    /// 1：执行存储过程
    /// 2：调用内部接口
    /// </summary>
    public int? __type { get; set; }

    /// <summary>
    /// 执行的存储过程（存储过程名称|参数(多个逗号相连)）或者内部接口（类名:方法）
    /// </summary>
    public string __func { get; set; }
}

/// <summary>
/// 返回结果
/// </summary>
public class ExecuteQtFuncResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool success { get; set; }

    /// <summary>
    /// 结果集
    /// </summary>
    public DataTable dataTable { get; set; }

    /// <summary>
    /// 记录总数
    /// </summary>
    public int total { get; set; }
}

public interface IExecuteQtFunc
{
    /// <summary>
    /// 执行内部方法的接口
    /// </summary>
    /// <param name="db">当前数据库连接</param>
    /// <param name="pageInput"></param>
    /// <param name="conModels"></param>
    /// <param name="dataPermissions"></param>
    /// <returns></returns>
    ExecuteQtFuncResult Execute(SqlSugarClient db, VisualDevModelListQueryInput pageInput, List<IConditionalModel> conModels, List<IConditionalModel> dataPermissions);
}
