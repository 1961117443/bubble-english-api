using QT.Common.Core.Manager;
using QT.Common.Core.Manager.Files;
using QT.Common.Dtos.DataBase;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Dto.Database;
using QT.Systems.Entitys.Model.DataBase;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.Common;
using QT.Systems.Interfaces.System;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System.Collections;
using System.Data;
using System.Text;

namespace QT.Systems;

/// <summary>
/// 数据管理



/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "DataModel", Order = 208)]
[Route("api/system/[controller]")]
public class DataBaseService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    public readonly ISqlSugarRepository<DbLinkEntity> _repository;

    /// <summary>
    /// 数据连接服务.
    /// </summary>
    private readonly IDbLinkService _dbLinkService;

    /// <summary>
    /// 文件服务.
    /// </summary>
    private readonly IFileManager _fileManager;

    /// <summary>
    /// 数据库管理.
    /// </summary>
    private readonly IDataBaseManager _dataBaseManager;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="DataBaseService"/>类型的新实例.
    /// </summary>
    public DataBaseService(
        ISqlSugarRepository<DbLinkEntity> dbLinkRepository,
        IDbLinkService dbLinkService,
        IFileManager fileManager,
        IDataBaseManager dataBaseManager,
        IUserManager userManager)
    {
        _repository = dbLinkRepository;
        _dbLinkService = dbLinkService;
        _fileManager = fileManager;
        _dataBaseManager = dataBaseManager;
        _userManager = userManager;
    }

    #region GET

    /// <summary>
    /// 表名列表.
    /// </summary>
    /// <param name="id">连接Id.</param>
    /// <param name="input">过滤条件.</param>
    /// <returns></returns>
    [HttpGet("{id}/Tables")]
    public async Task<dynamic> GetList(string id, [FromQuery] KeywordInput input)
    {
        try
        {
            var link = await _dbLinkService.GetInfo(id);
            var tenantLink = link ?? _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
            var tables = _dataBaseManager.GetTableInfos(tenantLink);
            tables = tables.Where((x, i) => tables.FindIndex(z => z.Name == x.Name) == i).ToList();
            var output = tables.Adapt<List<DatabaseTableListOutput>>();
            if (!string.IsNullOrEmpty(input.keyword))
                output = output.FindAll(d => d.table.ToLower().Contains(input.keyword.ToLower()) || (d.tableName.IsNotEmptyOrNull() && d.tableName.ToLower().Contains(input.keyword.ToLower())));
            GetTableCount(output);
            return new { list = output.OrderBy(x => x.table).ToList() };
        }
        catch (Exception ex)
        {
            return new { list = new List<DatabaseTableListOutput>() };
        }
    }

    /// <summary>
    /// 表名列表.
    /// </summary>
    /// <param name="id">连接Id.</param>
    /// <param name="input">过滤条件.</param>
    /// <returns></returns>
    [HttpGet("{id}/Views")]
    public async Task<dynamic> GetViewList(string id, [FromQuery] KeywordInput input)
    {
        try
        {
            var link = await _dbLinkService.GetInfo(id);
            var tenantLink = link ?? _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
            var tables = _dataBaseManager.GetViewInfos(tenantLink);
            tables = tables.Where((x, i) => tables.FindIndex(z => z.Name == x.Name) == i).ToList();
            var output = tables.Adapt<List<DatabaseTableListOutput>>();
            if (!string.IsNullOrEmpty(input.keyword))
                output = output.FindAll(d => d.table.ToLower().Contains(input.keyword.ToLower()) || (d.tableName.IsNotEmptyOrNull() && d.tableName.ToLower().Contains(input.keyword.ToLower())));
            GetTableCount(output);
            return new { list = output.OrderBy(x => x.table).ToList() };
        }
        catch (Exception ex)
        {
            return new { list = new List<DatabaseTableListOutput>() };
        }
    }

    /// <summary>
    /// 预览数据.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <param name="linkId">连接Id.</param>
    /// <param name="tableName">表名.</param>
    /// <returns></returns>
    [HttpGet("{linkId}/Table/{tableName}/Preview")]
    public async Task<dynamic> GetData([FromQuery] DatabaseTablePreviewQuery input, string linkId, string tableName)
    {
        var link = await _dbLinkService.GetInfo(linkId);
        if (string.IsNullOrEmpty(tableName)) return new PageResult();
        var tenantLink = link ?? _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
        StringBuilder dbSql = new StringBuilder();
        dbSql.AppendFormat("SELECT * FROM {0} WHERE 1=1", tableName);
        if (!string.IsNullOrEmpty(input.field) && !string.IsNullOrEmpty(input.keyword))
            dbSql.AppendFormat(" AND {0} like '%{1}%'", input.field, input.keyword);

        return await _dataBaseManager.GetDataTablePage(tenantLink, dbSql.ToString(), input.currentPage, input.pageSize);
    }

    /// <summary>
    /// 字段列表.
    /// </summary>
    /// <param name="linkId">连接Id.</param>
    /// <param name="tableName">表名.</param>
    /// <param name="type">字段类型.</param>
    /// <returns></returns>
    [HttpGet("{linkId}/Tables/{tableName}/Fields")]
    public async Task<dynamic> GetFieldList(string linkId, string tableName, [FromQuery] string type)
    {
        var link = await _dbLinkService.GetInfo(linkId);
        if (string.IsNullOrEmpty(tableName)) return new PageResult();
        var tenantLink = link ?? _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
        var data = _dataBaseManager.GetFieldList(tenantLink, tableName).Adapt<List<TableFieldOutput>>();
        // 删除内置的字段 __开头
        for (var i = data.Count - 1; i >= 0; i--)
        {
            var field = data[i];
            if (field.field.StartsWith("__"))
            {
                data.RemoveAt(i);
            }
        }
        if (type.Equals("1"))
        {
            data.ForEach(item =>
            {
                item.field = item.field.ReplaceRegex("^f_", string.Empty).ParseToPascalCase().ToLowerCase();
            });
        }

        return new { list = data };
    }

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="linkId">连接Id.</param>
    /// <param name="tableName">主键值.</param>
    /// <returns></returns>
    [HttpGet("{linkId}/Table/{tableName}")]
    public async Task<dynamic> GetInfo(string linkId, string tableName)
    {
        var link = await _dbLinkService.GetInfo(linkId);
        if (string.IsNullOrEmpty(tableName)) return new PageResult();
        var tenantLink = link ?? _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
        return _dataBaseManager.GetDataBaseTableInfo(tenantLink, tableName);
    }

    /// <summary>
    /// 获取数据库表字段下拉框列表.
    /// </summary>
    /// <param name="linkId">连接Id.</param>
    /// <param name="tableName">表名.</param>
    /// <returns></returns>
    [HttpGet("{DBId}/Tables/{tableName}/Fields/Selector")]
    public async Task<dynamic> SelectorData(string linkId, string tableName)
    {
        var link = await _dbLinkService.GetInfo(linkId);
        if (string.IsNullOrEmpty(tableName)) return new PageResult();
        var tenantLink = link ?? _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
        var data = _dataBaseManager.GetDBTableList(tenantLink).FindAll(m => m.table == tableName).Adapt<List<DatabaseTableFieldsSelectorOutput>>();
        return new { list = data };
    }

    /// <summary>
    /// 导出.
    /// </summary>
    /// <param name="linkId">连接ID.</param>
    /// <param name="tableName">表名称.</param>
    /// <returns></returns>
    [HttpGet("{linkId}/Table/{tableName}/Action/Export")]
    public async Task<dynamic> ActionsExport(string linkId, string tableName)
    {
        var link = await _dbLinkService.GetInfo(linkId);
        var tenantLink = link ?? _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
        var data = _dataBaseManager.GetDataBaseTableInfo(tenantLink, tableName);
        var jsonStr = data.ToJsonString();
        return await _fileManager.Export(jsonStr, data.tableInfo.table, ExportFileType.bdb);
    }

    #endregion

    #region POST

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="linkId">连接Id.</param>
    /// <param name="tableName">表名.</param>
    /// <returns></returns>
    [HttpDelete("{linkId}/Table/{tableName}")]
    public async Task Delete(string linkId, string tableName)
    {
        if (IsSysTable(tableName))
            throw Oops.Oh(ErrorCode.D1504);
        var link = await _dbLinkService.GetInfo(linkId);
        var tenantLink = link ?? _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
        var data = _dataBaseManager.GetData(tenantLink, tableName);
        if (data.Rows.Count > 0)
            throw Oops.Oh(ErrorCode.D1508);
        if (!_dataBaseManager.Delete(tenantLink, tableName))
            throw Oops.Oh(ErrorCode.D1500);
    }

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="linkId">连接Id.</param>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("{DBId}/Table")]
    public async Task Create(string linkId, [FromBody] DatabaseTableInfoOutput input)
    {
        var link = await _dbLinkService.GetInfo(linkId);
        var tenantLink = link ?? _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
        if (_dataBaseManager.IsAnyTable(tenantLink, input.tableInfo.newTable))
            throw Oops.Oh(ErrorCode.D1503);
        var tableInfo = input.tableInfo.Adapt<DbTableModel>();
        tableInfo.table = input.tableInfo.newTable;
        var tableFieldList = input.tableFieldList.Adapt<List<DbTableFieldModel>>();
        if (!await _dataBaseManager.Create(tenantLink, tableInfo, tableFieldList))
            throw Oops.Oh(ErrorCode.D1501);
    }

    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="linkId">连接Id.</param>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPut("{linkId}/Table")]
    public async Task Update(string linkId, [FromBody] DatabaseTableUpInput input)
    {
        var link = await _dbLinkService.GetInfo(linkId);
        var tenantLink = link ?? _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
        var oldFieldList = _dataBaseManager.GetFieldList(tenantLink, input.tableInfo.table).Adapt<List<TableFieldOutput>>();
        oldFieldList = _dataBaseManager.ViewDataTypeConversion(oldFieldList, _dataBaseManager.ToDbType(tenantLink.DbType));
        var oldTableInfo = _dataBaseManager.GetTableInfos(tenantLink).Find(m => m.Name == input.tableInfo.table).Adapt<DbTableModel>();
        try
        {
            var data = _dataBaseManager.GetData(tenantLink, input.tableInfo.table);
            if (data.Rows.Count > 0)
                throw Oops.Oh(ErrorCode.D1508);
            var tableInfo = input.tableInfo.Adapt<DbTableModel>();
            tableInfo.table = input.tableInfo.newTable;
            if (IsSysTable(tableInfo.table))
                throw Oops.Oh(ErrorCode.D1504);
            var tableFieldList = input.tableFieldList.Adapt<List<DbTableFieldModel>>();
            if (!input.tableInfo.table.Equals(input.tableInfo.newTable) && _dataBaseManager.IsAnyTable(tenantLink, input.tableInfo.newTable))
                throw Oops.Oh(ErrorCode.D1503);
            if (!await _dataBaseManager.Update(tenantLink, input.tableInfo.table, tableInfo, tableFieldList))
                throw Oops.Oh(ErrorCode.D1502);
        }
        catch (Exception ex)
        {
            await _dataBaseManager.Create(tenantLink, oldTableInfo, oldFieldList.Adapt<List<DbTableFieldModel>>());
            throw Oops.Oh(ErrorCode.D1502);
        }
    }

    /// <summary>
    /// 导入.
    /// </summary>
    /// <param name="linkid"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("{linkid}/Action/Import")]
    public async Task ActionsImport(string linkid, IFormFile file)
    {
        var fileType = Path.GetExtension(file.FileName).Replace(".", string.Empty);
        if (!fileType.ToLower().Equals(ExportFileType.bdb.ToString()))
            throw Oops.Oh(ErrorCode.D3006);
        var josn = _fileManager.Import(file);
        var data = josn.ToObject<DatabaseTableInfoOutput>();
        if (data == null || data.tableFieldList == null || data.tableInfo == null)
            throw Oops.Oh(ErrorCode.D3006);
        data.tableInfo.newTable = data.tableInfo.table;
        await Create(linkid, data);
    }

    #endregion

    #region PrivateMethod

    /// <summary>
    /// 是否系统表.
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    private bool IsSysTable(string table)
    {
        string[] byoTable =
        {
            "base_appdata",
            "base_authorize",
            "base_billrule",
            "base_columnspurview",
            "base_comfields",
            "base_datainterface",
            "base_datainterfacelog",
            "base_dbbackup",
            "base_dblink",
            "base_dictionarydata",
            "base_dictionarytype",
            "base_group",
            "base_imcontent",
            "base_imreply",
            "base_message",
            "base_message_template",
            "base_messagereceive",
            "base_module",
            "base_modulebutton",
            "base_modulecolumn",
            "base_moduledataauthorize",
            "base_moduledataauthorizescheme",
            "base_moduleform",
            "base_organize",
            "base_organize_relation",
            "base_organizeadministrator",
            "base_portal",
            "base_position",
            "base_printdev",
            "base_province",
            "base_role",
            "base_sms_template",
            "base_synthirdinfo",
            "base_sysconfig",
            "base_syslog",
            "base_timetask",
            "base_timetasklog",
            "base_user",
            "base_userrelation",
            "base_visualdev",
            "base_visualdev_modeldata",
            "blade_visual",
            "blade_visual_category",
            "blade_visual_config",
            "blade_visual_db",
            "blade_visual_map",
            "crm_busines",
            "crm_businesproduct",
            "crm_clue",
            "crm_contract",
            "crm_contractinvoice",
            "crm_contractmoney",
            "crm_contractproduct",
            "crm_customer",
            "crm_customercontacts",
            "crm_followlog",
            "crm_invoice",
            "crm_product",
            "crm_receivable",
            "ext_bigdata",
            "ext_document",
            "ext_documentshare",
            "ext_emailconfig",
            "ext_emailreceive",
            "ext_emailsend",
            "ext_employee",
            "ext_order",
            "ext_orderentry",
            "ext_orderreceivable",
            "ext_projectgantt",
            "ext_schedule",
            "ext_tableexample",
            "ext_worklog",
            "ext_worklogshare",
            "flow_delegate",
            "flow_engine",
            "flow_engineform",
            "flow_enginevisible",
            "flow_task",
            "flow_taskcirculate",
            "flow_tasknode",
            "flow_taskoperator",
            "flow_taskoperatorrecord",
            "wechat_mpeventcontent",
            "wechat_mpmaterial",
            "wechat_mpmessage",
            "wechat_qydepartment",
            "wechat_qymessage",
            "wechat_qyuser",
            "wform_applybanquet",
            "wform_applydelivergoods",
            "wform_applydelivergoodsentry",
            "wform_applymeeting",
            "wform_archivalborrow",
            "wform_articleswarehous",
            "wform_batchpack",
            "wform_batchtable",
            "wform_conbilling",
            "wform_contractapproval",
            "wform_contractapprovalsheet",
            "wform_debitbill",
            "wform_documentapproval",
            "wform_documentsigning",
            "wform_expenseexpenditure",
            "wform_finishedproduct",
            "wform_finishedproductentry",
            "wform_incomerecognition",
            "wform_leaveapply",
            "wform_letterservice",
            "wform_materialrequisition",
            "wform_materialrequisitionentry",
            "wform_monthlyreport",
            "wform_officesupplies",
            "wform_outboundorder",
            "wform_outboundorderentry",
            "wform_outgoingapply",
            "wform_paydistribution",
            "wform_paymentapply",
            "wform_postbatchtab",
            "wform_procurementmaterial",
            "wform_procurementmaterialentry",
            "wform_purchaselist",
            "wform_purchaselistentry",
            "wform_quotationapproval",
            "wform_receiptprocessing",
            "wform_receiptsign",
            "wform_rewardpunishment",
            "wform_salesorder",
            "wform_salesorderentry",
            "wform_salessupport",
            "wform_staffovertime",
            "wform_supplementcard",
            "wform_travelapply",
            "wform_travelreimbursement",
            "wform_vehicleapply",
            "wform_violationhandling",
            "wform_warehousereceipt",
            "wform_warehousereceiptentry",
            "wform_workcontactsheet"
        };
        return ((IList)byoTable).Contains(table.ToLower());
    }

    /// <summary>
    /// 获取表条数.
    /// </summary>
    /// <param name="tableList"></param>
    private void GetTableCount(List<DatabaseTableListOutput> tableList)
    {
        foreach (var item in tableList)
        {
            try
            {
                item.sum = _repository.Context.Queryable<dynamic>().AS(item.table).Count();
            }
            catch (Exception ex)
            {
                item.sum = 0;
            }
        }
    }

    #endregion
}