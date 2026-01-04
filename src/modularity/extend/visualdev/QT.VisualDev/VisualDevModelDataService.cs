using QT.Common.Configuration;
using QT.Common.Core.Manager;
using QT.Common.Core.Manager.Files;
using QT.Common.Dtos.VisualDev;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Models.NPOI;
using QT.Common.Security;
using QT.DataEncryption;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.VisualDev.Engine;
using QT.VisualDev.Engine.Security;
using QT.VisualDev.Entitys;
using QT.VisualDev.Entitys.Dto.VisualDevModelData;
using QT.VisualDev.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Yitter.IdGenerator;
using QT.Systems.Interfaces.System;
using QT.Systems.Entitys.Dto.DictionaryData;
using QT.VisualDev.Entitys.Dto.VisualDev;

namespace QT.VisualDev
{
    /// <summary>
    /// 可视化开发基础.
    /// </summary>
    [ApiDescriptionSettings(Tag = "VisualDev", Name = "OnlineDev", Order = 172)]
    [Route("api/visualdev/[controller]")]
    public class VisualDevModelDataService : IDynamicApiController, ITransient
    {
        /// <summary>
        /// 可视化开发基础.
        /// </summary>
        private readonly IVisualDevService _visualDevService;

        /// <summary>
        /// 在线开发运行服务.
        /// </summary>
        private readonly IRunService _runService;

        /// <summary>
        /// 用户管理.
        /// </summary>
        private readonly IUserManager _userManager;

        /// <summary>
        /// 文件服务.
        /// </summary>
        private readonly IFileManager _fileManager;
        private readonly IDictionaryDataService _dictionaryDataService;

        /// <summary>
        /// 初始化一个<see cref="VisualDevModelDataService"/>类型的新实例.
        /// </summary>
        public VisualDevModelDataService(IVisualDevService visualDevService, IRunService runService, IUserManager userManager, IFileManager fileManager, IDictionaryDataService dictionaryDataService)
        {
            _visualDevService = visualDevService;
            _runService = runService;
            _userManager = userManager;
            _fileManager = fileManager;
            _dictionaryDataService = dictionaryDataService;
        }

        #region Get

        /// <summary>
        /// 获取列表表单配置JSON.
        /// </summary>
        /// <param name="modelId">主键id.</param>
        /// <returns></returns>
        [HttpGet("{modelId}/Config")]
        public async Task<dynamic> GetData(string modelId)
        {
            VisualDevEntity? data = await _visualDevService.GetInfoById(modelId);
            return data.Adapt<VisualDevModelDataConfigOutput>();
        }

        /// <summary>
        /// 获取列表配置JSON.
        /// </summary>
        /// <param name="modelId">主键id.</param>
        /// <returns></returns>
        [HttpGet("{modelId}/ColumnData")]
        public async Task<dynamic> GetColumnData(string modelId)
        {
            VisualDevEntity? data = await _visualDevService.GetInfoById(modelId);
            return new { columnData = data.ColumnData };
        }

        /// <summary>
        /// 获取列表配置JSON.
        /// </summary>
        /// <param name="modelId">主键id.</param>
        /// <returns></returns>
        [HttpGet("{modelId}/FormData")]
        public async Task<dynamic> GetFormData(string modelId)
        {
            VisualDevEntity? data = await _visualDevService.GetInfoById(modelId);
            return new { formData = data.FormData };
        }

        /// <summary>
        /// 获取列表配置JSON.
        /// </summary>
        /// <param name="modelId">主键id.</param>
        /// <returns></returns>
        [HttpGet("{modelId}/FlowTemplate")]
        public async Task<dynamic> GetFlowTemplate(string modelId)
        {
            VisualDevEntity? data = await _visualDevService.GetInfoById(modelId);
            return new { flowTemplateJson = data.FlowTemplateJson };
        }

        /// <summary>
        /// 获取数据信息.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="modelId"></param>
        /// <returns></returns>
        [HttpGet("{modelId}/{id}")]
        public async Task<dynamic> GetInfo(string id, string modelId)
        {
            VisualDevEntity? templateEntity = await _visualDevService.GetInfoById(modelId); // 模板实体

            // 有表
            if (!string.IsNullOrEmpty(templateEntity.Tables) && !"[]".Equals(templateEntity.Tables))
                return new { id = id, data = (await _runService.GetHaveTableInfo(id, templateEntity)).ToJsonString() };

            VisualDevModelDataEntity? entity = await _runService.GetInfo(id); // 无表
            Dictionary<string, object>? data = await _runService.GetIsNoTableInfo(templateEntity, entity.Data);
            return new { id = entity.Id, data = data.ToJsonString() };
        }

