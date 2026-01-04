using System;
using System.Collections.Generic;
using System.Text;

namespace QT.CMS;

public static class HtmlHelper
{
    #region 去除富文本中的HTML标签
    /// <summary>
    /// 去除富文本中的HTML标签
    /// </summary>
    /// <param name="html"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string ReplaceHtmlTag(string html, int length = 0)
    {
        string strText = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", "");
        strText = System.Text.RegularExpressions.Regex.Replace(strText, "&[^;]+;", "");

        if (length > 0 && strText.Length > length)
            return strText.Substring(0, length);

        return strText;
    }
    #endregion

    /// <summary>
    /// 截取字符长度
    /// </summary>
    /// <param name="inputString">字符</param>
    /// <param name="len">长度</param>
    public static string CutString(string? inputString, int len)
    {
        if (inputString == null) return string.Empty;
        string newString = string.Empty;
        inputString = ReplaceHtmlTag(inputString);
        if (inputString.Length <= len)
        {
            newString = inputString;
        }
        else
        {
            if (inputString.Length > 3)
            {
                newString = inputString.Substring(0, len - 3) + "...";
            }
            else
            {
                newString = inputString.Substring(0, len);
            }

        }
        return newString;
    }
}
