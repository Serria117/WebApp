using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;


namespace WebApp.Services.CommonService;

public static partial class StringService
{
    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex SpaceRegex();
    
    [GeneratedRegex("\\p{IsCombiningDiacriticalMarks}+")]
    private static partial Regex UnsignRegex();
    
    public static string? RemoveSpace(this string? str)
    {
        return string.IsNullOrEmpty(str) ? null : SpaceRegex().Replace(str.Trim(), " ");
    }
    
    public static decimal ParseDecimal(this string text)
    {
        return string.IsNullOrEmpty(text) ? 0 : decimal.Parse(text);
    }
    
    public static string UnSign(this string s)
    {  
        var regex = UnsignRegex();
        var temp = s.Normalize(NormalizationForm.FormD);
        return regex.Replace(temp, string.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D').RemoveSpace()!;
    }

    public static string? GetXmlNodeValue(this XDocument doc, string nodeName)
    {
        return doc.Descendants()
          .FirstOrDefault(e => e.Name.LocalName == nodeName)?
          .Value;
    }
}