using Mapster;
using QT.Application.Entitys.Dto.FreshDelivery.ErpBuyorder;
using QT.Application.Entitys.Dto.FreshDelivery.ErpCustomer;
using QT.Application.Entitys.Dto.FreshDelivery.ErpInrecord;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOrder;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderCg;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderCw;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderPs;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOutrecord;
using QT.Application.Entitys.Dto.FreshDelivery.ErpProduct;
using QT.Application.Entitys.Dto.FreshDelivery.ErpStoreroom;
using QT.Application.Entitys.Dto.FreshDelivery.ErpSupplier;
using QT.Application.Entitys.FreshDelivery;
using QT.Common.Extension;
using QT.Common.Models;
using QT.Common.Security;

namespace QT.Extend.Entitys.Mapper
{
    class Mapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            //config.ForType<EmailReceiveEntity, EmailHomeOutput>()
            //    .Map(dest => dest.fullName, src => src.Subject);
            //config.ForType<EmailReceiveEntity, EmailListOutput>()
            //    .Map(dest => dest.fdate, src => src.Date)
            //    .Map(dest => dest.sender, src => src.SenderName)
            //    .Map(dest => dest.isRead, src => src.Read);
            //config.ForType<EmailSendEntity, EmailListOutput>()
            //    .Map(dest => dest.recipient, src => src.To);
            //config.ForType<EmailConfigEntity, EmailConfigInfoOutput>()
            //    .Map(dest => dest.emailSsl, src => src.Ssl);
            //config.ForType<EmailReceiveEntity, EmailInfoOutput>()
            //    .Map(dest => dest.recipient, src => src.MAccount)
            //    .Map(dest => dest.fdate, src => src.Date);
            //config.ForType<EmailSendEntity, EmailInfoOutput>()
            //    .Map(dest => dest.recipient, src => src.To);
            //config.ForType<OrderReceivableEntity, OrderCollectionPlanOutput>()
            //    .Map(dest => dest.fabstract, src => src.Abstract)
            //    .Map(dest => dest.receivableMoney, src => src.ReceivableMoney.ToString());
            //config.ForType<EmailConfigUpInput, EmailConfigEntity>()
            //   .Map(dest => dest.Ssl, src => src.emailSsl);
            //config.ForType<EmailConfigActionsCheckMailInput, EmailConfigEntity>()
            //   .Map(dest => dest.Ssl, src => src.emailSsl);

            //config.ForType<EmailConfigEntity, MailAccount>()
            //   .Map(dest => dest.Ssl, src => src.Ssl != null && src.Ssl == 1 ? true : false);

            config.ForType<ErpCustomerEntity, ErpCustomerInfoOutput>()
                .Map(dest => dest.license, src => string.IsNullOrEmpty(src.License) ? new List<FileControlsModel>() : src.License.ToObject<List<FileControlsModel>>());

            config.ForType<ErpCustomerInfoOutput, ErpCustomerEntity>()
                .Map(dest => dest.License, src => src.license.Any() ? src.license.ToJsonString() : string.Empty);

            config.ForType<ErpCustomerCrInput, ErpCustomerEntity>()
                .Map(dest => dest.License, src => src.license.Any() ? src.license.ToJsonString() : string.Empty);


            config.ForType<ErpStoreroomListOutput, ErpStoreroomListTreeOutput>()
                .Map(dest => dest.id, src => src.id)
                .Map(dest => dest.parentId, src => src.fid);


            config.ForType<ErpOrderCrInput, ErpOrderEntity>()
                .Map(dest => dest.DeliveryProof, src => src.deliveryProof != null && src.deliveryProof.Any() ? src.deliveryProof.ToJsonString() : string.Empty)
                .Map(dest => dest.DeliveryToProof, src => src.deliveryToProof != null && src.deliveryToProof.Any() ? src.deliveryToProof.ToJsonString() : string.Empty);

            config.ForType<ErpOrderEntity, ErpOrderPsInfoOutput>()
                .Map(dest => dest.deliveryToProof, src => string.IsNullOrEmpty(src.DeliveryToProof) ? new List<FileControlsModel>() : src.DeliveryToProof.ToObject<List<FileControlsModel>>())
                .Map(dest => dest.deliveryProof, src => string.IsNullOrEmpty(src.DeliveryProof) ? new List<FileControlsModel>() : src.DeliveryProof.ToObject<List<FileControlsModel>>());


