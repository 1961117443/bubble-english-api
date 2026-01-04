using Microsoft.AspNetCore.Http;
using QT.Application.Entitys.Dto.FreshDelivery.ErpCustomer;
using QT.Application.Entitys.Dto.FreshDelivery.ErpProductprice;
using QT.Application.Entitys.FreshDelivery;
using QT.ClayObject;
using QT.Common.Configuration;
using QT.Common.Core;
using QT.Common.Core.Manager.Files;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Helper;
using QT.Common.Models.NPOI;
using QT.Common.Security;
using QT.DataEncryption;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Extras.DatabaseAccessor.SqlSugar;
using QT.Logging.Attributes;
using QT.Systems.Entitys.Dto.User;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.Permission;
using QT.Systems.Interfaces.System;
using System.Data;

namespace QT.Application.FreshDelivery;

/// <summary>
/// 业务实现：客户信息.
/// </summary>
[ApiDescriptionSettings("生鲜配送", Tag = "客户信息", Name = "ErpCustomer", Order = 200)]
[Route("api/apply/FreshDelivery/[controller]")]
public class ErpCustomerService : QTBaseService<ErpCustomerEntity, ErpCustomerCrInput, ErpCustomerUpInput, ErpCustomerInfoOutput, ErpCustomerListQueryInput, ErpCustomerListOutput>, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ErpCustomerEntity> _repository;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly IUsersService _usersService;
    private readonly IFileManager _fileManager;
    private readonly IDictionaryDataService _dictionaryDataService;


    /// <summary>
    /// 初始化一个<see cref="ErpCustomerService"/>类型的新实例.
    /// </summary>
    public ErpCustomerService(
        ISqlSugarRepository<ErpCustomerEntity> erpCustomerRepository,
        ISqlSugarClient context,
        IUserManager userManager,
        IUsersService usersService, IFileManager fileManager, IDictionaryDataService dictionaryDataService) : base(erpCustomerRepository, context, userManager)
    {
        _repository = erpCustomerRepository;
        _db = context.AsTenant();
        _userManager = userManager;
        _usersService = usersService;
        _fileManager = fileManager;
        _dictionaryDataService = dictionaryDataService;
    }

    ///// <summary>
    ///// 获取客户信息.
    ///// </summary>
    ///// <param name="id">主键值.</param>
    ///// <returns></returns>
    //[HttpGet("{id}")]
    //public async Task<dynamic> GetInfo(string id)
    //{
    //    var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<ErpCustomerInfoOutput>();

    //    var erpProductpriceList = await _repository.Context.Queryable<ErpProductpriceEntity>()
    //        .Where(w => w.Cid == output.id)
    //        .LeftJoin<ErpProductmodelEntity>((w, x) => w.Gid == x.Id)
    //        .LeftJoin<ErpProductEntity>((w, x, y) => x.Pid == y.Id)
    //        .Select((w, x, y) => new ErpProductpriceInfoOutput
    //        {
    //            id = w.Id,
    //            gid = w.Gid,
    //            price = w.Price,
    //            time = w.Time,
    //            gidName = x.Name,
    //            productName = y.Name,
    //            salePrice = x.SalePrice,
    //            cid = w.Cid
    //        })
    //        .ToListAsync();
    //    output.erpProductpriceList = erpProductpriceList;

    //    return output;
    //}

    /// <summary>
    /// 获取初始化数据
    /// </summary>
    /// <returns></returns>
    [HttpGet("maxNo")]
    public async Task<string> maxNo()
    {
        string prefix = string.Empty;
        if (!string.IsNullOrEmpty(_userManager.CompanyId))
        {
            var entity = await _repository.Context.Queryable<OrganizeEntity>().InSingleAsync(_userManager.CompanyId);
            prefix = entity?.EnCode ?? "";
        }

        var maxNo = await _repository.AsQueryable().MaxAsync(x => x.No);

        int num = 0;
        int len = 3;
        if (!string.IsNullOrEmpty(maxNo) && maxNo.IsMatchNumber())
        {
            var no = maxNo.MatchLastNumber();
            if (int.TryParse(no, out num))
            {
                len = no.Length;
                prefix = maxNo.Replace(no, "");
            }
            
        }
        num++;
        var newNum = num.ToString().PadLeft(len, '0');

        return $"{prefix}{newNum}";


    }

    ///// <summary>
    ///// 获取客户信息列表.
    ///// </summary>
    ///// <param name="input">请求参数.</param>
    ///// <returns></returns>
    //[HttpGet("")]
    //public async Task<dynamic> GetList([FromQuery] ErpCustomerListQueryInput input)
    //{
    //    var data = await _repository.Context.Queryable<ErpCustomerEntity>()
    //        .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
    //        .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
    //        .WhereIF(!string.IsNullOrEmpty(input.admin), it => it.Admin.Contains(input.admin))
    //        .WhereIF(!string.IsNullOrEmpty(input.admintel), it => it.Admintel.Contains(input.admintel))
    //        .WhereIF(!string.IsNullOrEmpty(input.deliveryManId), it => it.DeliveryManId.Equals(input.deliveryManId))
    //        .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
    //            it.Name.Contains(input.keyword)
    //            || it.No.Contains(input.keyword)
    //            )
    //        .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
    //        .Select(it => new ErpCustomerListOutput
    //        {
    //            id = it.Id,
    //            oid = it.Oid,
    //            name = it.Name,
    //            no = it.No,
    //            address = it.Address,
    //            admin = it.Admin,
    //            admintel = it.Admintel,
    //            description = it.Description,
    //            loginId = it.LoginId,
    //            loginPwd = it.LoginPwd,
    //            stop = it.Stop,
    //            type = it.Type,
    //            oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.Oid).Select(d => d.FullName),
    //            srcDiningType = it.DiningType,
    //            discount = it.Discount,
    //            deliveryManId = it.DeliveryManId
    //        }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);

    //    foreach (var item in data.list)
    //    {
    //        item.diningType = string.IsNullOrEmpty(item.srcDiningType) ? new string[0] : item.srcDiningType.Split(",", true);
    //    }
    //    return PageResult<ErpCustomerListOutput>.SqlSugarPageResult(data);
    //}



    ///// <summary>
    ///// 新建客户信息.
    ///// </summary>
    ///// <param name="input">参数.</param>
    ///// <returns></returns>
    //[HttpPost("")]
    //[SqlSugarUnitOfWork]
    //public async Task Create([FromBody] ErpCustomerCrInput input)
    //{
    //    var entity = input.Adapt<ErpCustomerEntity>();
    //    entity.Id = SnowflakeIdHelper.NextId();

    //    await CheckUnique(entity);

    //    // 先判断是否有“客户”的角色
    //    var role = await _repository.Context.Queryable<RoleEntity>().Where(x => x.EnCode == "KH").FirstAsync();
    //    if (role == null)
    //    {
    //        throw Oops.Oh("“客户”角色不存在，请先创建好角色，客户编号[KH]");
    //    }

    //    //try
    //    //{
    //    //    // 开启事务
    //    //    _db.BeginTran();

    //        var newEntity = await _repository.Context.Insertable<ErpCustomerEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

    //        var erpProductpriceEntityList = input.erpProductpriceList.Adapt<List<ErpProductpriceEntity>>();
    //        if (erpProductpriceEntityList != null)
    //        {
    //            foreach (var item in erpProductpriceEntityList)
    //            {
    //                item.Id = SnowflakeIdHelper.NextId();
    //                item.Cid = newEntity.Id;
    //            }

    //            await _repository.Context.Insertable<ErpProductpriceEntity>(erpProductpriceEntityList).ExecuteCommandAsync();
    //        }

    //        // 创建客户账号
    //        await _usersService.InnerCreate(new UserInCrInput
    //        {
    //            id = newEntity.Id,
    //            account = newEntity.LoginId,
    //            realName = newEntity.Admin,
    //            password = newEntity.LoginPwd,
    //            mobilePhone = newEntity.Admintel,
    //            roleId = role.Id
    //        });

    //    //    // 关闭事务
    //    //    _db.CommitTran();
    //    //}
    //    //catch (Exception)
    //    //{
    //    //    // 回滚事务
    //    //    _db.RollbackTran();

    //    //    throw Oops.Oh(ErrorCode.COM1000);
    //    //}
    //}

    /// <summary>
    /// 获取客户信息无分页列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    private async Task<dynamic> GetNoPagingList([FromQuery] ErpCustomerListQueryInput input)
    {
        return await _repository.Context.Queryable<ErpCustomerEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                || it.No.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new ErpCustomerListOutput
            {
                id = it.Id,
                oid = it.Oid,
                name = it.Name,
                no = it.No,
                address = it.Address,
                admin = it.Admin,
                admintel = it.Admintel,
                description = it.Description,
                loginId = it.LoginId,
                loginPwd = it.LoginPwd,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToListAsync();
    }

    /// <summary>
    /// 导出客户信息.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Actions/Export")]
    public async Task<dynamic> Export([FromQuery] ErpCustomerListQueryInput input)
    {
        var exportData = new List<ErpCustomerListOutput>();
        if (input.dataType == 0)
            exportData = Clay.Object(await GetList(input)).Solidify<PageResult<ErpCustomerListOutput>>().list;
        else
            exportData = await GetNoPagingList(input);
        List<ParamsModel> paramList = "[{\"value\":\"客户名称\",\"field\":\"name\"},{\"value\":\"客户编号\",\"field\":\"no\"},{\"value\":\"客户地址\",\"field\":\"address\"},{\"value\":\"负责人\",\"field\":\"admin\"},{\"value\":\"负责人电话\",\"field\":\"admintel\"},{\"value\":\"登录帐号\",\"field\":\"loginId\"},{\"value\":\"登录密码\",\"field\":\"loginPwd\"},{\"value\":\"客户简介\",\"field\":\"description\"},{\"value\":\"公司ID\",\"field\":\"oid\"},]".ToList<ParamsModel>();
        ExcelConfig excelconfig = new ExcelConfig();
        excelconfig.FileName = "客户信息管理.xls";
        excelconfig.HeadFont = "微软雅黑";
        excelconfig.HeadPoint = 10;
        excelconfig.IsAllSizeColumn = true;
        excelconfig.ColumnModel = new List<ExcelColumnModel>();
        foreach (var item in input.selectKey.Split(',').ToList())
        {
            var isExist = paramList.Find(p => p.field == item);
            if (isExist != null)
                excelconfig.ColumnModel.Add(new ExcelColumnModel() { Column = isExist.field, ExcelColumn = isExist.value });
        }

        var addPath = FileVariable.TemporaryFilePath + excelconfig.FileName;
        ExcelExportHelper<ErpCustomerListOutput>.Export(exportData, excelconfig, addPath);
        var fileName = _userManager.UserId + "|" + addPath + "|xls";
        return new
        {
            name = excelconfig.FileName,
            url = "/api/File/Download?encryption=" + DESCEncryption.Encrypt(fileName, "QT")
        };
    }

    ///// <summary>
    ///// 更新客户信息.
    ///// </summary>
    ///// <param name="id">主键值.</param>
    ///// <param name="input">参数.</param>
    ///// <returns></returns>
    //[HttpPut("{id}")]
    //[SqlSugarUnitOfWork]
    //public async Task Update(string id, [FromBody] ErpCustomerUpInput input)
    //{
    //    var entity = input.Adapt<ErpCustomerEntity>();
    //    await CheckUnique(entity);
    //    //try
    //    //{
    //    //    // 开启事务
    //    //    _db.BeginTran();

    //        await _repository.Context.Updateable<ErpCustomerEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

    //        // 清空商品定价原有数据
    //        await _repository.Context.Deleteable<ErpProductpriceEntity>().Where(it => it.Cid == entity.Id).ExecuteCommandAsync();

    //        // 新增商品定价新数据
    //        var erpProductpriceEntityList = input.erpProductpriceList.Adapt<List<ErpProductpriceEntity>>();
    //        if (erpProductpriceEntityList != null)
    //        {
    //            foreach (var item in erpProductpriceEntityList)
    //            {
    //                item.Id = item.Id ?? SnowflakeIdHelper.NextId();
    //                item.Cid = entity.Id;
    //            }

    //            await _repository.Context.Insertable<ErpProductpriceEntity>(erpProductpriceEntityList).ExecuteCommandAsync();
    //        }

    //        var role = await _repository.Context.Queryable<RoleEntity>().Where(x => x.EnCode == "KH").FirstAsync();
    //        if (role == null)
    //        {
    //            throw Oops.Oh("“客户”角色不存在，请先创建好角色，客户编号[KH]");
    //        }

    //        await _usersService.InnerCreateOrUpdate(new UserInCrInput
    //        {
    //            id = entity.Id,
    //            account = entity.LoginId,
    //            realName = entity.Admin,
    //            password = entity.LoginPwd,
    //            mobilePhone = entity.Admintel,
    //            roleId = role.Id
    //        });

    //    //    // 关闭事务
    //    //    _db.CommitTran();
    //    //}
    //    //catch (Exception)
    //    //{
    //    //    // 回滚事务
    //    //    _db.RollbackTran();
    //    //    throw Oops.Oh(ErrorCode.COM1001);
    //    //}
    //}

    ///// <summary>
    ///// 删除客户信息.
    ///// </summary>
    ///// <returns></returns>
    //[HttpDelete("{id}")]
    //[SqlSugarUnitOfWork]
    //public async Task Delete(string id)
    //{
    //    if (!await _repository.Context.Queryable<ErpCustomerEntity>().AnyAsync(it => it.Id == id))
    //    {
    //        throw Oops.Oh(ErrorCode.COM1005);
    //    }

    //    // 判断订单是否有使用
    //    if (await _repository.Context.Queryable<ErpOrderEntity>().AnyAsync(x=>x.Cid == id))
    //    {
    //        throw Oops.Oh("该客户已下订单，不允许删除！");
    //    }

    //    var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
    //    await _repository.Context.Deleteable<ErpCustomerEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

    //    // 清空商品定价表数据
    //    await _repository.Context.Deleteable<ErpProductpriceEntity>().Where(it => it.Cid.Equals(entity.Id)).ExecuteCommandAsync();

    //    //try
    //    //{
    //    //    // 开启事务
    //    //    _db.BeginTran();

    //    //    var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
    //    //    await _repository.Context.Deleteable<ErpCustomerEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

    //    //    // 清空商品定价表数据
    //    //    await _repository.Context.Deleteable<ErpProductpriceEntity>().Where(it => it.Cid.Equals(entity.Id)).ExecuteCommandAsync();

    //    //    // 关闭事务
    //    //    _db.CommitTran();
    //    //}
    //    //catch (Exception)
    //    //{
    //    //    // 回滚事务
    //    //    _db.RollbackTran();

    //    //    throw Oops.Oh(ErrorCode.COM1002);
    //    //}
    //}

    /// <summary>
    /// 获取下拉框.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<dynamic> GetSelector([FromQuery]bool all)
    {
        var qur = all ? _repository.AsQueryable().ClearFilter<ICompanyEntity>() : _repository.AsQueryable();
        var data = await qur
            .Where(it => it.Stop == 0)
            .OrderBy(o => o.No)
            .OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .Select(it => new ErpCustomerListOutput
            {
                oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(x => x.Id == it.Oid).Select(x => x.FullName),
                srcDiningType = it.DiningType
            }, true)
            .ToListAsync();

        data.ForEach(x => x.diningType = string.IsNullOrEmpty(x.srcDiningType) ? new string[0] : x.srcDiningType.Split(",", true));

        return new { list = data };// data.Adapt<List<ErpCustomerListOutput>>() };
    }
   


    private async Task CheckUnique(ErpCustomerEntity entity)
    {
        if (await _repository.Context.Queryable<ErpCustomerEntity>().AnyAsync(x => x.Name == entity.Name && x.Id != entity.Id))
        {
            throw Oops.Oh("客户名称已存在，不允许重复录入！");
        }

        if (await _repository.Context.Queryable<ErpCustomerEntity>().AnyAsync(x => x.No == entity.No && x.Id != entity.Id))
        {
            throw Oops.Oh("客户编号已存在，不允许重复录入！");
        }

        if (await _repository.Context.Queryable<UserEntity>().AnyAsync(x => x.Account == entity.LoginId && x.DeleteMark == null && x.Id != entity.Id))
        {
            throw Oops.Oh("登录账号已存在，不允许重复录入！");
        }

    }


    #region 导入导出
    /// <summary>
    /// 模板下载.
    /// </summary>
    /// <returns></returns>
    [HttpGet("TemplateDownload")]
    public dynamic TemplateDownload()
    {
        // 初始化 一条空数据 
        List<ErpCustomerListImportDataInput>? dataList = new List<ErpCustomerListImportDataInput>() { new ErpCustomerListImportDataInput() { } };

        ExcelConfig excelconfig = ExcelConfig.Default("客户信息导入模板.xls");
        foreach (KeyValuePair<string, string> item in Common.Extension.Extensions.GetPropertityToMaps<ErpCustomerListImportDataInput>())
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel!.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName!);
        ExcelExportHelper<ErpCustomerListImportDataInput>.Export(dataList, excelconfig, addPath);

        return new { name = excelconfig.FileName, url = "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "QT") };
    }

    /// <summary>
    /// 导入数据.
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    [HttpPost("ImportData")]
    public async Task<dynamic> ImportData(IFormFile file)
    {
        // 先判断是否有“客户”的角色
        var role = await _repository.Context.Queryable<RoleEntity>().Where(x => x.EnCode == "KH").FirstAsync();
        if (role == null)
        {
            throw Oops.Oh("“客户”角色不存在，请先创建好角色，客户编号[KH]");
        }
        var _filePath = _fileManager.GetPathByType(string.Empty);
        var _fileName = DateTime.Now.ToString("yyyyMMdd") + "_" + SnowflakeIdHelper.NextId() + Path.GetExtension(file.FileName);
        var stream = file.OpenReadStream();
        await _fileManager.UploadFileByType(stream, _filePath, _fileName);

        try
        {
            Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpCustomerListImportDataInput>();

            string? filePath = FileVariable.TemporaryFilePath;
            string? savePath = Path.Combine(filePath, _fileName);

            // 得到数据
            var excelData = ExcelImportHelper.ToDataTable(file.OpenReadStream(), ExcelImportHelper.IsXls(_fileName));
            if (excelData != null)
            {
                foreach (DataColumn item in excelData.Columns)
                {
                    var key = FileEncode.Where(x => x.Value == item.ToString()).FirstOrDefault().Key;
                    if (!string.IsNullOrEmpty(key))
                    {
                        item.ColumnName = key;
                        //excelData.Columns[item.ToString()].ColumnName = ;
                    }
                }

            }

            var list = _repository.Context.Utilities.DataTableToList<ErpCustomerListImportDataInput>(excelData);

            //转化为实体 ErpProductEntity\ErpProductmodelEntity 
            var insertErpCustomerEntities = new List<ErpCustomerEntity>();
            var updateErpCustomerEntities = new List<ErpCustomerEntity>();

            //分类
            var typeOptions = await _dictionaryDataService.GetList("QTErpCustomType");
            // 餐别
            var diningTypeOptions = await _dictionaryDataService.GetList("ErpOrderDiningType");
            //数据库中的记录
            var dbErpCustomerEntities = await _repository.Context.Queryable<ErpCustomerEntity>().ToListAsync();

            //Dictionary<string, List<ErpProductmodelEntity>> kv = new Dictionary<string, List<ErpProductmodelEntity>>();
            //Dictionary<string, ErpProductEntity> kvProduct = new Dictionary<string, ErpProductEntity>();

            List<UserInCrInput> userList = new List<UserInCrInput>();

            // 判断手机号码是否重复
            var group = list.GroupBy(x => x.admintel).Where(x => x.Count() > 1);
            if (group.Any())
            {
                throw Oops.Oh($"负责人电话重复，{string.Join(",", group.Select(x => x.Key).ToArray())}");
            }

            foreach (var item in list)
            {
                if (string.IsNullOrEmpty(item.name))
                {
                    throw Oops.Oh("客户名称不能为空！");
                }

                //string.IsNullOrEmpty(item.prefix)

                // 判断是否已存在
                var erpCustomerEntity = dbErpCustomerEntities.Find(x => x.Name == item.name);
                if (erpCustomerEntity != null)
                {
                    _repository.Context.Tracking(erpCustomerEntity);

                    item.Adapt(erpCustomerEntity);

                    updateErpCustomerEntities.Add(erpCustomerEntity);
                }
                else
                {
                    erpCustomerEntity = item.Adapt<ErpCustomerEntity>();
                    erpCustomerEntity.Id = SnowflakeIdHelper.NextId();
                    insertErpCustomerEntities.Add(erpCustomerEntity);
                }
                if (!erpCustomerEntity.Admintel.IsEmpty() && string.IsNullOrEmpty(erpCustomerEntity.LoginId))
                {
                    erpCustomerEntity.LoginId = erpCustomerEntity.Admintel.Trim().Replace(" ", "");
                    if (!string.IsNullOrEmpty(item.prefix))
                    {
                        erpCustomerEntity.LoginId = $"{item.prefix}{erpCustomerEntity.LoginId}";
                    }

                    if (erpCustomerEntity.LoginPwd.IsEmpty())
                    {
                        erpCustomerEntity.LoginPwd = "123456";
                    }

                    userList.Add(new UserInCrInput
                    {
                        id = erpCustomerEntity.Id,
                        account = erpCustomerEntity.LoginId,
                        realName = erpCustomerEntity.Admin,
                        password = erpCustomerEntity.LoginPwd,
                        mobilePhone = erpCustomerEntity.Admintel,
                        roleId = role.Id
                    });
                }

                #region 客户信息 ErpCustomerEntity
                //设置分类
                var typeOpt = typeOptions.Find(x => x.FullName == item.type);
                if (typeOpt != null)
                {
                    erpCustomerEntity.Type = typeOpt.EnCode;
                }

                //设置餐别
                if (!string.IsNullOrEmpty(item.diningType))
                {
                    var arr = item.diningType.Split(",", true);
                    erpCustomerEntity.DiningType = string.Join(",", diningTypeOptions.Where(x => arr.Contains(x.FullName)).Select(x => x.EnCode).ToArray());
                }

                #endregion
            }

            // 开启事务
            _db.BeginTran();

            //更新数据库
            if (insertErpCustomerEntities.Any())
            {
                await _repository.Context.Insertable(insertErpCustomerEntities).ExecuteCommandAsync();
            }
            if (updateErpCustomerEntities.Any())
            {
                await _repository.Context.Updateable(updateErpCustomerEntities).ExecuteCommandAsync();
            }

            // 创建客户账号
            if (userList.Any())
            {
                foreach (var item in userList)
                {
                    await _usersService.InnerCreate(item);
                }
            }
           

            // 关闭事务
            _db.CommitTran();

            return new { np = insertErpCustomerEntities.Count, up = updateErpCustomerEntities.Count };
        }
        catch (Exception e)
        {
            // 回滚事务
            _db.RollbackTran();

            throw Oops.Oh(e.Message);
        }
    }

    /// <summary>
    /// 导出Excel.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("ExportExcel")]
    public async Task<dynamic> ExportExcel([FromQuery] ErpCustomerListQueryInput input)
    {
        List<string> ids = new List<string>();
        if (input.dataType == 0)
        {
            var pageIdList = await _repository.Context.Queryable<ErpCustomerEntity>()
          .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
          .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
          .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
              it.Name.Contains(input.keyword)
              || it.No.Contains(input.keyword)
              )
          .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
          .Select(it => new ErpCustomerListOutput
          {
              id = it.Id,
          }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort)
          .ToPagedListAsync(input.currentPage, input.pageSize);

            ids = pageIdList.list.Select(x => x.id).ToList();
        }

        var sqlQuery = _repository.Context.Queryable<ErpCustomerEntity>()
            .WhereIF(ids != null && ids.Any(), (a) => ids.Contains(a.Id))
            .Select((a) => new ErpCustomerListImportDataInput
            {
                name = a.Name,
                address = a.Address,
                admin = a.Admin,
                admintel = a.Admintel,
                description = a.Description,
                no = a.No,
                type = a.Type,
                diningType = a.DiningType,
                prefix = a.LoginId
            });

        List<ErpCustomerListImportDataInput> list = await sqlQuery.ToListAsync();

        var typeOptions = await _dictionaryDataService.GetList("QTErpCustomType");
        var diningTypeOptions = await _dictionaryDataService.GetList("ErpOrderDiningType");

        list.ForEach(item =>
        {
            item.type = typeOptions.Find(x => x.EnCode == item.type)?.FullName ?? "";
            if (!string.IsNullOrEmpty(item.diningType))
            {
                var arr = item.diningType.Split(",", true);
                item.diningType = string.Join(",", diningTypeOptions.Where(x => arr.Contains(x.EnCode)).Select(x => x.FullName).ToArray());
            }
            var admintel = item.admintel.Trim().Replace(" ", "");
            if (!string.IsNullOrEmpty(item.prefix) && item.prefix.EndsWith(admintel))
            {
                item.prefix = item.prefix.Replace(admintel, "");
            }
            else
            {
                item.prefix = "";
            }
        });

        ExcelConfig excelconfig = ExcelConfig.Default(string.Format("{0:yyyy-MM-dd}_客户信息.xls", DateTime.Now));
        Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpCustomerListImportDataInput>();
        //var selectKey = input.selectKey.Split(',').ToList();
        foreach (KeyValuePair<string, string> item in FileEncode)
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel!.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
        var fs = ExcelExportHelper<ErpCustomerListImportDataInput>.ExportMemoryStream(list, excelconfig);
        var flag = await _fileManager.UploadFileByType(fs, FileVariable.TemporaryFilePath, excelconfig.FileName);
        if (flag.Item1)
        {
            fs.Flush();
            fs.Close();
        }

        return new { name = excelconfig.FileName, url = flag.Item2 ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "QT") };
    }
    #endregion

    #region 客户定价
    /// <summary>
    /// 获取客户定价信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("Productprice/{id}")]
    public async Task<dynamic> GetErpProductpriceInfo(string id)
    {
        return await _repository.Context.Queryable<ErpProductpriceEntity>()
          .LeftJoin<ErpProductmodelEntity>((it, a) => it.Gid == a.Id)
          .LeftJoin<ErpProductEntity>((it, a, b) => a.Pid == b.Id)
          .Where(it => it.Id == id)
          .Select((it, a, b) => new ErpProductpriceInfoOutput
          {
              gidName = a.Name,
              productName = b.Name
          }, true)
          .FirstAsync();
    }

    /// <summary>
    /// 新建商品客户定价.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("Productprice")]
    [OperateLog("用户定价", "新增")]
    public async Task CreateProductprice([FromBody] ErpProductpriceCrInput input)
    {
        if (string.IsNullOrEmpty(input.cid))
        {
            throw Oops.Oh("客户不能为空！");
        }
        if (string.IsNullOrEmpty(input.gid))
        {
            throw Oops.Oh("商品不能为空！");
        }
        var entity = input.Adapt<ErpProductpriceEntity>();

        if (await _repository.Context.Queryable<ErpProductpriceEntity>().AnyAsync(it => it.Cid == entity.Cid && it.Gid == entity.Gid))
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }

        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).EnableDiffLogEvent($"{nameof(ErpProductpriceEntity)}:{entity.Id}").ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新商品客户定价.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("Productprice/{id}")]
    [OperateLog("用户定价", "修改")]
    public async Task Update(string id, [FromBody] ErpProductpriceCrInput input)
    {
        if (string.IsNullOrEmpty(input.cid))
        {
            throw Oops.Oh("客户不能为空！");
        }
        if (string.IsNullOrEmpty(input.gid))
        {
            throw Oops.Oh("商品不能为空！");
        }
        var entity = input.Adapt<ErpProductpriceEntity>();
        if (await _repository.Context.Queryable<ErpProductpriceEntity>().AnyAsync(it => it.Cid == entity.Cid && it.Gid == entity.Gid && it.Id != entity.Id))
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }

        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).EnableDiffLogEvent($"{nameof(ErpProductpriceEntity)}:{entity.Id}").ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除商品客户类型定价.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("Productprice/{id}")]
    [OperateLog("用户定价", "删除")]
    public async Task DeleteProductprice(string id)
    {
        var isOk = await _repository.Context.Deleteable<ErpProductpriceEntity>().Where(it => it.Id.Equals(id)).EnableDiffLogEvent($"{nameof(ErpProductpriceEntity)}:{id}").ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
    #endregion
}