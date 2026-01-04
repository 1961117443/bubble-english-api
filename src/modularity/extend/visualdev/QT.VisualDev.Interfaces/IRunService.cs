using QT.Common.Dtos.VisualDev;
using QT.Common.Filter;
using QT.VisualDev.Entitys;
using QT.VisualDev.Entitys.Dto.VisualDevModelData;

namespace QT.VisualDev.Interfaces;

/// <summary>
/// 在线开发运行服务接口.
/// </summary>
public interface IRunService
{
    /// <summary>
    /// 创建在线开发功能.
    /// </summary>
    /// <param name="templateEntity">功能模板实体.</param>
    /// <param name="dataInput">数据输入.</param>
    /// <returns></returns>
    Task Create(VisualDevEntity templateEntity, VisualDevModelDataCrInput dataInput);

    /// <summary>
    /// 创建在线开发有表SQL.
    /// </summary>
    /// <param name="templateEntity"></param>
    /// <param name="dataInput"></param>
    /// <param name="mainId"></param>
    /// <returns></returns>
    Task<List<string>> CreateHaveTableSql(VisualDevEntity templateEntity, VisualDevModelDataCrInput dataInput, string mainId);

    /// <summary>
    /// 修改在线开发功能.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="templateEntity"></param>
    /// <param name="visualdevModelDataUpForm"></param>
    /// <returns></returns>
    Task Update(string id, VisualDevEntity templateEntity, VisualDevModelDataUpInput visualdevModelDataUpForm);

    /// <summary>
    /// 修改在线开发有表sql.
    /// </summary>
    /// <param name="templateEntity"></param>
    /// <param name="dataInput"></param>
    /// <param name="mainId"></param>
    /// <returns></returns>
    Task<List<string>> UpdateHaveTableSql(VisualDevEntity templateEntity, VisualDevModelDataUpInput dataInput, string mainId);

    /// <summary>
    /// 删除无表信息.
    /// </summary>
    /// <returns></returns>
    Task DelIsNoTableInfo(string id, VisualDevEntity templateEntity);

    /// <summary>
    /// 批量删除无表数据.
    /// </summary>
    /// <returns></returns>
    Task BatchDelIsNoTableData(List<string> ids, VisualDevEntity templateEntity);

    /// <summary>
    /// 删除有表信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <param name="templateEntity">模板实体.</param>
    /// <returns></returns>
    Task DelHaveTableInfo(string id, VisualDevEntity templateEntity);

    /// <summary>
    /// 批量删除有表数据.
    /// </summary>
    /// <param name="ids">id数组.</param>
    /// <param name="templateEntity">模板实体.</param>
    /// <returns></returns>
    Task BatchDelHaveTableData(List<string> ids, VisualDevEntity templateEntity);

    /// <summary>
    /// 列表数据处理.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="input"></param>
    /// <param name="actionType"></param>
    /// <returns></returns>
    Task<PageResult<Dictionary<string, object>>> GetListResult(VisualDevEntity entity, VisualDevModelListQueryInput input, string actionType = "List");

    /// <summary>
    /// 关联表单列表数据处理.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="input"></param>
    /// <param name="actionType"></param>
    /// <returns></returns>
    Task<PageResult<Dictionary<string, object>>> GetRelationFormList(VisualDevEntity entity, VisualDevModelListQueryInput input, string actionType = "List");

    /// <summary>
    /// 获取模型数据信息.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<VisualDevModelDataEntity> GetInfo(string id);

    /// <summary>
    /// 获取无表详情转换.
    /// </summary>
    /// <param name="entity">模板实体.</param>
    /// <param name="data">真实数据.</param>
    /// <returns></returns>
    Task<Dictionary<string, object>> GetIsNoTableInfo(VisualDevEntity entity, string data);

    /// <summary>
    /// 获取无表信息详情.
    /// </summary>
    /// <param name="entity">模板实体.</param>
    /// <param name="data">真实数据.</param>
    /// <returns></returns>
    Task<string> GetIsNoTableInfoDetails(VisualDevEntity entity, VisualDevModelDataEntity data);

    /// <summary>
    /// 获取有表详情转换.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <param name="templateEntity">模板实体.</param>
    /// <returns></returns>
    Task<Dictionary<string, object>> GetHaveTableInfo(string id, VisualDevEntity templateEntity);

    /// <summary>
    /// 获取有表详情转换.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="templateEntity"></param>
    /// <returns></returns>
    Task<string> GetHaveTableInfoDetails(string id, VisualDevEntity templateEntity, bool isFlowTask = false);

    /// <summary>
    /// 生成系统自动生成字段.
    /// </summary>
    /// <param name="fieldsModelListJson">模板数据.</param>
    /// <param name="allDataMap">真实数据.</param>
    /// <param name="IsCreate">创建与修改标识 true创建 false 修改.</param>
    /// <returns></returns>
    Task<Dictionary<string, object>> GenerateFeilds(string fieldsModelListJson, Dictionary<string, object> allDataMap, bool IsCreate);

}
