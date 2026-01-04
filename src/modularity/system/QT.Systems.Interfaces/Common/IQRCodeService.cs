using Microsoft.AspNetCore.Mvc;
using QT.Systems.Entitys.Dto.Common.QRCode;

namespace QT.Systems.Interfaces.System;

public interface IQRCodeService
{
    /// <summary>
    /// 生成Base64图片
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    string GenerateToString(string content);
}