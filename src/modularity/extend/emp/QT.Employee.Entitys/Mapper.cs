using Mapster;
using QT.Common.Extension;
using QT.Common.Models;
using QT.Common.Security;
using QT.Emp.Entitys;
using QT.Emp.Entitys.Dto.EmpDismissionEmployee;
using QT.Emp.Entitys.Dto.EmpEmployee;
using QT.Emp.Entitys.Dto.EmpTransferEmployee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Employee.Entitys;

public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<EmpEmployeeEntity, EmpEmployeeInfoOutput>()
            .Map(dest => dest.iDcardCertificate, src => src.IDcardCertificate.IsNotEmptyOrNull()? src.IDcardCertificate.ToObject<List<FileControlsModel>>() : new List<FileControlsModel>())
            .Map(dest => dest.academicCertificate, src => src.AcademicCertificate.IsNotEmptyOrNull() ? src.AcademicCertificate.ToObject<List<FileControlsModel>>() : new List<FileControlsModel>())
            .Map(dest => dest.personalPhoto, src => src.PersonalPhoto.IsNotEmptyOrNull() ? src.PersonalPhoto.ToObject<List<FileControlsModel>>() : new List<FileControlsModel>())
            .Map(dest => dest.attachment, src => src.Attachment.IsNotEmptyOrNull() ? src.Attachment.ToObject<List<FileControlsModel>>() : new List<FileControlsModel>())
            .Map(dest => dest.diplomaCertificate, src => src.DiplomaCertificate.IsNotEmptyOrNull() ? src.DiplomaCertificate.ToObject<List<FileControlsModel>>() : new List<FileControlsModel>());

        config.ForType<EmpEmployeeCrInput, EmpEmployeeEntity>()
            .Map(dest => dest.IDcardCertificate, src => src.iDcardCertificate.IsAny() ? src.iDcardCertificate.ToJsonString() : string.Empty)
            .Map(dest => dest.AcademicCertificate, src => src.academicCertificate.IsAny() ? src.academicCertificate.ToJsonString() : string.Empty)
            .Map(dest => dest.PersonalPhoto, src => src.personalPhoto.IsAny() ? src.personalPhoto.ToJsonString() : string.Empty)
            .Map(dest => dest.DiplomaCertificate, src => src.diplomaCertificate.IsAny() ? src.diplomaCertificate.ToJsonString() : string.Empty)
            .Map(dest => dest.Attachment, src => src.attachment.IsAny() ? src.attachment.ToJsonString() : string.Empty);

        config.ForType<EmpDismissionEmployeeEntity, EmpDismissionEmployeeInfoOutput>()
            .Map(dest => dest.reason, src => string.IsNullOrEmpty(src.Reason)?new string[0] : src.Reason.Split(",", true));

        config.ForType<EmpDismissionEmployeeCrInput, EmpDismissionEmployeeEntity>()
            .Map(dest => dest.Reason, src => src.reason.IsAny() ? string.Join(",", src.reason) : string.Empty);


        config.ForType<EmpTransferEmployeeCrInput, EmpTransferEmployeeEntity>()
            .Map(dest => dest.Attachment, src => src.attachment.IsAny() ? src.attachment.ToJsonString() : string.Empty);

        config.ForType<EmpTransferEmployeeEntity, EmpTransferEmployeeInfoOutput>()
            .Map(dest => dest.attachment, src => src.Attachment.IsNotEmptyOrNull() ? src.Attachment.ToObject<List<FileControlsModel>>() : new List<FileControlsModel>());

        config.ForType<EmpChangeEmployeeCrInput, EmpChangeEmployeeEntity>()
            .Map(dest => dest.Attachment, src => src.attachment.IsAny() ? src.attachment.ToJsonString() : string.Empty);
    }
}
