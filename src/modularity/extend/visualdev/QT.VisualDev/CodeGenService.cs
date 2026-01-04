using QT.Common.Configuration;
using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Security;
using QT.DataEncryption;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.Common;
using QT.Systems.Interfaces.System;
using QT.ViewEngine;
using QT.VisualDev.Engine;
using QT.VisualDev.Engine.CodeGen;
using QT.VisualDev.Engine.Model.CodeGen;
using QT.VisualDev.Engine.Security;
using QT.VisualDev.Entitys;
using QT.VisualDev.Entitys.Dto.CodeGen;
using QT.VisualDev.Entitys.Enum;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System.IO.Compression;
using System.Text;

namespace QT.VisualDev;

/// <summary>
/// 业务实现：代码生成.
/// </summary>
[ApiDescriptionSettings(Tag = "VisualDev", Name = "Generater", Order = 175)]
[Route("api/visualdev/[controller]")]
public class CodeGenService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<VisualDevEntity> _repository;

    /// <summary>
    /// 视图引擎.
    /// </summary>
    private readonly IViewEngine _viewEngine;

    /// <summary>
    /// 数据连接服务.
    /// </summary>
    private readonly IDbLinkService _dbLinkService;

    /// <summary>
    /// 字典数据服务.
    /// </summary>
    private readonly IDictionaryDataService _dictionaryDataService;

    /// <summary>
    /// 文件服务.
    /// </summary>
    private readonly IFileService _fileService;

    /// <summary>
    /// 用户管理器.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 数据库管理器.
    /// </summary>
    private readonly IDataBaseManager _databaseManager;

    /// <summary>
    /// 初始化一个<see cref="CodeGenService"/>类型的新实例.
    /// </summary>
    public CodeGenService(
        ISqlSugarRepository<VisualDevEntity> visualDevRepository,
        IViewEngine viewEngine,
        IDbLinkService dbLinkService,
        IDictionaryDataService dictionaryDataService,
        IFileService fileService,
        IUserManager userManager,
        IDataBaseManager databaseManager)
    {
        _repository = visualDevRepository;
        _viewEngine = viewEngine;
        _dbLinkService = dbLinkService;
        _dictionaryDataService = dictionaryDataService;
        _fileService = fileService;
        _userManager = userManager;
        _databaseManager = databaseManager;
    }

    #region Get

    /// <summary>
    /// 获取命名空间.
    /// </summary>
    [HttpGet("AreasName")]
    public dynamic GetAreasName()
    {
        List<string> areasName = new List<string>();
        if (KeyVariable.AreasName.Count > 0)
            areasName = KeyVariable.AreasName;
        return areasName;
    }

    #endregion

    #region Post

    /// <summary>
    /// 下载代码.
    /// </summary>
    [HttpPost("{id}/Actions/DownloadCode")]
    public async Task<dynamic> DownloadCode(string id, [FromBody] DownloadCodeFormInput downloadCodeForm)
    {
        var templateEntity = await _repository.FirstOrDefaultAsync(v => v.Id == id && v.DeleteMark == null);
        _ = templateEntity ?? throw Oops.Oh(ErrorCode.COM1005);
        _ = templateEntity.Tables ?? throw Oops.Oh(ErrorCode.D2100);
        var model = TemplateKeywordsHelper.ReplaceKeywords(templateEntity.FormData).ToObject<FormDataModel>();
        if (templateEntity.Type == 3)
            downloadCodeForm.module = "WorkFlowForm";
        model.className = downloadCodeForm.className.ParseToPascalCase();
        model.areasName = downloadCodeForm.module;
        string fileName = templateEntity.FullName;

        // 判断子表名称
        var childTb = new List<string>();
        if (!downloadCodeForm.subClassName.IsNullOrEmpty())
            childTb = new List<string>(downloadCodeForm.subClassName.Split(','));

        // 子表名称去重
        HashSet<string> set = new HashSet<string>(childTb);
        templateEntity.FormData = model.ToJsonString();
        bool result = childTb.Count == set.Count ? true : false;
        if (!result)
            throw Oops.Oh(ErrorCode.D2101);

        // 模板数据聚合
        await TemplatesDataAggregation(fileName, templateEntity);
        string randPath = Path.Combine(KeyVariable.SystemPath, "CodeGenerate", fileName);
        string downloadPath = randPath + ".zip";

        // 判断是否存在同名称文件
        if (File.Exists(downloadPath))
            File.Delete(downloadPath);

        ZipFile.CreateFromDirectory(randPath, downloadPath);
        if (!App.Configuration["OSS:Provider"].Equals("Invalid"))
            await _fileService.UploadFileByType(downloadPath, "CodeGenerate", string.Format("{0}.zip", fileName));
        var downloadFileName = _userManager.UserId + "|" + fileName + ".zip|codeGenerator";
        return new { name = fileName, url = "/api/File/Download?encryption=" + DESCEncryption.Encrypt(downloadFileName, "QT") };
    }

    /// <summary>
    /// 把代码下载到项目.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="downloadCodeForm"></param>
    /// <returns></returns>
    [HttpPost("{id}/Actions/DownloadCode/Local")]
    public async Task DownloadCodeToProject(string id, [FromBody] DownloadCodeFormInput downloadCodeForm)
    {
        var templateEntity = await _repository.FirstOrDefaultAsync(v => v.Id == id && v.DeleteMark == null);
        _ = templateEntity ?? throw Oops.Oh(ErrorCode.COM1005);
        _ = templateEntity.Tables ?? throw Oops.Oh(ErrorCode.D2100);
        var model = TemplateKeywordsHelper.ReplaceKeywords(templateEntity.FormData).ToObject<FormDataModel>();
        if (templateEntity.Type == 3)
            downloadCodeForm.module = "WorkFlowForm";
        model.className = downloadCodeForm.className.ParseToPascalCase();
        model.areasName = downloadCodeForm.module;
        string fileName = templateEntity.FullName;

        // 判断子表名称
        var childTb = new List<string>();
        if (!downloadCodeForm.subClassName.IsNullOrEmpty())
            childTb = new List<string>(downloadCodeForm.subClassName.Split(','));

        // 子表名称去重
        HashSet<string> set = new HashSet<string>(childTb);
        templateEntity.FormData = model.ToJsonString();
        bool result = childTb.Count == set.Count ? true : false;
        if (!result)
            throw Oops.Oh(ErrorCode.D2101);

        await this.TemplatesDataAggregation(fileName, templateEntity);
        string randPath = Path.Combine(KeyVariable.SystemPath, "CodeGenerate", fileName);

        //需要复制的内容
        //1、NetCode\Entitys\Dto
        //2、NetCode\Entitys\Entity
        //3、NetCode\Extend
        //4、NetCode\Interfaces


        var folderApi = downloadCodeForm.folderApi;
        var folderEntity = $"{folderApi}.Entitys";
        var folderInterface = $"{folderApi}.Interfaces";
        var folderWeb = downloadCodeForm.folderWeb;

        if (!FileHelper.IsExistDirectory(folderApi))
        {
            throw Oops.Oh($"【folderApi={folderApi}】不存在！");
        }

        // 复制api文件
        {
            var dataList = FileHelper.GetAllFiles(Path.Combine(randPath, "NetCode", "Extend"));
            if (dataList.IsAny())
            {
                dataList.ForEach(f =>
                {
                    var nf = Path.Combine(folderApi, f.Name);
                    FileHelper.Copy(f.FullName, nf);
                });
            }
        }

        // 复制实体类
        if (FileHelper.IsExistDirectory(folderEntity))
        {
            foreach (var item in new string[] { "Dto", "Entity" })
            {
                var newFolder = item == "Dto" ? Path.Combine(folderEntity, item, model.className) : Path.Combine(folderEntity, item);
                if (!FileHelper.IsExistDirectory(newFolder))
                {
                    FileHelper.CreateDir(newFolder);
                }
                FileHelper.CopyFolder(Path.Combine(randPath, "NetCode", "Entitys", item), newFolder);
            }
        }

        if (FileHelper.IsExistDirectory(folderInterface))
        {
            var dataList = FileHelper.GetAllFiles(Path.Combine(randPath, "NetCode", "Interfaces"));
            if (dataList.IsAny())
            {
                dataList.ForEach(f =>
                {
                    var nf = Path.Combine(folderInterface, f.Name);
                    FileHelper.Copy(f.FullName, nf);
                });
            }
        }

        // 复制PC页面
        if (!string.IsNullOrEmpty(folderWeb) && FileHelper.IsExistDirectory(folderWeb))
        {
            foreach (var item in FileHelper.GetDirectories(Path.Combine(randPath, "NetCode", "html", "PC")))
            {
                var dir = Path.Combine(folderWeb, Path.GetFileNameWithoutExtension(item));
                if (!FileHelper.IsExistDirectory(dir))
                {
                    FileHelper.CreateDir(dir);
                }
                FileHelper.CopyFolder(item, dir);
            }
        }
    }

    /// <summary>
    /// 预览代码.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="downloadCodeForm"></param>
    /// <returns></returns>
    [HttpPost("{id}/Actions/CodePreview")]
    public async Task<dynamic> CodePreview(string id, [FromBody] DownloadCodeFormInput downloadCodeForm)
    {
        var templateEntity = await _repository.FirstOrDefaultAsync(v => v.Id == id && v.DeleteMark == null);
        _ = templateEntity ?? throw Oops.Oh(ErrorCode.COM1005);
        _ = templateEntity.Tables ?? throw Oops.Oh(ErrorCode.D2100);
        var model = TemplateKeywordsHelper.ReplaceKeywords(templateEntity.FormData).ToObject<FormDataModel>();
        model.className = downloadCodeForm.className.ParseToPascalCase();
        model.areasName = downloadCodeForm.module;
        string fileName = SnowflakeIdHelper.NextId();

        // 判断子表名称
        var childTb = new List<string>();

        // 子表名称去重
        HashSet<string> set = new HashSet<string>(childTb);
        templateEntity.FormData = model.ToJsonString();
        bool result = childTb.Count == set.Count ? true : false;
        if (!result)
            throw Oops.Oh(ErrorCode.D2101);
        await this.TemplatesDataAggregation(fileName, templateEntity);
        string randPath = Path.Combine(FileVariable.GenerateCodePath, fileName);
        var dataList = this.PriviewCode(randPath);
        if (dataList == null && dataList.Count == 0)
            throw Oops.Oh(ErrorCode.D2102);
        return new { list = dataList };
    }
    #endregion

    #region PrivateMethod

    /// <summary>
    /// 模板数据聚合.
    /// </summary>
    /// <param name="fileName">生成ZIP文件名.</param>
    /// <param name="templateEntity">模板实体.</param>
    /// <returns></returns>
    private async Task TemplatesDataAggregation(string fileName, VisualDevEntity templateEntity)
    {
        // 类型名称
        var categoryName = (await _dictionaryDataService.GetInfo(templateEntity.Category)).EnCode;

        // 表关系
        List<DbTableRelationModel>? tableRelation = templateEntity.Tables.ToObject<List<DbTableRelationModel>>();

        // 表单数据
        var formDataModel = templateEntity.FormData.ToObject<FormDataModel>();

        // 列表属性
        ColumnDesignModel? pcColumnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
        ColumnDesignModel? appColumnDesignModel = templateEntity.AppColumnData?.ToObject<ColumnDesignModel>();
        pcColumnDesignModel ??= new ColumnDesignModel();
        appColumnDesignModel ??= new ColumnDesignModel();

        bool useDataPermission = false;

        if (pcColumnDesignModel.useDataPermission && appColumnDesignModel.useDataPermission)
        {
            useDataPermission = true;
        }
        else if (!pcColumnDesignModel.useDataPermission && appColumnDesignModel.useDataPermission)
        {
            useDataPermission = true;
        }
        else if (pcColumnDesignModel.useDataPermission && !appColumnDesignModel.useDataPermission)
        {
            useDataPermission = true;
        }
        else
        {
            useDataPermission = false;
        }

        // 控件组
        var controls = TemplateAnalysis.AnalysisTemplateData(formDataModel.fields);

        List<string> targetPathList = new List<string>();
        List<string> templatePathList = new List<string>();

        string tableName = string.Empty;
        CodeGenConfigModel codeGenConfigModel = new CodeGenConfigModel();

        /*区分是纯主表、主带副、主带子、主带副与子
         * 1-纯主表、2-主带子、3-主带副、4-主带副与子
         * 生成模式*/
        switch (JudgmentGenerationModel(tableRelation, controls))
        {
            case GeneratePatterns.MainBelt:
                {
                    var link = await _repository.Context.Queryable<DbLinkEntity>().FirstAsync(m => m.Id == templateEntity.DbLinkId && m.DeleteMark == null);
                    var targetLink = link ?? _databaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
                    foreach (var item in tableRelation)
                    {
                        if (!item.relationTable.Equals(string.Empty))
                        {
                            var children = controls.Find(f => f.__vModel__.Contains("Field") && f.__config__.tableName.Equals(item.table));
                            if (children != null) controls = children.__config__.children;
                        }

                        tableName = item.table;
                        var fieldList = _databaseManager.GetFieldList(targetLink, tableName);

                        // 后端生成
                        codeGenConfigModel = CodeGenWay.MainBeltBackEnd(tableName, fieldList, controls, templateEntity, item.relationTable.Equals(string.Empty));

                        switch (item.relationTable.Equals(string.Empty))
                        {
                            // 主表
                            case true:
                                codeGenConfigModel.BusName = item.tableName;
                                var tableRelations = tableRelation.FindAll(it => !it.relationTable.Equals(string.Empty)) ?? new List<DbTableRelationModel>();
                                var c = controls.FindAll(it => it.__vModel__.Contains("tableField") || tableRelations.Any(x => x.table == it.__vModel__));
                                codeGenConfigModel.TableRelations = GetCodeGenTableRelationList(tableRelations, targetLink, c);
                                targetPathList = CodeGenTargetPathHelper.BackendTargetPathList(tableName.ParseToPascalCase(), fileName, templateEntity.WebType);
                                templatePathList = CodeGenTargetPathHelper.BackendTemplatePathList("MainBelt", templateEntity.WebType);
                                break;

                            // 子表
                            default:
                                codeGenConfigModel.BusName = item.tableName;
                                codeGenConfigModel.ClassName = tableName.ParseToPascalCase();
                                targetPathList = CodeGenTargetPathHelper.BackendChildTableTargetPathList(tableName.ParseToPascalCase(), fileName, templateEntity.WebType);
                                templatePathList = CodeGenTargetPathHelper.BackendChildTableTemplatePathList("SingleTable", templateEntity.WebType);
                                break;
                        }

                        // 生成后端文件
                        for (int i = 0; i < templatePathList.Count; i++)
                        {
                            string tContent = File.ReadAllText(templatePathList[i]);
                            string tResult = _viewEngine.RunCompileFromCached(tContent, new
                            {
                                NameSpace = codeGenConfigModel.NameSpace,
                                BusName = codeGenConfigModel.BusName,
                                ClassName = codeGenConfigModel.ClassName,
                                PrimaryKey = codeGenConfigModel.PrimaryKey,
                                LowerPrimaryKey = codeGenConfigModel.LowerPrimaryKey,
                                OriginalPrimaryKey = codeGenConfigModel.OriginalPrimaryKey,
                                MainTable = codeGenConfigModel.MainTable,
                                LowerMainTable = codeGenConfigModel.LowerMainTable,
                                OriginalMainTableName = codeGenConfigModel.OriginalMainTableName,
                                hasPage = codeGenConfigModel.hasPage,
                                Function = codeGenConfigModel.Function,
                                TableField = codeGenConfigModel.TableField,
                                DefaultSidx = codeGenConfigModel.DefaultSidx,
                                IsExport = codeGenConfigModel.IsExport,
                                IsBatchRemove = codeGenConfigModel.IsBatchRemove,
                                IsUploading = codeGenConfigModel.IsUpload,
                                IsTableRelations = codeGenConfigModel.IsTableRelations,
                                IsMapper = codeGenConfigModel.IsMapper,
                                IsBillRule = codeGenConfigModel.IsBillRule,
                                DbLinkId = codeGenConfigModel.DbLinkId,
                                FlowId = codeGenConfigModel.FlowId,
                                WebType = codeGenConfigModel.WebType,
                                Type = codeGenConfigModel.Type,
                                IsMainTable = codeGenConfigModel.IsMainTable,
                                IsFlowId = codeGenConfigModel.IsFlowId,
                                EnCode = codeGenConfigModel.EnCode,
                                UseDataPermission = useDataPermission,
                                SearchControlNum = codeGenConfigModel.SearchControlNum,
                                IsAuxiliaryTable = codeGenConfigModel.IsAuxiliaryTable,
                                ExportField = codeGenConfigModel.ExportField,
                                FlowIdFieldName = codeGenConfigModel.LowerFlowIdFieldName,
                                TableRelations = codeGenConfigModel.TableRelations,
                                ConfigId = _userManager.TenantId,
                                DBName = _userManager.TenantDbName,
                                PcUseDataPermission = pcColumnDesignModel.useDataPermission ? "true" : "false",
                                AppUseDataPermission = appColumnDesignModel.useDataPermission ? "true" : "false",
                                FullName = codeGenConfigModel.FullName,
                            });
                            var dirPath = new DirectoryInfo(targetPathList[i]).Parent.FullName;
                            if (!Directory.Exists(dirPath))
                                Directory.CreateDirectory(dirPath);
                            File.WriteAllText(targetPathList[i], tResult, Encoding.UTF8);
                        }

                        controls = TemplateAnalysis.AnalysisTemplateData(formDataModel.fields);
                    }
                }

                break;
            case GeneratePatterns.MainBeltVice:
                {
                    var link = await _repository.Context.Queryable<DbLinkEntity>().FirstAsync(m => m.Id == templateEntity.DbLinkId && m.DeleteMark == null);
                    var targetLink = link ?? _databaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
                    tableName = tableRelation.Find(it => it.relationTable.Equals(string.Empty)).table;
                    var fieldList = _databaseManager.GetFieldList(targetLink, tableName);

                    // 副表与主表列表
                    List<DbTableRelationModel> auxiliaryTableList = new List<DbTableRelationModel>();

                    // 副表表字段配置
                    List<TableColumnConfigModel> auxiliaryTableColumnList = new List<TableColumnConfigModel>();

                    // 找副表控件
                    if (controls.Any(x => x.__vModel__.Contains("_qt_")))
                    {
                        var auxiliaryTableName = new List<string>();
                        foreach (var field in controls.FindAll(it => it.__vModel__.Contains("_qt_")).Select(it => it.__vModel__))
                        {
                            var str = field.Matches(@"qt_(?<table>[\s\S]*?)_qt_", "table");
                            auxiliaryTableName.Add(str.FirstOrDefault());
                        }

                        // 为副表列表添加主表信息
                        auxiliaryTableList.Add(tableRelation.Find(it => it.relationTable.Equals(string.Empty)));
                        auxiliaryTableName = auxiliaryTableName.Distinct().ToList();
                        foreach (var item in auxiliaryTableName)
                        {
                            var tableInfo = tableRelation.Find(it => it.table.Equals(item));
                            auxiliaryTableList.Add(tableInfo);
                            tableRelation.Remove(tableInfo);
                        }

                        int tableNo = 1;

                        // 将副表数据添加至主表内
                        foreach (DbTableRelationModel? auxiliay in auxiliaryTableList.FindAll(it => !it.relationTable.Equals(string.Empty)))
                        {
                            List<Systems.Entitys.Model.DataBase.DbTableFieldModel>? auxiliaryTable = _databaseManager.GetFieldList(targetLink, auxiliay.table);
                            var auxiliaryControls = controls.FindAll(it => it.__config__.tableName.Equals(auxiliay.table));

                            codeGenConfigModel = CodeGenWay.AuxiliaryTableBackEnd(auxiliay.table, tableName, auxiliaryTable, controls, templateEntity, tableNo);

                            targetPathList = CodeGenTargetPathHelper.BackendAuxiliaryTargetPathList(auxiliay.table.ParseToPascalCase(), fileName, templateEntity.WebType);
                            templatePathList = CodeGenTargetPathHelper.BackendAuxiliaryTemplatePathList("Auxiliary", templateEntity.WebType);

                            codeGenConfigModel.BusName = auxiliay.tableName;
                            codeGenConfigModel.OriginalMainTableName = auxiliay.table;

                            // 生成副表相关文件
                            for (int i = 0; i < templatePathList.Count; i++)
                            {
                                var tContent = File.ReadAllText(templatePathList[i]);
                                var tResult = _viewEngine.RunCompileFromCached(tContent, new
                                {
                                    BusName = templateEntity.FullName,
                                    ClassName = auxiliay.table.ParseToPascalCase(),
                                    NameSpace = formDataModel.areasName,
                                    PrimaryKey = codeGenConfigModel.TableField.Find(it => it.PrimaryKey).ColumnName,
                                    AuxiliaryTable = auxiliay.table.ParseToPascalCase(),
                                    MainTable = tableName.ParseToPascalCase(),
                                    OriginalMainTableName = codeGenConfigModel.OriginalMainTableName,
                                    TableField = codeGenConfigModel.TableField,
                                    IsUploading = codeGenConfigModel.IsUpload,
                                    IsMapper = true,
                                    WebType = templateEntity.WebType,
                                    Type = templateEntity.Type,
                                });
                                var dirPath = new DirectoryInfo(targetPathList[i]).Parent.FullName;
                                if (!Directory.Exists(dirPath))
                                    Directory.CreateDirectory(dirPath);
                                File.WriteAllText(targetPathList[i], tResult, Encoding.UTF8);
                            }

                            auxiliaryTableColumnList.AddRange(codeGenConfigModel.TableField.FindAll(it => it.qtKey != null));
                            tableNo++;
                        }
                    }

                    // 后端生成
                    codeGenConfigModel = CodeGenWay.MainBeltViceBackEnd(tableName, fieldList, auxiliaryTableColumnList, controls, templateEntity);

                    targetPathList = CodeGenTargetPathHelper.BackendTargetPathList(tableName.ParseToPascalCase(), fileName, templateEntity.WebType);
                    templatePathList = CodeGenTargetPathHelper.BackendTemplatePathList("MainBeltVice", templateEntity.WebType);

                    for (var i = 0; i < templatePathList.Count; i++)
                    {
                        var tContent = File.ReadAllText(templatePathList[i]);
                        var tResult = _viewEngine.RunCompileFromCached(tContent, new
                        {
                            NameSpace = codeGenConfigModel.NameSpace,
                            BusName = codeGenConfigModel.BusName,
                            ClassName = codeGenConfigModel.ClassName,
                            PrimaryKey = codeGenConfigModel.PrimaryKey,
                            LowerPrimaryKey = codeGenConfigModel.LowerPrimaryKey,
                            OriginalPrimaryKey = codeGenConfigModel.OriginalPrimaryKey,
                            MainTable = codeGenConfigModel.MainTable,
                            LowerMainTable = codeGenConfigModel.LowerMainTable,
                            OriginalMainTableName = codeGenConfigModel.OriginalMainTableName,
                            hasPage = codeGenConfigModel.hasPage,
                            Function = codeGenConfigModel.Function,
                            TableField = codeGenConfigModel.TableField,
                            DefaultSidx = codeGenConfigModel.DefaultSidx,
                            IsExport = codeGenConfigModel.IsExport,
                            IsBatchRemove = codeGenConfigModel.IsBatchRemove,
                            IsUploading = codeGenConfigModel.IsUpload,
                            IsTableRelations = codeGenConfigModel.IsTableRelations,
                            IsMapper = codeGenConfigModel.IsMapper,
                            IsBillRule = codeGenConfigModel.IsBillRule,
                            DbLinkId = codeGenConfigModel.DbLinkId,
                            FlowId = codeGenConfigModel.FlowId,
                            WebType = codeGenConfigModel.WebType,
                            Type = codeGenConfigModel.Type,
                            IsMainTable = codeGenConfigModel.IsMainTable,
                            IsFlowId = codeGenConfigModel.IsFlowId,
                            EnCode = codeGenConfigModel.EnCode,
                            UseDataPermission = useDataPermission,
                            SearchControlNum = codeGenConfigModel.SearchControlNum,
                            IsAuxiliaryTable = codeGenConfigModel.IsAuxiliaryTable,
                            ExportField = codeGenConfigModel.ExportField,
                            FlowIdFieldName = codeGenConfigModel.LowerFlowIdFieldName,
                            ConfigId = _userManager.TenantId,
                            DBName = _userManager.TenantDbName,
                            PcUseDataPermission = pcColumnDesignModel.useDataPermission ? "true" : "false",
                            AppUseDataPermission = appColumnDesignModel.useDataPermission ? "true" : "false",
                            AuxiliayTableRelations = CodeGenAuxiliaryTableRelation(auxiliaryTableList.FindAll(it => !it.relationTable.Equals(string.Empty)), targetLink),
                            FullName = codeGenConfigModel.FullName,
                        });
                        var dirPath = new DirectoryInfo(targetPathList[i]).Parent.FullName;
                        if (!Directory.Exists(dirPath))
                            Directory.CreateDirectory(dirPath);
                        File.WriteAllText(targetPathList[i], tResult, Encoding.UTF8);
                    }
                }

                break;
            case GeneratePatterns.PrimarySecondary:
                {
                    {
                        var link = await _repository.Context.Queryable<DbLinkEntity>().FirstAsync(m => m.Id == templateEntity.DbLinkId && m.DeleteMark == null);
                        var targetLink = link ?? _databaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
                        tableName = tableRelation.Find(it => it.relationTable.Equals(string.Empty)).table;

                        // 副表与主表列表
                        List<DbTableRelationModel> auxiliaryTableList = new List<DbTableRelationModel>();

                        // 副表表字段配置
                        List<TableColumnConfigModel> auxiliaryTableColumnList = new List<TableColumnConfigModel>();

                        // 找副表控件
                        if (controls.Any(x => x.__vModel__.Contains("_qt_")))
                        {
                            var auxiliaryTableName = new List<string>();
                            foreach (var field in controls.FindAll(it => it.__vModel__.Contains("_qt_")).Select(it => it.__vModel__))
                            {
                                var str = field.Matches(@"qt_(?<table>[\s\S]*?)_qt_", "table");
                                auxiliaryTableName.Add(str.FirstOrDefault());
                            }

                            // 为副表列表添加主表信息
                            auxiliaryTableList.Add(tableRelation.Find(it => it.relationTable.Equals(string.Empty)));
                            auxiliaryTableName = auxiliaryTableName.Distinct().ToList();
                            foreach (var item in auxiliaryTableName)
                            {
                                var tableInfo = tableRelation.Find(it => it.table.Equals(item));
                                auxiliaryTableList.Add(tableInfo);
                                tableRelation.Remove(tableInfo);
                            }

                            int tableNo = 1;

                            // 将副表数据添加至主表内
                            foreach (DbTableRelationModel? auxiliay in auxiliaryTableList.FindAll(it => !it.relationTable.Equals(string.Empty)))
                            {
                                List<Systems.Entitys.Model.DataBase.DbTableFieldModel>? auxiliaryTable = _databaseManager.GetFieldList(targetLink, auxiliay.table);
                                var auxiliaryControls = controls.FindAll(it => it.__config__.tableName.Equals(auxiliay.table));

                                codeGenConfigModel = CodeGenWay.AuxiliaryTableBackEnd(auxiliay.table, tableName, auxiliaryTable, controls, templateEntity, tableNo);

                                targetPathList = CodeGenTargetPathHelper.BackendAuxiliaryTargetPathList(auxiliay.table.ParseToPascalCase(), fileName, templateEntity.WebType);
                                templatePathList = CodeGenTargetPathHelper.BackendAuxiliaryTemplatePathList("Auxiliary", templateEntity.WebType);

                                codeGenConfigModel.BusName = auxiliay.tableName;
                                codeGenConfigModel.OriginalMainTableName = auxiliay.table;

                                // 生成副表相关文件
                                for (int i = 0; i < templatePathList.Count; i++)
                                {
                                    var tContent = File.ReadAllText(templatePathList[i]);
                                    var tResult = _viewEngine.RunCompileFromCached(tContent, new
                                    {
                                        BusName = templateEntity.FullName,
                                        ClassName = auxiliay.table.ParseToPascalCase(),
                                        NameSpace = formDataModel.areasName,
                                        PrimaryKey = codeGenConfigModel.TableField.Find(it => it.PrimaryKey).ColumnName,
                                        AuxiliaryTable = auxiliay.table.ParseToPascalCase(),
                                        MainTable = tableName.ParseToPascalCase(),
                                        OriginalMainTableName = codeGenConfigModel.OriginalMainTableName,
                                        TableField = codeGenConfigModel.TableField,
                                        IsUploading = codeGenConfigModel.IsUpload,
                                        IsMapper = true,
                                        WebType = templateEntity.WebType,
                                        Type = templateEntity.Type,
                                    });
                                    var dirPath = new DirectoryInfo(targetPathList[i]).Parent.FullName;
                                    if (!Directory.Exists(dirPath))
                                        Directory.CreateDirectory(dirPath);
                                    File.WriteAllText(targetPathList[i], tResult, Encoding.UTF8);
                                }

                                auxiliaryTableColumnList.AddRange(codeGenConfigModel.TableField.FindAll(it => it.qtKey != null));
                                tableNo++;
                            }
                        }

                        foreach (var item in tableRelation)
                        {
                            if (!item.relationTable.Equals(string.Empty))
                            {
                                var children = controls.Find(f => f.__vModel__.Contains("Field") && f.__config__.tableName.Equals(item.table));
                                if (children != null) controls = children.__config__.children;
                            }

                            tableName = item.table;
                            var fieldList = _databaseManager.GetFieldList(targetLink, tableName);

                            switch (item.relationTable.Equals(string.Empty))
                            {
                                // 主表
                                case true:
                                    // 后端生成
                                    codeGenConfigModel = CodeGenWay.PrimarySecondaryBackEnd(tableName, fieldList, auxiliaryTableColumnList, controls, templateEntity, item.relationTable.Equals(string.Empty));
                                    codeGenConfigModel.BusName = item.tableName;
                                    var tableRelations = tableRelation.FindAll(it => !it.relationTable.Equals(string.Empty)) ?? new List<DbTableRelationModel>();
                                    var c = controls.FindAll(it => it.__vModel__.Contains("tableField") || tableRelations.Any(x => x.table == it.__vModel__));
                                    codeGenConfigModel.TableRelations = GetCodeGenTableRelationList(tableRelations, targetLink, c);
                                    //codeGenConfigModel.TableRelations = GetCodeGenTableRelationList(tableRelation.FindAll(it => !it.relationTable.Equals(string.Empty)), targetLink, controls.FindAll(it => it.__vModel__.Contains("tableField")));
                                    targetPathList = CodeGenTargetPathHelper.BackendTargetPathList(tableName.ParseToPascalCase(), fileName, templateEntity.WebType);
                                    templatePathList = CodeGenTargetPathHelper.BackendTemplatePathList("PrimarySecondary", templateEntity.WebType);
                                    break;

                                // 子表
                                default:
                                    // 后端生成
                                    codeGenConfigModel = CodeGenWay.MainBeltBackEnd(tableName, fieldList, controls, templateEntity, item.relationTable.Equals(string.Empty));
                                    codeGenConfigModel.BusName = item.tableName;
                                    codeGenConfigModel.ClassName = tableName.ParseToPascalCase();
                                    targetPathList = CodeGenTargetPathHelper.BackendChildTableTargetPathList(tableName.ParseToPascalCase(), fileName, templateEntity.WebType);
                                    templatePathList = CodeGenTargetPathHelper.BackendChildTableTemplatePathList("SingleTable", templateEntity.WebType);
                                    break;
                            }

                            // 生成后端文件
                            for (int i = 0; i < templatePathList.Count; i++)
                            {
                                string tContent = File.ReadAllText(templatePathList[i]);
                                string tResult = _viewEngine.RunCompileFromCached(tContent, new
                                {
                                    NameSpace = codeGenConfigModel.NameSpace,
                                    BusName = codeGenConfigModel.BusName,
                                    ClassName = codeGenConfigModel.ClassName,
                                    PrimaryKey = codeGenConfigModel.PrimaryKey,
                                    LowerPrimaryKey = codeGenConfigModel.LowerPrimaryKey,
                                    OriginalPrimaryKey = codeGenConfigModel.OriginalPrimaryKey,
                                    MainTable = codeGenConfigModel.MainTable,
                                    LowerMainTable = codeGenConfigModel.LowerMainTable,
                                    OriginalMainTableName = codeGenConfigModel.OriginalMainTableName,
                                    hasPage = codeGenConfigModel.hasPage,
                                    Function = codeGenConfigModel.Function,
                                    TableField = codeGenConfigModel.TableField,
                                    DefaultSidx = codeGenConfigModel.DefaultSidx,
                                    IsExport = codeGenConfigModel.IsExport,
                                    IsBatchRemove = codeGenConfigModel.IsBatchRemove,
                                    IsUploading = codeGenConfigModel.IsUpload,
                                    IsTableRelations = codeGenConfigModel.IsTableRelations,
                                    IsMapper = codeGenConfigModel.IsMapper,
                                    IsBillRule = codeGenConfigModel.IsBillRule,
                                    DbLinkId = codeGenConfigModel.DbLinkId,
                                    FlowId = codeGenConfigModel.FlowId,
                                    WebType = codeGenConfigModel.WebType,
                                    Type = codeGenConfigModel.Type,
                                    IsMainTable = codeGenConfigModel.IsMainTable,
                                    IsFlowId = codeGenConfigModel.IsFlowId,
                                    EnCode = codeGenConfigModel.EnCode,
                                    UseDataPermission = useDataPermission,
                                    SearchControlNum = codeGenConfigModel.SearchControlNum,
                                    IsAuxiliaryTable = codeGenConfigModel.IsAuxiliaryTable,
                                    ExportField = codeGenConfigModel.ExportField,
                                    FlowIdFieldName = codeGenConfigModel.LowerFlowIdFieldName,
                                    TableRelations = codeGenConfigModel.TableRelations,
                                    ConfigId = _userManager.TenantId,
                                    DBName = _userManager.TenantDbName,
                                    PcUseDataPermission = pcColumnDesignModel.useDataPermission ? "true" : "false",
                                    AppUseDataPermission = appColumnDesignModel.useDataPermission ? "true" : "false",
                                    AuxiliayTableRelations = CodeGenAuxiliaryTableRelation(auxiliaryTableList.FindAll(it => !it.relationTable.Equals(string.Empty)), targetLink),
                                    FullName = codeGenConfigModel.FullName,
                                });
                                var dirPath = new DirectoryInfo(targetPathList[i]).Parent.FullName;
                                if (!Directory.Exists(dirPath))
                                    Directory.CreateDirectory(dirPath);
                                File.WriteAllText(targetPathList[i], tResult, Encoding.UTF8);
                            }

                            controls = TemplateAnalysis.AnalysisTemplateData(formDataModel.fields);
                        }
                    }
                }

                break;
            default:
                {
                    tableName = tableRelation.FirstOrDefault().table;
                    var link = await _repository.Context.Queryable<DbLinkEntity>().FirstAsync(m => m.Id == templateEntity.DbLinkId && m.DeleteMark == null);
                    var targetLink = link ?? _databaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
                    var fieldList = _databaseManager.GetFieldList(targetLink, tableName);

                    // 后端生成
                    codeGenConfigModel = CodeGenWay.SingleTableBackEnd(tableName, fieldList, controls, templateEntity);
                    switch (templateEntity.Type)
                    {
                        case 3:
                            targetPathList = CodeGenTargetPathHelper.BackendFlowTargetPathList(tableName.ParseToPascalCase(), fileName);
                            templatePathList = CodeGenTargetPathHelper.BackendFlowTemplatePathList("SingleTable");
                            break;
                        default:
                            targetPathList = CodeGenTargetPathHelper.BackendTargetPathList(tableName.ParseToPascalCase(), fileName, templateEntity.WebType);
                            templatePathList = CodeGenTargetPathHelper.BackendTemplatePathList("SingleTable", templateEntity.WebType);
                            break;
                    }

                    targetPathList = CodeGenTargetPathHelper.BackendTargetPathList(tableName.ParseToPascalCase(), fileName, templateEntity.WebType);
                    templatePathList = CodeGenTargetPathHelper.BackendTemplatePathList("SingleTable", templateEntity.WebType);

                    // 生成后端文件
                    for (var i = 0; i < templatePathList.Count; i++)
                    {
                        var tContent = File.ReadAllText(templatePathList[i]);
                        var tResult = _viewEngine.RunCompileFromCached(tContent, new
                        {
                            NameSpace = codeGenConfigModel.NameSpace,
                            BusName = codeGenConfigModel.BusName,
                            ClassName = codeGenConfigModel.ClassName,
                            PrimaryKey = codeGenConfigModel.PrimaryKey,
                            LowerPrimaryKey = codeGenConfigModel.LowerPrimaryKey,
                            OriginalPrimaryKey = codeGenConfigModel.OriginalPrimaryKey,
                            MainTable = codeGenConfigModel.MainTable,
                            LowerMainTable = codeGenConfigModel.LowerMainTable,
                            OriginalMainTableName = codeGenConfigModel.OriginalMainTableName,
                            hasPage = codeGenConfigModel.hasPage,
                            Function = codeGenConfigModel.Function,
                            TableField = codeGenConfigModel.TableField,
                            DefaultSidx = codeGenConfigModel.DefaultSidx,
                            IsExport = codeGenConfigModel.IsExport,
                            IsBatchRemove = codeGenConfigModel.IsBatchRemove,
                            IsUploading = codeGenConfigModel.IsUpload,
                            IsTableRelations = codeGenConfigModel.IsTableRelations,
                            IsMapper = codeGenConfigModel.IsMapper,
                            IsBillRule = codeGenConfigModel.IsBillRule,
                            DbLinkId = codeGenConfigModel.DbLinkId,
                            FlowId = codeGenConfigModel.FlowId,
                            WebType = codeGenConfigModel.WebType,
                            Type = codeGenConfigModel.Type,
                            IsMainTable = codeGenConfigModel.IsMainTable,
                            IsFlowId = codeGenConfigModel.IsFlowId,
                            EnCode = codeGenConfigModel.EnCode,
                            UseDataPermission = useDataPermission,
                            SearchControlNum = codeGenConfigModel.SearchControlNum,
                            IsAuxiliaryTable = codeGenConfigModel.IsAuxiliaryTable,
                            ExportField = codeGenConfigModel.ExportField,
                            FlowIdFieldName = codeGenConfigModel.LowerFlowIdFieldName,
                            ConfigId = _userManager.TenantId,
                            DBName = _userManager.TenantDbName,
                            PcUseDataPermission = pcColumnDesignModel.useDataPermission ? "true" : "false",
                            AppUseDataPermission = appColumnDesignModel.useDataPermission ? "true" : "false",
                            FullName = codeGenConfigModel.FullName,
                            TableRelations = codeGenConfigModel.TableRelations,
                        });
                        var dirPath = new DirectoryInfo(targetPathList[i]).Parent.FullName;
                        if (!Directory.Exists(dirPath))
                            Directory.CreateDirectory(dirPath);
                        File.WriteAllText(targetPathList[i], tResult, Encoding.UTF8);
                    }
                }

                break;
        }

        // 强行将文件夹名称定义成主表名称
        tableName = tableRelation.Find(it => it.relationTable.Equals(string.Empty)).table;

        // 生成前端
        await GenFrondEnd(tableName.ParseToPascalCase().ToLowerCase(), codeGenConfigModel.DefaultSidx, formDataModel, controls, codeGenConfigModel.TableField, templateEntity, fileName);
    }

    /// <summary>
    /// 预览代码.
    /// </summary>
    /// <param name="codePath"></param>
    /// <returns></returns>
    private List<Dictionary<string, object>> PriviewCode(string codePath)
    {
        var dataList = FileHelper.GetAllFiles(codePath);
        List<Dictionary<string, string>> datas = new List<Dictionary<string, string>>();
        List<Dictionary<string, object>> allDatas = new List<Dictionary<string, object>>();
        foreach (var item in dataList)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            FileStream fileStream = new FileStream(item.FullName, FileMode.Open);
            using (StreamReader reader = new StreamReader(fileStream))
            {
                var buffer = new char[(int)reader.BaseStream.Length];
                reader.Read(buffer, 0, (int)reader.BaseStream.Length);
                var content = new string(buffer);
                if ("cs".Equals(item.Extension.Replace(".", string.Empty)))
                {
                    string fileName = item.FullName.ToLower();
                    if (fileName.Contains("listqueryinput") || fileName.Contains("crinput") || fileName.Contains("upinput") || fileName.Contains("upoutput") || fileName.Contains("listoutput") || fileName.Contains("infooutput"))
                    {
                        data.Add("folderName", "dto");
                    }
                    else if (fileName.Contains("mapper"))
                    {
                        data.Add("folderName", "mapper");
                    }
                    else if (fileName.Contains("entity"))
                    {
                        data.Add("folderName", "entity");
                    }
                    else
                    {
                        data.Add("folderName", "dotnet");
                    }

                    data.Add("fileName", item.Name);

                    // 剔除"\0"特殊符号
                    data.Add("fileContent", content.Replace("\0", string.Empty));
                    data.Add("fileType", item.Extension.Replace(".", string.Empty));
                    datas.Add(data);
                }
                else if (".ffe".Equals(item.Extension))
                {
                    data.Add("folderName", "ffe");
                    data.Add("id", SnowflakeIdHelper.NextId());
                    data.Add("fileName", item.Name);

                    // 剔除"\0"特殊符号
                    data.Add("fileContent", content.Replace("\0", string.Empty));
                    data.Add("fileType", item.Extension.Replace(".", string.Empty));
                    datas.Add(data);

                }
                else if (".vue".Equals(item.Extension))
                {
                    if (item.FullName.ToLower().Contains("app"))
                        data.Add("folderName", "app");
                    else if (item.FullName.ToLower().Contains("pc"))
                        data.Add("folderName", "web");

                    data.Add("id", SnowflakeIdHelper.NextId());
                    data.Add("fileName", item.Name);

                    // 剔除"\0"特殊符号
                    data.Add("fileContent", content.Replace("\0", string.Empty));
                    data.Add("fileType", item.Extension.Replace(".", string.Empty));
                    datas.Add(data);
                }
            }
        }

        // datas 集合去重
        foreach (var item in datas.GroupBy(d => d["folderName"]).Select(d => d.First()).OrderBy(d => d["folderName"]).ToList())
        {
            Dictionary<string, object> dataMap = new Dictionary<string, object>();
            dataMap["fileName"] = item["folderName"];
            dataMap["id"] = SnowflakeIdHelper.NextId();
            dataMap["children"] = datas.FindAll(d => d["folderName"] == item["folderName"]);
            allDatas.Add(dataMap);
        }

        return allDatas;
    }

    /// <summary>
    /// 判断生成模式.
    /// </summary>
    /// <returns>1-纯主表、2-主带子、3-主带副、4-主带副与子.</returns>
    private GeneratePatterns JudgmentGenerationModel(List<DbTableRelationModel> tableRelation, List<FieldsModel> controls)
    {
        // 默认纯主表
        var codeModel = GeneratePatterns.PrimaryTable;

        // 找副表控件
        if (tableRelation.Count > 1 && controls.Any(x => x.__vModel__.Contains("_qt_")) && controls.Any(it => it.__config__.qtKey.Equals(QtKeyConst.TABLE)))
            codeModel = GeneratePatterns.PrimarySecondary;
        else if (tableRelation.Count > 1 && controls.Any(x => x.__vModel__.Contains("_qt_")))
            codeModel = GeneratePatterns.MainBeltVice;
        else if (tableRelation.Count > 1 && controls.Any(it => it.__config__.qtKey.Equals(QtKeyConst.TABLE)))
            codeModel = GeneratePatterns.MainBelt;
        return codeModel;
    }

    /// <summary>
    /// 生成前端.
    /// </summary>
    /// <param name="tableName">表名称.</param>
    /// <param name="defaultSidx">默认排序.</param>
    /// <param name="formDataModel">表单JSON包.</param>
    /// <param name="controls">移除布局控件后的控件列表.</param>
    /// <param name="tableColumns">表字段.</param>
    /// <param name="templateEntity">模板实体.</param>
    /// <param name="fileName">文件夹名称.</param>
    private async Task GenFrondEnd(string tableName, string defaultSidx, FormDataModel formDataModel, List<FieldsModel> controls, List<TableColumnConfigModel> tableColumns, VisualDevEntity templateEntity, string fileName)
    {
        var categoryName = (await _dictionaryDataService.GetInfo(templateEntity.Category)).EnCode;
        List<string> targetPathList = new List<string>();
        List<string> templatePathList = new List<string>();

        FrontEndGenConfigModel frondEndGenConfig = new FrontEndGenConfigModel();

        switch (templateEntity.Type)
        {
            case 3:
                {
                    frondEndGenConfig = CodeGenWay.SingleTableFrontEnd(4, formDataModel, controls, tableColumns, templateEntity);

                    targetPathList = CodeGenTargetPathHelper.FlowFrontEndTargetPathList(tableName, fileName);
                    templatePathList = CodeGenTargetPathHelper.FlowFrontEndTemplatePathList();

                    for (var i = 0; i < templatePathList.Count; i++)
                    {
                        var tContent = File.ReadAllText(templatePathList[i]);
                        var tResult = _viewEngine.RunCompileFromCached(tContent, new
                        {
                            NameSpace = frondEndGenConfig.NameSpace,
                            ClassName = frondEndGenConfig.ClassName,
                            FormRef = frondEndGenConfig.FormRef,
                            FormModel = frondEndGenConfig.FormModel,
                            Size = frondEndGenConfig.Size,
                            LabelPosition = frondEndGenConfig.LabelPosition,
                            LabelWidth = frondEndGenConfig.LabelWidth,
                            FormRules = frondEndGenConfig.FormRules,
                            GeneralWidth = frondEndGenConfig.GeneralWidth,
                            FullScreenWidth = frondEndGenConfig.FullScreenWidth,
                            DrawerWidth = frondEndGenConfig.DrawerWidth,
                            FormStyle = frondEndGenConfig.FormStyle,
                            Type = frondEndGenConfig.Type,
                            TreeRelation = frondEndGenConfig.TreeRelation,
                            TreeTitle = frondEndGenConfig.TreeTitle,
                            TreePropsValue = frondEndGenConfig.TreePropsValue,
                            TreeDataSource = frondEndGenConfig.TreeDataSource,
                            TreeDictionary = frondEndGenConfig.TreeDictionary,
                            TreePropsUrl = frondEndGenConfig.TreePropsUrl,
                            TreePropsLabel = frondEndGenConfig.TreePropsLabel,
                            TreePropsChildren = frondEndGenConfig.TreePropsChildren,
                            IsExistQuery = frondEndGenConfig.IsExistQuery,
                            PrimaryKey = frondEndGenConfig.PrimaryKey,
                            FormList = frondEndGenConfig.FormList,
                            PopupType = frondEndGenConfig.PopupType,
                            SearchColumnDesign = frondEndGenConfig.SearchColumnDesign,
                            TopButtonDesign = frondEndGenConfig.TopButtonDesign,
                            ColumnButtonDesign = frondEndGenConfig.ColumnButtonDesign,
                            ColumnDesign = frondEndGenConfig.ColumnDesign,
                            OptionsList = frondEndGenConfig.OptionsList,
                            IsBatchRemoveDel = frondEndGenConfig.IsBatchRemoveDel,
                            IsDownload = frondEndGenConfig.IsDownload,
                            IsRemoveDel = frondEndGenConfig.IsRemoveDel,
                            IsDetail = frondEndGenConfig.IsDetail,
                            IsEdit = frondEndGenConfig.IsEdit,
                            IsAdd = frondEndGenConfig.IsAdd,
                            IsSort = frondEndGenConfig.IsSort,
                            FormAllContols = frondEndGenConfig.FormAllContols,
                            CancelButtonText = frondEndGenConfig.CancelButtonText,
                            ConfirmButtonText = frondEndGenConfig.ConfirmButtonText,
                            UseBtnPermission = frondEndGenConfig.UseBtnPermission,
                            UseColumnPermission = frondEndGenConfig.UseColumnPermission,
                            UseFormPermission = frondEndGenConfig.UseFormPermission,
                            DefaultSidx = defaultSidx,
                            WebType = templateEntity.WebType,
                            HasPage = frondEndGenConfig.HasPage,
                            IsSummary = frondEndGenConfig.IsSummary,
                            AddTitleName = frondEndGenConfig.TopButtonDesign.Find(it => it.Value.Equals("add"))?.Label,
                            EditTitleName = frondEndGenConfig.ColumnButtonDesign.Find(it => it.Value.Equals("edit"))?.Label,
                            DetailTitleName = frondEndGenConfig.ColumnButtonDesign.Find(it => it.IsDetail.Equals(true))?.Label,
                            PageSize = frondEndGenConfig.PageSize,
                            Sort = frondEndGenConfig.Sort,
                            HasPrintBtn = frondEndGenConfig.HasPrintBtn,
                            PrintButtonText = frondEndGenConfig.PrintButtonText,
                            PrintId = frondEndGenConfig.PrintId,
                            EnCode = templateEntity.EnCode,
                            FlowId = templateEntity.Id,
                        }, builderAction: builder =>
                        {
                            builder.AddUsing("QT.VisualDev.Engine.Model.CodeGen");
                            builder.AddAssemblyReferenceByName("QT.VisualDev.Engine");
                        });
                        var dirPath = new DirectoryInfo(targetPathList[i]).Parent.FullName;
                        if (!Directory.Exists(dirPath))
                            Directory.CreateDirectory(dirPath);
                        File.WriteAllText(targetPathList[i], tResult, Encoding.UTF8);
                    }
                }

                break;
            default:
                {
                    // 前端生成 APP与PC合并
                    foreach (int logic in new List<int> { 5, 4 })
                    {
                        // 每次循环前重新定义表单数据
                        formDataModel = templateEntity.FormData.ToObject<FormDataModel>();

                        // 根据生成逻辑生成前端
                        frondEndGenConfig = CodeGenWay.SingleTableFrontEnd(logic, formDataModel, controls, tableColumns, templateEntity);
                        switch (logic)
                        {
                            case 4:
                                targetPathList = CodeGenTargetPathHelper.FrontEndTargetPathList(tableName, fileName, templateEntity.WebType, frondEndGenConfig.IsDetail);
                                templatePathList = CodeGenTargetPathHelper.FrontEndTemplatePathList(templateEntity.WebType, frondEndGenConfig.IsDetail);
                                break;
                            case 5:
                                targetPathList = CodeGenTargetPathHelper.AppFrontEndTargetPathList(tableName, fileName, templateEntity.WebType);
                                templatePathList = CodeGenTargetPathHelper.AppFrontEndTemplatePathList(templateEntity.WebType);
                                break;
                        }

                        for (int i = 0; i < templatePathList.Count; i++)
                        {
                            string tContent = File.ReadAllText(templatePathList[i]);
                            var tResult = _viewEngine.RunCompileFromCached(tContent, new
                            {
                                NameSpace = frondEndGenConfig.NameSpace,
                                ClassName = frondEndGenConfig.ClassName,
                                FormRef = frondEndGenConfig.FormRef,
                                FormModel = frondEndGenConfig.FormModel,
                                Size = frondEndGenConfig.Size,
                                LabelPosition = frondEndGenConfig.LabelPosition,
                                LabelWidth = frondEndGenConfig.LabelWidth,
                                FormRules = frondEndGenConfig.FormRules,
                                GeneralWidth = frondEndGenConfig.GeneralWidth,
                                FullScreenWidth = frondEndGenConfig.FullScreenWidth,
                                DrawerWidth = frondEndGenConfig.DrawerWidth,
                                FormStyle = frondEndGenConfig.FormStyle,
                                Type = frondEndGenConfig.Type,
                                TreeRelation = frondEndGenConfig.TreeRelation,
                                TreeTitle = frondEndGenConfig.TreeTitle,
                                TreePropsValue = frondEndGenConfig.TreePropsValue,
                                TreeDataSource = frondEndGenConfig.TreeDataSource,
                                TreeDictionary = frondEndGenConfig.TreeDictionary,
                                TreePropsUrl = frondEndGenConfig.TreePropsUrl,
                                TreePropsLabel = frondEndGenConfig.TreePropsLabel,
                                TreePropsChildren = frondEndGenConfig.TreePropsChildren,
                                IsExistQuery = frondEndGenConfig.IsExistQuery,
                                PrimaryKey = frondEndGenConfig.PrimaryKey,
                                FormList = frondEndGenConfig.FormList,
                                PopupType = frondEndGenConfig.PopupType,
                                SearchColumnDesign = frondEndGenConfig.SearchColumnDesign,
                                TopButtonDesign = frondEndGenConfig.TopButtonDesign,
                                ColumnButtonDesign = frondEndGenConfig.ColumnButtonDesign,
                                ColumnDesign = frondEndGenConfig.ColumnDesign,
                                OptionsList = frondEndGenConfig.OptionsList,
                                IsBatchRemoveDel = frondEndGenConfig.IsBatchRemoveDel,
                                IsDownload = frondEndGenConfig.IsDownload,
                                IsRemoveDel = frondEndGenConfig.IsRemoveDel,
                                IsDetail = frondEndGenConfig.IsDetail,
                                IsEdit = frondEndGenConfig.IsEdit,
                                IsAdd = frondEndGenConfig.IsAdd,
                                IsSort = frondEndGenConfig.IsSort,
                                FormAllContols = frondEndGenConfig.FormAllContols,
                                CancelButtonText = frondEndGenConfig.CancelButtonText,
                                ConfirmButtonText = frondEndGenConfig.ConfirmButtonText,
                                UseBtnPermission = frondEndGenConfig.UseBtnPermission,
                                UseColumnPermission = frondEndGenConfig.UseColumnPermission,
                                UseFormPermission = frondEndGenConfig.UseFormPermission,
                                DefaultSidx = defaultSidx,
                                WebType = templateEntity.WebType,
                                HasPage = frondEndGenConfig.HasPage,
                                IsSummary = frondEndGenConfig.IsSummary,
                                AddTitleName = frondEndGenConfig.TopButtonDesign?.Find(it => it.Value.Equals("add"))?.Label,
                                EditTitleName = frondEndGenConfig.ColumnButtonDesign?.Find(it => it.Value.Equals("edit"))?.Label,
                                DetailTitleName = frondEndGenConfig.ColumnButtonDesign?.Find(it => it.IsDetail.Equals(true))?.Label,
                                PageSize = frondEndGenConfig.PageSize,
                                Sort = frondEndGenConfig.Sort,
                                HasPrintBtn = frondEndGenConfig.HasPrintBtn,
                                PrintButtonText = frondEndGenConfig.PrintButtonText,
                                PrintId = frondEndGenConfig.PrintId,
                                EnCode = templateEntity.EnCode,
                                FlowId = templateEntity.Id,
                                FullName = templateEntity.FullName,
                                Category = categoryName,
                                FlowTemplateJson = templateEntity.FlowTemplateJson.ToJsonString(),
                                Tables = templateEntity.Tables.ToJsonString(),
                                DbLinkId = templateEntity.DbLinkId,
                                MianTable = tableName,
                                CreatorTime = DateTime.Now.ParseToUnixTime(),
                                CreatorUserId = _userManager.UserId
                            }, builderAction: builder =>
                            {
                                builder.AddUsing("QT.VisualDev.Engine.Model.CodeGen");
                                builder.AddAssemblyReferenceByName("QT.VisualDev.Engine");
                            });
                            var dirPath = new DirectoryInfo(targetPathList[i]).Parent.FullName;
                            if (!Directory.Exists(dirPath))
                                Directory.CreateDirectory(dirPath);
                            File.WriteAllText(targetPathList[i], tResult, Encoding.UTF8);
                        }
                    }
                }

                break;
        }
    }

    /// <summary>
    /// 获取代码生成表关系列表.
    /// </summary>
    /// <param name="tableRelation">全部表列表.</param>
    /// <param name="link">连接ID.</param>
    /// <param name="fieldList">子表控件.</param>
    /// <returns></returns>
    private List<CodeGenTableRelationsModel> GetCodeGenTableRelationList(List<DbTableRelationModel> tableRelation, DbLinkEntity link, List<FieldsModel> fieldList)

    {
        List<CodeGenTableRelationsModel> tableRelationsList = new List<CodeGenTableRelationsModel>();
        if (tableRelation.Count > 0)
        {
            var tableNo = 1;
            foreach (var table in tableRelation)
            {
                List<TableColumnConfigModel> tableColumnConfigList = new List<TableColumnConfigModel>();
                if (fieldList.Any(it => it.__config__.tableName.Equals(table.table)))
                {
                    var tableControl = fieldList.Find(it => it.__config__.tableName.Equals(table.table));
                    var tableControls = tableControl.__config__.children;
                    var tableField = _databaseManager.GetFieldList(link, table.table);
                    foreach (var column in tableField)
                    {
                        var field = column.field.ReplaceRegex("^f_", string.Empty).ParseToPascalCase().ToLowerCase();
                        switch (column.primaryKey)
                        {
                            case true:
                                tableColumnConfigList.Add(new TableColumnConfigModel()
                                {
                                    ColumnName = field.ToUpperCase(),
                                    OriginalColumnName = column.field,
                                    ColumnComment = column.fieldName,
                                    DataType = column.dataType,
                                    NetType = CodeGenHelper.ConvertDataType(column.dataType),
                                    PrimaryKey = true,
                                });
                                break;
                            default:
                                if (tableControls.Any(c => c.__vModel__ == field))
                                {
                                    FieldsModel control = tableControls.Find(c => c.__vModel__ == field);
                                    tableColumnConfigList.Add(new TableColumnConfigModel()
                                    {
                                        ColumnName = field.ToUpperCase(),
                                        OriginalColumnName = column.field,
                                        ColumnComment = column.fieldName,
                                        DataType = column.dataType,
                                        NetType = CodeGenHelper.ConvertDataType(column.dataType),
                                        PrimaryKey = column.primaryKey.ParseToBool(),
                                        IsMultiple = CodeGenFieldJudgeHelper.IsMultipleColumn(fieldList, field),
                                        qtKey = control.__config__.qtKey,
                                        Rule = control.__config__.rule,
                                        IsDateTime = CodeGenFieldJudgeHelper.IsDateTime(control),
                                    });
                                }

                                break;
                        }
                    }

                    tableRelationsList.Add(new CodeGenTableRelationsModel()
                    {
                        TableName = table.table.ParseToPascalCase(),
                        PrimaryKey = tableField.Find(it => it.primaryKey).field.ReplaceRegex("^f_", string.Empty).ParseToPascalCase(),
                        TableField = table.tableField.ReplaceRegex("^f_", string.Empty).ParseToPascalCase(),
                        RelationField = table.relationField.ReplaceRegex("^f_", string.Empty).ParseToPascalCase(),
                        TableComment = table.tableName,
                        ChilderColumnConfigList = tableColumnConfigList,
                        TableNo = tableNo,
                        ControlModel = tableControl.__vModel__,
                    });
                }

                tableNo++;
            }
        }

        return tableRelationsList;
    }

    /// <summary>
    /// 代码生成副表表关系.
    /// </summary>
    /// <param name="tableRelation">全部副表关系.</param>
    /// <param name="link">连接ID.</param>
    /// <returns></returns>
    private List<CodeGenTableRelationsModel> CodeGenAuxiliaryTableRelation(List<DbTableRelationModel> tableRelation, DbLinkEntity link)
    {
        List<CodeGenTableRelationsModel> tableRelationsList = new List<CodeGenTableRelationsModel>();
        if (tableRelation.Count > 0)
        {
            int tableNo = 1;
            foreach (var table in tableRelation)
            {
                var tableField = _databaseManager.GetFieldList(link, table.table);
                tableRelationsList.Add(new CodeGenTableRelationsModel()
                {
                    TableName = table.table.ParseToPascalCase(),
                    OriginalTableName = table.table,
                    PrimaryKey = tableField.Find(it => it.primaryKey).field.ReplaceRegex("^f_", string.Empty).ParseToPascalCase(),
                    TableField = table.tableField.ReplaceRegex("^f_", string.Empty).ParseToPascalCase(),
                    RelationField = table.relationField.ReplaceRegex("^f_", string.Empty).ParseToPascalCase(),
                    TableComment = table.tableName,
                    TableNo = tableNo
                });
                tableNo++;
            }
        }

        return tableRelationsList;
    }

    #endregion
}