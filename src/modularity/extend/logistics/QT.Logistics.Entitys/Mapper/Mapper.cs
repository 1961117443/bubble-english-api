using Mapster;
using QT.Common.Extension;
using QT.Common.Models;
using QT.Common.Security;
using QT.Logistics.Entitys.Dto.LogEnterprise;
using QT.Logistics.Entitys.Dto.LogEnterpriseAttachment;
using QT.Logistics.Entitys.Dto.LogEnterpriseProduct;
using QT.Logistics.Entitys.Dto.LogEnterpriseProducttype;
using QT.Logistics.Entitys.Dto.LogEnterpriseStoreroom;
using QT.Logistics.Entitys.Dto.LogEnterpriseSupplyProduct;
using QT.Logistics.Entitys.Dto.LogMember;
using QT.Logistics.Entitys.Dto.LogOrderAttachment;
using QT.Logistics.Entitys.Dto.LogOrderClasses;
using QT.Logistics.Entitys.Dto.LogOrderDelivery;
using QT.Logistics.Entitys.Dto.LogProduct;
using QT.Logistics.Entitys.Dto.LogProducttype;
using QT.Logistics.Entitys.Dto.LogStoreroom;
using QT.Logistics.Entitys.Dto.LogVehicle;

namespace QT.Logistics.Entitys;

