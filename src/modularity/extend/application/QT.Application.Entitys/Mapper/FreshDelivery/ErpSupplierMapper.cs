using Mapster;
using QT.Application.Entitys.Dto.FreshDelivery.ErpBuyorder;
using QT.Application.Entitys.Dto.FreshDelivery.ErpSupplier;
using QT.Application.Entitys.FreshDelivery;
using QT.Common.Models;
using QT.Common.Security;

namespace QT.Application.Entitys.Mapper.FreshDelivery;

public class Mapper : IRegister
{
	public void Register(TypeAdapterConfig config)
	{
		config.ForType<ErpSupplierCrInput, ErpSupplierEntity>()
			.Map(dest => dest.Logo, src => src.logo.ToJsonString())
            .Map(dest => dest.BusinessLicense, src => src.businessLicense.ToJsonString())
            .Map(dest => dest.ProductionLicense, src => src.productionLicense.ToJsonString())
            .Map(dest => dest.FoodBusinessLicense, src => src.foodBusinessLicense.ToJsonString())
        ;
		config.ForType<ErpSupplierEntity, ErpSupplierInfoOutput>()
			.Map(dest => dest.logo, src => string.IsNullOrEmpty(src.Logo) ? new List<FileControlsModel>() : src.Logo.ToObject<List<FileControlsModel>>())
            .Map(dest => dest.businessLicense, src => string.IsNullOrEmpty(src.BusinessLicense) ? new List<FileControlsModel>() : src.BusinessLicense.ToObject<List<FileControlsModel>>())
            .Map(dest => dest.foodBusinessLicense, src => string.IsNullOrEmpty(src.FoodBusinessLicense) ? new List<FileControlsModel>() : src.FoodBusinessLicense.ToObject<List<FileControlsModel>>())
            .Map(dest => dest.productionLicense, src => string.IsNullOrEmpty(src.ProductionLicense) ? new List<FileControlsModel>() : src.ProductionLicense.ToObject<List<FileControlsModel>>())
        ;

        config.ForType<ErpBuyorderdetailDoneInput, ErpBuyorderdetailEntity>()
            .Map(dest => dest.Proof, src => src.proof.ToJsonString())
        ;

        config.ForType<ErpBuyorderdetailEntity, ErpBuyorderdetailDoneInput>()
           .Map(dest => dest.proof, src => string.IsNullOrEmpty(src.Proof) ? new List<FileControlsModel>():  src.Proof.ToObject<List<FileControlsModel>>())
       ;

        config.ForType<ErpBuyorderdetailOutput, ErpBuyorderdetailEntity>()
    .Map(dest => dest.Proof, src => src.proof.ToJsonString())
;

        config.ForType<ErpBuyorderdetailEntity, ErpBuyorderdetailOutput>()
           .Map(dest => dest.proof, src => string.IsNullOrEmpty(src.Proof) ? new List<FileControlsModel>() : src.Proof.ToObject<List<FileControlsModel>>())
       ;

    }
}