        /// <summary>
        /// 获取详情.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="modelId"></param>
        /// <returns></returns>
        [HttpGet("{modelId}/{id}/DataChange")]
        public async Task<dynamic> GetDetails(string id, string modelId)
        {
            VisualDevEntity? templateEntity = await _visualDevService.GetInfoById(modelId); // 模板实体

            // 有表
            if (!string.IsNullOrEmpty(templateEntity.Tables) && !"[]".Equals(templateEntity.Tables))
                return new { id = id, data = await _runService.GetHaveTableInfoDetails(id, templateEntity) };

            VisualDevModelDataEntity? entity = await _runService.GetInfo(id); // 无表
            string data = await _runService.GetIsNoTableInfoDetails(templateEntity, entity);
            return new { id = entity.Id, data = data };
        }

        #endregion

        #region Post

        /// <summary>
        /// 功能导出.
        /// </summary>
        /// <param name="modelId"></param>
        /// <returns></returns>
        [HttpPost("{modelId}/Actions/ExportData")]
        public async Task<dynamic> ActionsExportData(string modelId)
        {
            VisualDevEntity? templateEntity = await _visualDevService.GetInfoById(modelId); // 模板实体
            string? jsonStr = templateEntity.ToJsonString();
            return await _fileManager.Export(jsonStr, templateEntity.FullName, ExportFileType.vdd);
        }

        /// <summary>
        /// 导入.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("Model/Actions/ImportData")]
        public async Task ActionsActionsImport(IFormFile file)
        {
            string? fileType = Path.GetExtension(file.FileName).Replace(".", string.Empty);
            if (!fileType.ToLower().Equals(ExportFileType.vdd.ToString())) throw Oops.Oh(ErrorCode.D3006);
            string? josn = _fileManager.Import(file);
            VisualDevEntity? templateEntity;
            try
            {
                templateEntity = josn.ToObject<VisualDevEntity>();
            }
            catch
            {
                throw Oops.Oh(ErrorCode.D3006);
            }

            if (templateEntity == null || templateEntity.Type.IsNullOrEmpty())throw Oops.Oh(ErrorCode.D3006);
            else if (templateEntity.Type != 1) throw Oops.Oh(ErrorCode.D3009);
            if (!string.IsNullOrEmpty(templateEntity.Id) && await _visualDevService.GetDataExists(templateEntity.Id))
                throw Oops.Oh(ErrorCode.D1400);
            if(await _visualDevService.GetDataExists(templateEntity.EnCode, templateEntity.FullName))
                throw Oops.Oh(ErrorCode.D1400);
            await _visualDevService.CreateImportData(templateEntity);
        }

        /// <summary>
        /// 应用中心导入.
        /// </summary>
        /// <returns></returns>
        [HttpPost("Model/Actions/CloudImportData")]
        public async Task ActionsCloudImport([FromBody] VisualDevInfoOutput input)
        {
            VisualDevEntity? templateEntity = input.Adapt<VisualDevEntity>();
            //try
            //{
            //    templateEntity = josn.ToObject<VisualDevEntity>();
            //}
            //catch
            //{
            //    throw Oops.Oh(ErrorCode.D3006);
            //}

            if (templateEntity == null || templateEntity.Type.IsNullOrEmpty()) throw Oops.Oh(ErrorCode.D3006);
            else if (templateEntity.Type != 1) throw Oops.Oh(ErrorCode.D3009);
            if (!string.IsNullOrEmpty(templateEntity.Id) && await _visualDevService.GetDataExists(templateEntity.Id))
                throw Oops.Oh(ErrorCode.D1400);
            if (await _visualDevService.GetDataExists(templateEntity.EnCode, templateEntity.FullName))
                throw Oops.Oh(ErrorCode.D1400);
            await _visualDevService.CreateImportData(templateEntity);
        }

        /// <summary>
        /// 获取数据列表.
        /// </summary>
        /// <param name="modelId">主键id.</param>
        /// <param name="input">分页查询条件.</param>
        /// <returns></returns>
        [HttpPost("{modelId}/List")]
        public async Task<dynamic> List(string modelId, [FromBody] VisualDevModelListQueryInput input)
        {
            VisualDevEntity? templateEntity = await _visualDevService.GetInfoById(modelId);
            return await _runService.GetListResult(templateEntity, input);
        }