public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<LogStoreroomEntity, LogStoreroomTreeListOutput>()
            .Map(dest => dest.parentId, src => src.PId)
            .Map(dest => dest.fullName, src => src.Name);

        config.ForType<LogDeliveryStoreroomEntity, LogStoreroomTreeListOutput>()
            .Map(dest => dest.parentId, src => src.PId)
            .Map(dest => dest.fullName, src => src.Name);

        config.ForType<LogMemberCrInput, LogMemberEntity>()
            .Map(dest => dest.Avatar, src => src.avatar.ToJsonString());

        config.ForType<LogMemberEntity, LogMemberInfoOutput>()
            .Map(dest => dest.avatar, src => string.IsNullOrEmpty(src.Avatar) ? new List<FileControlsModel>() : src.Avatar.ToObject<List<FileControlsModel>>());

        config.ForType<LogOrderAttachmentCrInput, LogOrderAttachmentEntity>()
            .Map(dest => dest.Id, src => src.id)
            .Map(dest => dest.AttachmentName, src => src.name)
            .Map(dest => dest.AttachmentPath, src => src.url)
            .Map(dest => dest.Remark, src => src.fileId);

        config.ForType<LogOrderAttachmentEntity, LogOrderAttachmentInfoOutput>()
            .Map(dest => dest.id, src => src.Id)
            .Map(dest => dest.name, src => src.AttachmentName)
            .Map(dest => dest.url, src => src.AttachmentPath)
            .Map(dest => dest.fileId, src => src.Remark);


        config.ForType<LogOrderDeliveryEntity, LogOrderDeliveryInfoOutput>()
            .Map(dest => dest.status, src => src.OutboundTime.HasValue ? 2 : src.InboundTime.HasValue ? 1 : 0);

        config.ForType<LogOrderClassesEntity, LogOrderClassesInfoOutput>()
            .Map(dest => dest.status, src => src.OutboundTime.HasValue ? 2 : src.InboundTime.HasValue ? 1 : 0);


        config.ForType<LogEnterpriseAttachmentInfoOutput, LogEnterpriseAttachmentEntity>()
            .Map(dest => dest.Id, src => src.id)
            .Map(dest => dest.FileSize, src => src.size)
            .Map(dest => dest.UploadTime, src => src.uploadTime)
            .Map(dest => dest.AttachmentName, src => src.name)
            .Map(dest => dest.AttachmentPath, src => src.url);

        config.ForType<LogEnterpriseAttachmentEntity, LogEnterpriseAttachmentInfoOutput>()
            .Map(dest => dest.id, src => src.Id)
            .Map(dest => dest.size, src => src.FileSize)
            .Map(dest => dest.uploadTime, src => src.UploadTime)
            .Map(dest => dest.name, src => src.AttachmentName)
            .Map(dest => dest.url, src => src.AttachmentPath);

        config.ForType<LogEnterpriseProducttypeEntity, LogEnterpriseProducttypeTreeListOutput>()
            .Map(dest => dest.parentId, src => src.Fid)
            .Map(dest => dest.fullName, src => src.Name);


        config.ForType<LogProducttypeEntity, LogProducttypeTreeListOutput>()
            .Map(dest => dest.parentId, src => src.Fid)
            .Map(dest => dest.fullName, src => src.Name);


        config.ForType<LogEnterpriseStoreroomEntity, LogEnterpriseStoreroomTreeListOutput>()
            .Map(dest => dest.parentId, src => src.PId)
            .Map(dest => dest.fullName, src => src.Name);

        config.ForType<LogEnterpriseProductCrInput, LogEnterpriseProductEntity>()
           .Map(dest => dest.ImageUrl, src => src.imageUrl.IsAny() ? src.imageUrl.ToJsonString() : "");

        config.ForType<LogEnterpriseProductEntity, LogEnterpriseProductInfoOutput>()
           .Map(dest => dest.imageUrl, src => !string.IsNullOrEmpty(src.ImageUrl) ? src.ImageUrl.ToObject<List<FileControlsModel>>() : new List<FileControlsModel>());


        config.ForType<LogEnterpriseSupplyProductCrInput, LogEnterpriseSupplyProductEntity>()
          .Map(dest => dest.ImageUrl, src => src.imageUrl.IsAny() ? src.imageUrl.ToJsonString() : "");

        config.ForType<LogEnterpriseSupplyProductEntity, LogEnterpriseSupplyProductInfoOutput>()
          .Map(dest => dest.imageUrl, src => !string.IsNullOrEmpty(src.ImageUrl) ? src.ImageUrl.ToObject<List<FileControlsModel>>() : new List<FileControlsModel>());

        

        config.ForType<LogProductCrInput, LogProductEntity>()
         .Map(dest => dest.ImageUrl, src => src.imageUrl.IsAny() ? src.imageUrl.ToJsonString() : "");

        config.ForType<LogProductEntity, LogProductInfoOutput>()
           .Map(dest => dest.imageUrl, src => !string.IsNullOrEmpty(src.ImageUrl) ? src.ImageUrl.ToObject<List<FileControlsModel>>() : new List<FileControlsModel>());


        config.ForType<LogVehicleCrInput, LogVehicleEntity>()
         .Map(dest => dest.ImageUrl, src => src.imageUrl.IsAny() ? src.imageUrl.ToJsonString() : "");

        config.ForType<LogVehicleEntity, LogVehicleInfoOutput>()
           .Map(dest => dest.imageUrl, src => !string.IsNullOrEmpty(src.ImageUrl) ? src.ImageUrl.ToObject<List<FileControlsModel>>() : new List<FileControlsModel>());

        config.ForType<LogVehicleAttachmentInfoOutput, LogVehicleAttachmentEntity>()
            .Map(dest => dest.Id, src => src.id)
            .Map(dest => dest.AttachmentName, src => src.name)
            .Map(dest => dest.AttachmentPath, src => src.url)
            .Map(dest => dest.FileSize, src => src.size);

        config.ForType<LogVehicleAttachmentEntity, LogVehicleAttachmentInfoOutput>()
            .Map(dest => dest.id, src => src.Id)
            .Map(dest => dest.name, src => src.AttachmentName)
            .Map(dest => dest.url, src => src.AttachmentPath)
            .Map(dest => dest.size, src => src.FileSize);

        config.ForType<LogEnterpriseCrInput, LogEnterpriseEntity>()
         .Map(dest => dest.ImageUrl, src => src.imageUrl.IsAny() ? src.imageUrl.ToJsonString() : "");

        config.ForType<LogEnterpriseEntity, LogEnterpriseInfoOutput>()
           .Map(dest => dest.imageUrl, src => !string.IsNullOrEmpty(src.ImageUrl) ? src.ImageUrl.ToObject<List<FileControlsModel>>() : new List<FileControlsModel>());
    }
}
