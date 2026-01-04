using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace QT.CMS;

/// <summary>
/// 文件读写封装
/// 注意：contentPath需要在Startup初始化
/// </summary>
public class FileHelper
{
    static string? _contentPath { get; set; }
    public FileHelper(string contentPath)
    {
        _contentPath = contentPath;
    }

    #region 文件上传辅助方法
    /// <summary>
    /// 获取文件类库的物理路径
    /// </summary>
    /// <param name="fileName">文件路径</param>
    public static string GetCurrPath(string fileName)
    {
        return Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + fileName);
    }

    /// <summary>
    /// 获取文件项目的物理路径
    /// </summary>
    /// <param name="fileName">文件路径</param>
    public static string GetRootPath(string fileName)
    {
        return Path.GetFullPath(_contentPath + fileName);
    }

    /// <summary>
    /// 获取文件站点的物理路径
    /// </summary>
    /// <param name="fileName">文件路径</param>
    public static string GetWebPath(string? fileName)
    {
        return Path.GetFullPath(_contentPath + $"/{DTKeys.DIRECTORY_WEB_PATH}/" + fileName);
    }

    /// <summary>
    /// 转换为字节数组
    /// </summary>
    /// <param name="fullName">文件物理路径含文件名</param>
    /// <returns>字节数组</returns>
    public static byte[] GetBinaryFile(string? fullName)
    {
        if (File.Exists(fullName))
        {
            FileStream? Fsm = null;
            try
            {
                Fsm = File.OpenRead(fullName);
                return ConvertStreamToByteBuffer(Fsm);
            }
            catch
            {
                return new byte[0];
            }
            finally
            {
                Fsm?.Close();
            }
        }
        else
        {
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// 流转化为字节数组
    /// </summary>
    /// <param name="theStream">流</param>
    /// <returns>字节数组</returns>
    public static byte[] ConvertStreamToByteBuffer(Stream theStream)
    {
        int bi;
        MemoryStream tempStream = new System.IO.MemoryStream();
        try
        {
            while ((bi = theStream.ReadByte()) != -1)
            {
                tempStream.WriteByte(((byte)bi));
            }
            return tempStream.ToArray();
        }
        catch
        {
            return new byte[0];
        }
        finally
        {
            tempStream.Close();
        }
    }

    /// <summary>
    /// 文件流上传文件
    /// </summary>
    /// <param name="binData">字节数组</param>
    /// <param name="fullName">文件物理路径含文件名</param>
    public static bool SaveFile(byte[] binData, string fullName)
    {
        FileStream? fileStream = null;
        MemoryStream m = new MemoryStream(binData);
        try
        {
            fileStream = new FileStream(fullName, FileMode.Create);
            m.WriteTo(fileStream);
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            m.Close();
            fileStream?.Close();
        }
    }

    /// <summary>
    /// 删除本地单个文件
    /// </summary>
    /// <param name="filePath">相对路径</param>
    public static bool DeleteFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return false;
        }
        string fullPath = GetWebPath(filePath);
        if (string.IsNullOrEmpty(fullPath))
        {
            return false;
        }
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 删除本地上传的文件(及缩略图)
    /// </summary>
    /// <param name="filePath">相对路径</param>
    public static void DeleteUpFile(string rootPath, string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }
        string fullPath = GetWebPath(filePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        if (filePath.LastIndexOf("/") >= 0)
        {
            int lastIndex = filePath.LastIndexOf("/");
            string thumbPath = filePath.Substring(0, lastIndex) + "thumb_" + filePath.Substring(lastIndex + 1);
            string fullTPATH = rootPath + thumbPath;//宿略图
            if (File.Exists(fullTPATH))
            {
                File.Delete(fullTPATH);
            }
        }
    }

    /// <summary>
    /// 删除内容图片
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="startstr">匹配开头字符串</param>
    public static void DeleteContentPic(string content, string startstr)
    {
        if (string.IsNullOrEmpty(content))
        {
            return;
        }
        Regex reg = new Regex("IMG[^>]*?src\\s*=\\s*(?:\"(?<1>[^\"]*)\"|'(?<1>[^\']*)')", RegexOptions.IgnoreCase);
        MatchCollection m = reg.Matches(content);
        foreach (Match math in m)
        {
            string imgUrl = math.Groups[1].Value;
            string fullPath = GetWebPath(imgUrl);
            try
            {
                if (imgUrl.ToLower().StartsWith(startstr.ToLower()) && File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch { }
        }
    }

    /// <summary>
    /// 删除指定文件夹
    /// </summary>
    /// <param name="dirpath">文件绝对路径</param>
    public static bool DeleteDirectory(string dirPath)
    {
        if (string.IsNullOrEmpty(dirPath))
        {
            return false;
        }
        if (Directory.Exists(dirPath))
        {
            Directory.Delete(dirPath, true);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 修改指定文件夹名称
    /// </summary>
    /// <param name="oldDirPath">旧绝对路径</param>
    /// <param name="newDirPath">新绝对路径</param>
    public static bool MoveDirectory(string oldDirPath, string newDirPath)
    {
        if (string.IsNullOrEmpty(oldDirPath))
        {
            return false;
        }
        if (Directory.Exists(oldDirPath))
        {
            Directory.Move(oldDirPath, newDirPath);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 返回文件大小KB
    /// </summary>
    /// <param name="filePath">文件绝对路径</param>
    public static int GetFileSize(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return 0;
        }
        if (File.Exists(filePath))
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return ((int)fileInfo.Length) / 1024;
        }
        return 0;
    }

    /// <summary>
    /// 将大小转换成存储单位
    /// </summary>
    /// <param name="size">单位字节</param>
    /// <returns></returns>
    public static string ByteConvertStorage(long size)
    {
        string strSize = string.Empty;
        if (size < 1024.00)
            strSize = size + " B";
        else if (size >= 1024.00 && size < 1048576)
            strSize = Math.Round(Convert.ToDouble(size / 1024.00), 2) + " KB";
        else if (size >= 1048576 && size < 1073741824)
            strSize = Math.Round(Convert.ToDouble(size / 1024.00 / 1024.00), 2) + " MB";
        else if (size >= 1073741824)
            strSize = Math.Round(Convert.ToDouble(size / 1024.00 / 1024.00 / 1024.00), 2) + " GB";
        return strSize;
    }


    /// <summary>
    /// 返回文件扩展名，不含“.”
    /// </summary>
    /// <param name="filePath">文件全名称</param>
    /// <returns>string</returns>
    public static string GetFileExt(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return string.Empty;
        }
        return Path.GetExtension(filePath).Trim('.');
    }

    /// <summary>
    /// 返回文件扩展名，含“.”
    /// </summary>
    /// <param name="filePath">文件全名称</param>
    /// <returns>string</returns>
    public static string GetFileFullExt(string filePath) {
        string ext= GetFileExt(filePath);
        if (!string.IsNullOrWhiteSpace(ext))
        {
            return $".{ext}";
        }
        else
        {
            return ext;
        }
    }

    /// <summary>
    /// 返回文件名，不含路径
    /// </summary>
    /// <param name="filePath">文件相对路径</param>
    /// <returns>string</returns>
    public static string GetFileName(string filePath)
    {
        return filePath.Substring(filePath.LastIndexOf(@"/") + 1);
    }

    /// <summary>
    /// 文件是否存在
    /// </summary>
    /// <param name="filePath">文件绝对路径</param>
    /// <returns>bool</returns>
    public static bool FileExists(string filePath)
    {
        if (File.Exists(filePath))
        {
            return true;
        }
        return false;
    }
    #endregion
}
