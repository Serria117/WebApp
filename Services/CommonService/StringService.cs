using System.Text;
using System.Text.RegularExpressions;

namespace WebApp.Services.CommonService;

public static partial class StringService
{
    public static string? RemoveSpace(this string? str)
    {
        return string.IsNullOrEmpty(str) ? null : MyRegex().Replace(str.Trim(), " ");
    }

    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex MyRegex();
    
    public static string UnSign(this string s)
    {  
        Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
        string temp = s.Normalize(NormalizationForm.FormD);
        return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D').RemoveSpace()!;
    }  
}