            config.ForType<ErpOrderEntity, ErpOrderInfoOutput>()
                .Map(dest => dest.deliveryToProof, src => string.IsNullOrEmpty(src.DeliveryToProof) ? new List<FileControlsModel>() : src.DeliveryToProof.ToObject<List<FileControlsModel>>())
                .Map(dest => dest.deliveryProof, src => string.IsNullOrEmpty(src.DeliveryProof) ? new List<FileControlsModel>() : src.DeliveryProof.ToObject<List<FileControlsModel>>());


            config.ForType<ErpInrecordCrInput, ErpOutrecordCrInput>()
                .Map(dest => dest.gid, src => src.gid)
                .Map(dest => dest.num, src => src.inNum)
                .Map(dest => dest.price, src => src.price)
                .Map(dest => dest.amount, src => src.amount)
                .Map(dest => dest.remark, src => src.remark)
                .Map(dest => dest.storeDetailList, src => src.storeDetailList);


            config.ForType<ErpProductListImportDataInput, ErpProductEntity>()
                .Map(dest => dest.Name, src => src.name)
                .Map(dest => dest.Name, src => src.name)
                .Map(dest => dest.Nickname, src => src.nickname)
                .Map(dest => dest.No, src => src.no)
                .Map(dest => dest.Saletype, src => src.saletype)
                .Map(dest => dest.Producer, src => src.producer)
                .Map(dest => dest.Sort, src => src.sort)
                .Map(dest => dest.Storage, src => src.storage)
                .Map(dest => dest.Retention, src => src.retention)
                .Map(dest => dest.State, src => src.state == "启用" ? "1" : "0");

            config.ForType<ErpProductListImportDataInput, ErpProductmodelEntity>()
                .Map(dest => dest.Name, src => src.model_name)
                .Map(dest => dest.Ratio, src => src.model_ratio)
                .Map(dest => dest.CostPrice, src => src.model_costPrice)
                .Map(dest => dest.SalePrice, src => src.model_salePrice)
                .Map(dest => dest.MinNum, src => src.model_minNum)
                .Map(dest => dest.MaxNum, src => src.model_maxNum)
                .Map(dest => dest.GrossMargin, src => src.model_grossMargin)
                .Map(dest => dest.Package, src => src.model_package)
                .Map(dest => dest.BarCode, src => src.model_barCode);

            config.ForType<ErpSupplierListImportDataInput, ErpSupplierEntity>()
                .Map(dest => dest.JoinTime, src => src.joinTime.ParseToDateTime(null));


            config.ForType<ErpOrderEntity, ErpOrderCwInfoOutput>()
         .Map(dest => dest.deliveryToProof, src => string.IsNullOrEmpty(src.DeliveryToProof) ? new List<FileControlsModel>() : src.DeliveryToProof.ToObject<List<FileControlsModel>>())
         .Map(dest => dest.deliveryProof, src => string.IsNullOrEmpty(src.DeliveryProof) ? new List<FileControlsModel>() : src.DeliveryProof.ToObject<List<FileControlsModel>>());


            config.ForType<ErpBuyorderEntity, ErpBuyorderInfoOutput>()
                .Map(dest=>dest.posttime,src=> string.IsNullOrEmpty(src.Posttime) ? new string[0] : src.Posttime.Split(",",true))
                .Map(dest => dest.taskToUserId, src => string.IsNullOrEmpty(src.TaskToUserId) ? new string[0] : new string[] { src.TaskToUserId })
                .Map(dest => dest.proof, src => string.IsNullOrEmpty(src.Proof) ? new List<FileControlsModel>() : src.Proof.ToObject<List<FileControlsModel>>());


            config.ForType<ErpBuyorderCrInput, ErpBuyorderEntity>()
                .Map(dest=>dest.Posttime,src=> src.posttime.IsAny()? string.Join(",",src.posttime):"")
                .Map(dest => dest.TaskToUserId, src => src.taskToUserId.IsAny() ? src.taskToUserId[0] : "");

            config.ForType<ErpCustomerEntity, ErpCustomerInfoOutput>()
                .Map(dest => dest.diningType, src => !string.IsNullOrEmpty(src.DiningType) ? src.DiningType.Split(",", true) : new string[0]);

            config.ForType<ErpCustomerCrInput, ErpCustomerEntity>()
                .Map(dest => dest.DiningType, src => src.diningType.IsAny() ? string.Join(",", src.diningType) : string.Empty);


