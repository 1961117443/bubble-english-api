using Mapster;
using QT.Common.Extension;
using QT.Common.Models;
using QT.Common.Security;
using QT.JZRC.Entitys;
using QT.JZRC.Entitys.Dto.JzrcCertificateIn;
using QT.JZRC.Entitys.Dto.JzrcCompany;
using QT.JZRC.Entitys.Dto.JzrcCompanyCommunication;
using QT.JZRC.Entitys.Dto.JzrcCompanyJob;
using QT.JZRC.Entitys.Dto.JzrcMember;
using QT.JZRC.Entitys.Dto.JzrcStoreroom;
using QT.JZRC.Entitys.Dto.JzrcTalent;
using QT.JZRC.Entitys.Dto.JzrcTalentCertificate;
using QT.JZRC.Entitys.Dto.JzrcTalentCommunication;
using QT.JZRC.Entitys.Dto.JzrcTalentHandover;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JZRC;

public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<JzrcCompanyCommunicationCrInput, JzrcCompanyCommunicationEntity>()
            .Map(dest => dest.Attachment, src => src.attachment.IsAny() ? src.attachment.ToJsonString() : string.Empty);

        config.ForType<JzrcCompanyCommunicationEntity, JzrcCompanyCommunicationInfoOutput>()
            .Map(dest => dest.attachment, src => string.IsNullOrEmpty(src.Attachment) ? new List<FileControlsModel>() : src.Attachment.ToObject<List<FileControlsModel>>());

        config.ForType<JzrcTalentCommunicationCrInput, JzrcTalentCommunicationEntity>()
            .Map(dest => dest.Attachment, src => src.attachment.IsAny() ? src.attachment.ToJsonString() : string.Empty);

        config.ForType<JzrcTalentCommunicationEntity, JzrcTalentCommunicationInfoOutput>()
            .Map(dest => dest.attachment, src => string.IsNullOrEmpty(src.Attachment) ? new List<FileControlsModel>() : src.Attachment.ToObject<List<FileControlsModel>>());

        config.ForType<JzrcStoreroomEntity, JzrcStoreroomSelectorOutput>()
            .Map(dest => dest.parentId, src => src.PId)
            .Map(dest => dest.fullName, src => src.Name);

        config.ForType<JzrcStoreroomEntity, JzrcStoreroomListOutput>()
    .Map(dest => dest.parentId, src => src.PId);

        config.ForType<JzrcCompanyJobCrInput, JzrcCompanyJobEntity>()
            .Map(dest => dest.Region, src => src.region.IsAny() ? string.Join(",", src.region) : "");

        config.ForType<JzrcCompanyJobEntity, JzrcCompanyJobInfoOutput>()
            .Map(dest => dest.region, src => src.Region.IsNotEmptyOrNull() ? src.Region.Split(",", true).ToList() : new List<string>());


        //    config.ForType<JzrcMemberCrInput, JzrcMemberEntity>()
        //        .Map(dest => dest.HeadIcon, src => src.headIcon.IsAny() ? src.headIcon.ToJsonString() : string.Empty);

        //    config.ForType<JzrcMemberUpInput, JzrcMemberEntity>()
        //.Map(dest => dest.HeadIcon, src => src.headIcon.IsAny() ? src.headIcon.ToJsonString() : string.Empty);


        //    config.ForType<JzrcMemberEntity, JzrcMemberInfoOutput>()
        //.Map(dest => dest.headIcon, src => string.IsNullOrEmpty(src.HeadIcon) ? new List<FileControlsModel>() : src.HeadIcon.ToObject<List<FileControlsModel>>());

        config.ForType<JzrcTalentImportDataTemplate, JzrcTalentEntity>()
            .Map(dest => dest.Gender, src => src.gender == "男" ? 1 : (src.gender == "女" ? 2 : 0));

        config.ForType<JzrcTalentCertificateCrInput, JzrcTalentCertificateEntity>()
    .Map(dest => dest.Attachment, src => src.attachment.IsAny() ? src.attachment.ToJsonString() : string.Empty)
    .Map(dest => dest.Region, src => src.region.IsAny() ? src.region.FirstOrDefault() : "");


        config.ForType<JzrcTalentCertificateEntity, JzrcTalentCertificateInfoOutput>()
            .Map(dest => dest.attachment, src => string.IsNullOrEmpty(src.Attachment) ? new List<FileControlsModel>() : src.Attachment.ToObject<List<FileControlsModel>>());
            //.Map(dest=>dest.region,src=>src.Region.IsNotEmptyOrNull()? new string[] {src.Region} : new string[0]);


        config.ForType<JzrcTalentHandoverCrInput, JzrcTalentHandoverEntity>()
           .Map(dest => dest.Attachment, src => src.attachment.IsAny() ? src.attachment.ToJsonString() : string.Empty);

        config.ForType<JzrcTalentHandoverEntity, JzrcTalentHandoverInfoOutput>()
            .Map(dest => dest.attachment, src => string.IsNullOrEmpty(src.Attachment) ? new List<FileControlsModel>() : src.Attachment.ToObject<List<FileControlsModel>>());

        config.ForType<JzrcTalentCrInput, JzrcTalentEntity>()
            .Map(dest => dest.Region, src => src.region.IsAny() ? string.Join(",", src.region) : "");

        config.ForType<JzrcTalentEntity, JzrcTalentInfoOutput>()
            .Map(dest => dest.region, src => src.Region.IsNotEmptyOrNull() ? src.Region.Split(",",true) : new string[0]);

        config.ForType<JzrcCompanyCrInput, JzrcCompanyEntity>()
    .Map(dest => dest.Province, src => src.province.IsAny() ? string.Join(",", src.province) : "");


        config.ForType<JzrcCompanyEntity, JzrcCompanyInfoOutput>()
            .Map(dest => dest.province, src => src.Province.IsNotEmptyOrNull() ? src.Province.Split(",", true) : new string[0]);

        config.ForType<JzrcMemberEntity, JzrcMemberInfoOutput>()
            .Map(dest => dest.role, src => ((int)src.Role).ToString());


        config.ForType<JzrcCertificateInCrInput, JzrcCertificateInEntity>()
            .Map(dest => dest.Attachment, src => src.attachment.IsAny() ? src.attachment.ToJsonString() : string.Empty);


        config.ForType<JzrcCertificateInEntity, JzrcCertificateInInfoOutput>()
            .Map(dest => dest.attachment, src => string.IsNullOrEmpty(src.Attachment) ? new List<FileControlsModel>() : src.Attachment.ToObject<List<FileControlsModel>>());
    }
}
