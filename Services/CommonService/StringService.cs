using System.Text;
using System.Text.RegularExpressions;

namespace WebApp.Services.CommonService;

public static partial class StringService
{
    public static string? RemoveSpace(this string? str)
    {
        return string.IsNullOrEmpty(str) ? null : SpaceRegex().Replace(str.Trim(), " ");
    }

    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex SpaceRegex();
    
    public static string UnSign(this string s)
    {  
        var regex = UnsignRegex();
        var temp = s.Normalize(NormalizationForm.FormD);
        return regex.Replace(temp, string.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D').RemoveSpace()!;
    }

    [GeneratedRegex("\\p{IsCombiningDiacriticalMarks}+")]
    private static partial Regex UnsignRegex();
}