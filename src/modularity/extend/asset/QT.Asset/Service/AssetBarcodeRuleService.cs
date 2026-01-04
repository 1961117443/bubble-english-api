using COSXML.Model.Tag;
using Microsoft.AspNetCore.Mvc;
using Minio.DataModel;
using NPOI.SS.Formula.Functions;
using QT.Asset.Dto.AssetBarcode;
using QT.Asset.Entity;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using Spire.Barcode;
using SqlSugar;

namespace QT.Asset;

/// <summary>
/// 条码规则服务
/// </summary>
[ApiDescriptionSettings("资产管理", Tag = "条码规则", Name = "AssetBarcodeRule", Order = 605)]
[Route("api/extend/asset/[controller]")]
public class AssetBarcodeRuleService : QTBaseService<AssetBarcodeRuleEntity, AssetBarcodeRuleCrInput, AssetBarcodeRuleUpInput, AssetBarcodeRuleInfoOutput, AssetBarcodeRuleListPageInput, AssetBarcodeRuleListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<AssetBarcodeRuleEntity> _repository;

    public AssetBarcodeRuleService(
        ISqlSugarRepository<AssetBarcodeRuleEntity> repository,
        ISqlSugarClient context,
        IUserManager userManager)
        : base(repository, context, userManager)
    {
        _repository = repository;
    }

    public override async Task<PageResult<AssetBarcodeRuleListOutput>> GetList([FromQuery] AssetBarcodeRuleListPageInput input)
    {
        var data = await _repository.Context.Queryable<AssetBarcodeRuleEntity>()
            .Select<AssetBarcodeRuleListOutput>(it => new AssetBarcodeRuleListOutput
            {
                // 自定义输出字段
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<AssetBarcodeRuleListOutput>.SqlSugarPagedList(data);
    }


    /// <summary>
    /// 根据指定规则生成指定数量条码
    /// </summary>
    [HttpPost("Actions/{id}/GenerateBarcodes/{count}")]
    public async Task GenerateBarcodesAsync(string id, int count)
    {
        if (count <= 0)
        {
            throw Oops.Oh("请输入生成数量！");
        }

        var rule = await _repository.SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);
        if (rule == null)
            throw new Exception("条码规则不存在");

        var startSerial = rule.LastSerialNumber;
        //var endSerial = startSerial + count - 1;

        var newBarcodes = new List<AssetBarcodeEntity>();
        int i = 0;
        while (count > 0)
        {
            count--;
            i++;

            int serial = startSerial + i;

            var dateStr = "";
            if (rule.DateFormat!="no")
            {
                dateStr = DateTime.Now.ToString(rule.DateFormat);
            }
            string number = $"{rule.Prefix}{dateStr}{serial.ToString().PadLeft(rule.ZeroPadding, '0')}";

            // 可选：生成二维码图片（Base64 或文件 URL）
            //string qrUrl = _barcodeGenerator.GenerateQrCodeUrl(number, rule.StyleJson);


            var barcodeEntity = new AssetBarcodeEntity
            {
                RuleId = rule.Id,
                BarcodeNumber = number,
                BarcodeImageUrl = "",
                Status = AssetBarcodeStatus.Unbound
            };
            barcodeEntity.Create();
            newBarcodes.Add(barcodeEntity);

            rule.LastSerialNumber = serial;
            int limit = 1000;
            if (newBarcodes.Count == limit)
            {
                // 1000条执行一次
                int result = await SaveBarcode(rule, newBarcodes);

                newBarcodes.Clear();

                var diff = limit - result;

                if (diff>0)
                {
                    count = count + diff;
                }
            }

        }
        if (newBarcodes.IsAny())
        {
            await SaveBarcode(rule, newBarcodes);
        }
         
    }

    private async Task<int> SaveBarcode(AssetBarcodeRuleEntity rule, List<AssetBarcodeEntity> list)
    {
        int result = 0;
        // 1、排除重复的条码
        // 2、插入条码
        // 3、更新流水号
        if (list.IsAny())
        {
            using (var uow = _repository.Context.CreateContext())
            {
                var newBarcodeNums = list.Select(x => x.BarcodeNumber).ToList();

                var existsNum = await _repository.Context.Queryable<AssetBarcodeEntity>().Where(it => newBarcodeNums.Contains(it.BarcodeNumber)).Select(it => it.BarcodeNumber).ToListAsync();

                if (existsNum.Count > 0)
                {
                    list = list.Where(it => !existsNum.Any(z => z == it.BarcodeNumber)).ToList();
                }

                result = list.Any() ? await _repository.Context.Insertable<AssetBarcodeEntity>(list).ExecuteCommandAsync() : 0;

                // 更新流水号
                await _repository.Context.Updateable<AssetBarcodeRuleEntity>(rule).UpdateColumns(x => x.LastSerialNumber).ExecuteCommandAsync();

                uow.Commit();
            }
        }


        return result;
    }
}
