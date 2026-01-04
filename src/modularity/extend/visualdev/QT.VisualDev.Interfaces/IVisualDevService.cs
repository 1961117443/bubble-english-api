using QT.VisualDev.Entitys;
namespace QT.VisualDev.Interfaces;

/// <summary>
/// 可视化开发基础抽象类.
/// </summary>
public interface IVisualDevService
{
    /// <summary>
    /// 获取功能信息.
    /// </summary>
    /// <param name="id">主键ID.</param>
    /// <returns></returns>
    Task<VisualDevEntity> GetInfoById(string id);

    /// <summary>
    /// 判断功能ID是否存在.
    /// </summary>
    /// <param name="id">id.</param>
    /// <returns></returns>
    Task<bool> GetDataExists(string id);

    /// <summary>
    /// 判断是否存在编码、名称相同的数据.
    /// </summary>
    /// <param name="enCode">编码.</param>
    /// <param name="fullName">名称.</param>
    /// <returns></returns>
    Task<bool> GetDataExists(string enCode, string fullName);

    /// <summary>
    /// 新增导入数据.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task CreateImportData(VisualDevEntity input);

    /// <summary>
    /// 功能模板 无表 转 有表.
    /// </summary>
    /// <param name="vEntity">功能实体.</param>
    /// <param name="mainTableName">主表名称.</param>
    /// <returns></returns>
    Task<VisualDevEntity> NoTblToTable(VisualDevEntity vEntity, string mainTableName);

    /// <summary>
    /// 数据 转 插入Sql语句.
    /// </summary>
    /// <param name="tableName">表名.</param>
    /// <param name="dataStr">数据包字符串.</param>
    /// <returns></returns>
    List<string> DataToInsertSql(string tableName, string dataStr);
}