            config.ForType<ErpCustomerEntity, ErpCustomerListOutput>()
                .Map(dest => dest.diningType, src => !string.IsNullOrEmpty(src.DiningType) ? src.DiningType.Split(",", true) : new string[0]);


            config.ForType<ErpBuyorderCkUpProofInput, ErpBuyorderEntity>()
    .Map(dest => dest.QualityReportProof, src => src.qualityReportProof.ToJsonString())
    .Map(dest => dest.SelfReportProof, src => src.selfReportProof.ToJsonString())
;

            config.ForType<ErpBuyorderEntity, ErpBuyorderCkUpProofInput>()
               .Map(dest => dest.qualityReportProof, src => string.IsNullOrEmpty(src.QualityReportProof) ? new List<FileControlsModel>() : src.QualityReportProof.ToObject<List<FileControlsModel>>())
               .Map(dest => dest.selfReportProof, src => string.IsNullOrEmpty(src.SelfReportProof) ? new List<FileControlsModel>() : src.SelfReportProof.ToObject<List<FileControlsModel>>());

            config.ForType<ErpBuyorderCkUpProofInput, ErpInrecordEntity>()
   .Map(dest => dest.QualityReportProof, src => src.qualityReportProof.ToJsonString())
   //.Map(dest => dest.SelfReportProof, src => src.selfReportProof.ToJsonString())
;

            config.ForType<ErpInrecordEntity, ErpBuyorderCkUpProofInput>()
               .Map(dest => dest.qualityReportProof, src => string.IsNullOrEmpty(src.QualityReportProof) ? new List<FileControlsModel>() : src.QualityReportProof.ToObject<List<FileControlsModel>>());
               //.Map(dest => dest.selfReportProof, src => string.IsNullOrEmpty(src.SelfReportProof) ? new List<FileControlsModel>() : src.SelfReportProof.ToObject<List<FileControlsModel>>());


            config.ForType<ErpOrderCgListExportOutput, ErpOrderCgListHisExportOutput>()
                .Map(dest => dest.state, src => src.state.ToDescription())
                ;

            config.ForType<ErpBuyorderCkUpProofInput, ErpBuyorderdetailEntity>()
  .Map(dest => dest.QualityReportProof, src => src.qualityReportProof.ToJsonString())
;

            config.ForType<ErpBuyorderdetailEntity, ErpBuyorderCkUpProofInput>()
               .Map(dest => dest.qualityReportProof, src => string.IsNullOrEmpty(src.QualityReportProof) ? new List<FileControlsModel>() : src.QualityReportProof.ToObject<List<FileControlsModel>>())
              ;

            config.ForType<ErpBuyorderCkUpProofInput, ErpProductcustomertypepriceEntity>()
 .Map(dest => dest.QualityReportProof, src => src.qualityReportProof.ToJsonString())
;

            config.ForType<ErpProductcustomertypepriceEntity, ErpBuyorderCkUpProofInput>()
               .Map(dest => dest.qualityReportProof, src => string.IsNullOrEmpty(src.QualityReportProof) ? new List<FileControlsModel>() : src.QualityReportProof.ToObject<List<FileControlsModel>>())
              ;

            config.ForType<ErpBuyorderCkUpProofInput, ErpProductpriceEntity>()
.Map(dest => dest.QualityReportProof, src => src.qualityReportProof.ToJsonString())
;

            config.ForType<ErpProductpriceEntity, ErpBuyorderCkUpProofInput>()
               .Map(dest => dest.qualityReportProof, src => string.IsNullOrEmpty(src.QualityReportProof) ? new List<FileControlsModel>() : src.QualityReportProof.ToObject<List<FileControlsModel>>())
              ;

            config.ForType<ErpBuyorderCkUpProofInput, ErpOrderdetailEntity>()
.Map(dest => dest.QualityReportProof, src => src.qualityReportProof.ToJsonString())
;

            config.ForType<ErpOrderdetailEntity, ErpBuyorderCkUpProofInput>()
               .Map(dest => dest.qualityReportProof, src => string.IsNullOrEmpty(src.QualityReportProof) ? new List<FileControlsModel>() : src.QualityReportProof.ToObject<List<FileControlsModel>>())
              ;

            config.ForType<ErpOrderCgListExportOutput, ErpOrderCgListExportData>()
                .Map(dest => dest.posttime, src => src.posttime.HasValue ? src.posttime.Value.ToString("yyyy-MM-dd") : "")
                ;
        }
    }
}