        /// <summary>
        /// 创建数据.
        /// </summary>
        /// <param name="modelId"></param>
        /// <param name="visualdevModelDataCrForm"></param>
        /// <returns></returns>
        [HttpPost("{modelId}")]
        public async Task Create(string modelId, [FromBody] VisualDevModelDataCrInput visualdevModelDataCrForm)
        {
            VisualDevEntity? templateEntity = await _visualDevService.GetInfoById(modelId);
            await _runService.Create(templateEntity, visualdevModelDataCrForm);
        }

        /// <summary>
        /// 修改数据.
        /// </summary>
        /// <param name="modelId"></param>
        /// <param name="id"></param>
        /// <param name="visualdevModelDataUpForm"></param>
        /// <returns></returns>
        [HttpPut("{modelId}/{id}")]
        public async Task Update(string modelId, string id, [FromBody] VisualDevModelDataUpInput visualdevModelDataUpForm)
        {
            VisualDevEntity? templateEntity = await _visualDevService.GetInfoById(modelId);
            await _runService.Update(id, templateEntity, visualdevModelDataUpForm);
         }

        /// <summary>
        /// 删除数据.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="modelId"></param>
        /// <returns></returns>
        [HttpDelete("{modelId}/{id}")]
        public async Task Delete(string id, string modelId)
        {
            VisualDevEntity? templateEntity = await _visualDevService.GetInfoById(modelId);
            if (!string.IsNullOrEmpty(templateEntity.Tables) && !"[]".Equals(templateEntity.Tables)) await _runService.DelHaveTableInfo(id, templateEntity);
            else await _runService.DelIsNoTableInfo(id, templateEntity);
        }

        /// <summary>
        /// 批量删除.
        /// </summary>
        /// <param name="modelId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("batchDelete/{modelId}")]
        public async Task BatchDelete(string modelId, [FromBody] VisualDevModelDataBatchDelInput input)
        {
            VisualDevEntity? templateEntity = await _visualDevService.GetInfoById(modelId);
            if (!string.IsNullOrEmpty(templateEntity.Tables) && !"[]".Equals(templateEntity.Tables)) await _runService.BatchDelHaveTableData(input.ids, templateEntity);
            else await _runService.BatchDelIsNoTableData(input.ids, templateEntity);
        }

        /// <summary>
        /// 导出.
        /// </summary>
        /// <param name="modelId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("{modelId}/Actions/Export")]
        public async Task<dynamic> Export(string modelId, [FromBody] VisualDevModelListQueryInput input)
        {
            VisualDevEntity? templateEntity = await _visualDevService.GetInfoById(modelId);
            List<VisualDevModelDataEntity> list = new List<VisualDevModelDataEntity>();
            if (input.dataType == "1") input.pageSize = 99999999;
            PageResult<Dictionary<string, object>>? pageList = await _runService.GetListResult(templateEntity, input);

            // 如果是 分组表格 模板
            ColumnDesignModel? columnData = TemplateKeywordsHelper.ReplaceKeywords(templateEntity.ColumnData).ToObject<ColumnDesignModel>(); // 列配置模型
            if (columnData.type == 3)
            {
                List<Dictionary<string, object>>? newValueList = new List<Dictionary<string, object>>();
                pageList.list.ForEach(item =>
                {
                    List<Dictionary<string, object>>? tt = item["children"].ToJsonString().ToObject<List<Dictionary<string, object>>>();
                    newValueList.AddRange(tt);
                });
                pageList.list = newValueList;
            }

            List<Dictionary<string, object>> realList = pageList.list.ToObject<List<Dictionary<string, object>>>();
            var columnDesignModel = templateEntity.ColumnData.ToObject<ColumnDesignModel>();

            var optionColumns = columnDesignModel.columnList.Where(it => it.__config__ != null && it.__config__.dataType == "dictionary" && it.__slot__ != null && it.__slot__.options.IsAny());
                //.Select(it=> new
                //{
                //    propValue = it.__config__.props.value ?? "",
                //    propLabel = it.__config__.props.label,
                //    options = it.__slot__.options,
                //    key = it.__vModel__
                //});
            if (optionColumns.IsAny())
            {
                foreach (var xitem in optionColumns)
                {
                    string key = xitem.__vModel__;
                    string propLabel = xitem.__config__.props.label!;
                    string propValue = xitem.__config__.props.value!;
                    List<Dictionary<string, object>> options = xitem.__slot__.options ?? new List<Dictionary<string, object>>();

                    // 重新获取字典类型的下拉参数
                    if (xitem.__config__ !=null && xitem.__config__.dataType == "dictionary" && xitem.__config__.dictionaryType.IsNotEmptyOrNull())
                    {
                        var dictionary = (await _dictionaryDataService.GetList(xitem.__config__.dictionaryType)).Adapt<List<DictionaryDataListOutput>>();
                        options = dictionary.ToObject<List<Dictionary<string, object>>>();
                    }
                    //if (!options.IsAny() && xitem.__config__.dictionaryType.IsNotEmptyOrNull())
                    //{
                    //    var dictionary = await _dictionaryDataService.GetList(xitem.__config__.dictionaryType);
                    //    options = dictionary.ToObject<List<Dictionary<string, object>>>();
                    //}

                    foreach (var item in realList)
                    {
                        if (item.TryGetValue(key, out object value) && value.IsNotEmptyOrNull())
                        {
                            var row = options.Find(x => x[propValue].ToString() == value.ToString());
                            if (row != null)
                            {
                                item[key] = row[propLabel];
                            }
                        }
                    }

                }
               
            }
            
            if (templateEntity.Type == 6)
            {
                
                var modelList= columnDesignModel.columnList.Where(it=>!it.hidden).Select(it =>
                {
                    var o = it as FieldsModel;
                    if (o.__config__ == null)
                    {
                        o.__config__ = new ConfigModel();
                    }
                    if (o.__config__.label.IsNullOrEmpty())
                    {
                        o.__config__.label = it.label;
                    }                    
                    return o;
                }).ToList();
                return await ExcelCreateModel(modelList, realList, input.selectKey);
            }
            return await ExcelCreateModel(templateEntity.FormData, realList, input.selectKey);
        }

        #endregion

        #region PublicMethod

        /// <summary>
        /// Excel 转输出 Model.
        /// </summary>
        /// <param name="formJson">表单Json.</param>
        /// <param name="realList">数据列表.</param>
        /// <param name="keys"></param>
        /// <returns>VisualDevModelDataExportOutput.</returns>
        public async Task<VisualDevModelDataExportOutput> ExcelCreateModel(string formJson, List<Dictionary<string, object>> realList, List<string> keys)
        {
            List<ExcelTemplateModel> templateList = new List<ExcelTemplateModel>();
            VisualDevModelDataExportOutput output = new VisualDevModelDataExportOutput();
            FormDataModel formDataModel = TemplateKeywordsHelper.ReplaceKeywords(formJson).ToObject<FormDataModel>();
            List<FieldsModel>? modelList = TemplateAnalysis.AnalysisTemplateData(formDataModel.fields);

            return await ExcelCreateModel(modelList, realList, keys);
        }


        /// <summary>
        /// Excel 转输出 Model.
        /// </summary>
        /// <param name="formJson">表单Json.</param>
        /// <param name="realList">数据列表.</param>
        /// <param name="keys"></param>
        /// <returns>VisualDevModelDataExportOutput.</returns>
        public async Task<VisualDevModelDataExportOutput> ExcelCreateModel(List<FieldsModel> modelList, List<Dictionary<string, object>> realList, List<string> keys)
        {
            VisualDevModelDataExportOutput output = new VisualDevModelDataExportOutput();
            List<string> columnList = new List<string>();
            if (!keys.IsAny())
            {
                keys = modelList.Where(it => it.__vModel__.IsNotEmptyOrNull()).Select(it => it.__vModel__).ToList();
            }
            try
            {
                ExcelConfig excelconfig = new ExcelConfig();
                excelconfig.FileName = YitIdHelper.NextId().ToString() + ".xls";
                excelconfig.HeadFont = "微软雅黑";
                excelconfig.HeadPoint = 10;
                excelconfig.IsAllSizeColumn = true;
                excelconfig.ColumnModel = new List<ExcelColumnModel>();
                foreach (string? item in keys)
                {
                    FieldsModel? excelColumn = modelList.Find(t => t.__vModel__ == item);
                    if (excelColumn != null)
                    {
                        excelconfig.ColumnModel.Add(new ExcelColumnModel() { Column = item, ExcelColumn = excelColumn.__config__.label });
                        columnList.Add(excelColumn.__config__.label);
                    }
                }

                string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
                var fs = ExcelExportHelper<Dictionary<string, object>>.ExportMemoryStream(realList, excelconfig, columnList);
                var flag = await _fileManager.UploadFileByType(fs, FileVariable.TemporaryFilePath, excelconfig.FileName);
                if (flag.Item1)
                {
                    fs.Flush();
                    fs.Close();
                }
                output.name = excelconfig.FileName;
                output.url = flag.Item2 ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "QT");
                return output;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}
