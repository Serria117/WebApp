using Newtonsoft.Json;

namespace WebApp.Services.RestService.Dto.SoldInvoice;

using Newtonsoft.Json;
using System.Collections.Generic;

public class SoldInvoiceModel
{
    [JsonProperty("nbmst", NullValueHandling = NullValueHandling.Ignore)]
    public string Nbmst { get; set; }

    [JsonProperty("khmshdon", NullValueHandling = NullValueHandling.Ignore)]
    public int Khmshdon { get; set; }

    [JsonProperty("khhdon", NullValueHandling = NullValueHandling.Ignore)]
    public string Khhdon { get; set; }

    [JsonProperty("shdon", NullValueHandling = NullValueHandling.Ignore)]
    public int Shdon { get; set; }

    [JsonProperty("cqt", NullValueHandling = NullValueHandling.Ignore)]
    public string Cqt { get; set; }

    [JsonProperty("cttkhac", NullValueHandling = NullValueHandling.Ignore)]
    public List<object> Cttkhac { get; set; }

    [JsonProperty("dvtte", NullValueHandling = NullValueHandling.Ignore)]
    public string Dvtte { get; set; }

    [JsonProperty("hdon", NullValueHandling = NullValueHandling.Ignore)]
    public string Hdon { get; set; }

    [JsonProperty("hsgcma", NullValueHandling = NullValueHandling.Ignore)]
    public string Hsgcma { get; set; }

    [JsonProperty("hsgoc", NullValueHandling = NullValueHandling.Ignore)]
    public string Hsgoc { get; set; }

    [JsonProperty("hthdon", NullValueHandling = NullValueHandling.Ignore)]
    public int Hthdon { get; set; }

    [JsonProperty("htttoan", NullValueHandling = NullValueHandling.Ignore)]
    public int Htttoan { get; set; }

    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public string Id { get; set; }

    [JsonProperty("idtbao", NullValueHandling = NullValueHandling.Ignore)]
    public object Idtbao { get; set; }

    [JsonProperty("khdon", NullValueHandling = NullValueHandling.Ignore)]
    public object Khdon { get; set; }

    [JsonProperty("khhdgoc", NullValueHandling = NullValueHandling.Ignore)]
    public object Khhdgoc { get; set; }

    [JsonProperty("khmshdgoc", NullValueHandling = NullValueHandling.Ignore)]
    public object Khmshdgoc { get; set; }

    [JsonProperty("lhdgoc", NullValueHandling = NullValueHandling.Ignore)]
    public object Lhdgoc { get; set; }

    [JsonProperty("mhdon", NullValueHandling = NullValueHandling.Ignore)]
    public string Mhdon { get; set; }

    [JsonProperty("mtdiep", NullValueHandling = NullValueHandling.Ignore)]
    public object Mtdiep { get; set; }

    [JsonProperty("mtdtchieu", NullValueHandling = NullValueHandling.Ignore)]
    public string Mtdtchieu { get; set; }

    [JsonProperty("nbdchi", NullValueHandling = NullValueHandling.Ignore)]
    public string Nbdchi { get; set; }

    [JsonProperty("nbhdktngay", NullValueHandling = NullValueHandling.Ignore)]
    public object Nbhdktngay { get; set; }

    [JsonProperty("nbhdktso", NullValueHandling = NullValueHandling.Ignore)]
    public object Nbhdktso { get; set; }

    [JsonProperty("nbhdso", NullValueHandling = NullValueHandling.Ignore)]
    public object Nbhdso { get; set; }

    [JsonProperty("nblddnbo", NullValueHandling = NullValueHandling.Ignore)]
    public object Nblddnbo { get; set; }

    [JsonProperty("nbptvchuyen", NullValueHandling = NullValueHandling.Ignore)]
    public object Nbptvchuyen { get; set; }

    [JsonProperty("nbstkhoan", NullValueHandling = NullValueHandling.Ignore)]
    public object Nbstkhoan { get; set; }

    [JsonProperty("nbten", NullValueHandling = NullValueHandling.Ignore)]
    public string Nbten { get; set; }

    [JsonProperty("nbtnhang", NullValueHandling = NullValueHandling.Ignore)]
    public object Nbtnhang { get; set; }

    [JsonProperty("nbtnvchuyen", NullValueHandling = NullValueHandling.Ignore)]
    public object Nbtnvchuyen { get; set; }

    [JsonProperty("nbttkhac", NullValueHandling = NullValueHandling.Ignore)]
    public List<object> Nbttkhac { get; set; }

    [JsonProperty("ncma", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? Ncma { get; set; }

    [JsonProperty("ncnhat", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? Ncnhat { get; set; }

    [JsonProperty("ngcnhat", NullValueHandling = NullValueHandling.Ignore)]
    public string Ngcnhat { get; set; }

    [JsonProperty("nky", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? Nky { get; set; }

    [JsonProperty("nmdchi", NullValueHandling = NullValueHandling.Ignore)]
    public string Nmdchi { get; set; }

    [JsonProperty("nmmst", NullValueHandling = NullValueHandling.Ignore)]
    public string Nmmst { get; set; }

    [JsonProperty("nmstkhoan", NullValueHandling = NullValueHandling.Ignore)]
    public object Nmstkhoan { get; set; }

    [JsonProperty("nmten", NullValueHandling = NullValueHandling.Ignore)]
    public string Nmten { get; set; }

    [JsonProperty("nmtnhang", NullValueHandling = NullValueHandling.Ignore)]
    public object Nmtnhang { get; set; }

    [JsonProperty("nmtnmua", NullValueHandling = NullValueHandling.Ignore)]
    public object Nmtnmua { get; set; }

    [JsonProperty("nmttkhac", NullValueHandling = NullValueHandling.Ignore)]
    public List<object> Nmttkhac { get; set; }

    [JsonProperty("ntao", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? Ntao { get; set; }

    [JsonProperty("ntnhan", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? Ntnhan { get; set; }

    [JsonProperty("pban", NullValueHandling = NullValueHandling.Ignore)]
    public string Pban { get; set; }

    [JsonProperty("ptgui", NullValueHandling = NullValueHandling.Ignore)]
    public int Ptgui { get; set; }

    [JsonProperty("shdgoc", NullValueHandling = NullValueHandling.Ignore)]
    public object Shdgoc { get; set; }

    [JsonProperty("tchat", NullValueHandling = NullValueHandling.Ignore)]
    public int Tchat { get; set; }

    [JsonProperty("tdlap", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? Tdlap { get; set; }

    [JsonProperty("tgia", NullValueHandling = NullValueHandling.Ignore)]
    public double? Tgia { get; set; }

    [JsonProperty("tgtcthue", NullValueHandling = NullValueHandling.Ignore)]
    public double? Tgtcthue { get; set; }

    [JsonProperty("tgtthue", NullValueHandling = NullValueHandling.Ignore)]
    public double? Tgtthue { get; set; }

    [JsonProperty("tgtttbchu", NullValueHandling = NullValueHandling.Ignore)]
    public string Tgtttbchu { get; set; }

    [JsonProperty("tgtttbso", NullValueHandling = NullValueHandling.Ignore)]
    public double? Tgtttbso { get; set; }

    [JsonProperty("thdon", NullValueHandling = NullValueHandling.Ignore)]
    public string Thdon { get; set; }

    [JsonProperty("thlap", NullValueHandling = NullValueHandling.Ignore)]
    public int Thlap { get; set; }

    [JsonProperty("thttlphi", NullValueHandling = NullValueHandling.Ignore)]
    public List<object> Thttlphi { get; set; }

    [JsonProperty("thttltsuat", NullValueHandling = NullValueHandling.Ignore)]
    public List<Thttltsuat> Thttltsuat { get; set; }

    [JsonProperty("tlhdon", NullValueHandling = NullValueHandling.Ignore)]
    public string Tlhdon { get; set; }

    [JsonProperty("ttcktmai", NullValueHandling = NullValueHandling.Ignore)]
    public double? Ttcktmai { get; set; }

    [JsonProperty("tthai", NullValueHandling = NullValueHandling.Ignore)]
    public int Tthai { get; set; }

    [JsonProperty("ttkhac", NullValueHandling = NullValueHandling.Ignore)]
    public List<object> Ttkhac { get; set; }

    [JsonProperty("tttbao", NullValueHandling = NullValueHandling.Ignore)]
    public int Tttbao { get; set; }

    [JsonProperty("ttttkhac", NullValueHandling = NullValueHandling.Ignore)]
    public List<object> Ttttkhac { get; set; }

    [JsonProperty("ttxly", NullValueHandling = NullValueHandling.Ignore)]
    public int Ttxly { get; set; }

    [JsonProperty("tvandnkntt", NullValueHandling = NullValueHandling.Ignore)]
    public string Tvandnkntt { get; set; }

    [JsonProperty("mhso", NullValueHandling = NullValueHandling.Ignore)]
    public object Mhso { get; set; }

    [JsonProperty("ladhddt", NullValueHandling = NullValueHandling.Ignore)]
    public int Ladhddt { get; set; }

    [JsonProperty("mkhang", NullValueHandling = NullValueHandling.Ignore)]
    public object Mkhang { get; set; }

    [JsonProperty("nbsdthoai", NullValueHandling = NullValueHandling.Ignore)]
    public object Nbsdthoai { get; set; }

    [JsonProperty("nbdctdtu", NullValueHandling = NullValueHandling.Ignore)]
    public object Nbdctdtu { get; set; }

    [JsonProperty("nbfax", NullValueHandling = NullValueHandling.Ignore)]
    public object Nbfax { get; set; }

    [JsonProperty("nbwebsite", NullValueHandling = NullValueHandling.Ignore)]
    public object Nbwebsite { get; set; }

    [JsonProperty("nbcks", NullValueHandling = NullValueHandling.Ignore)]
    public string Nbcks { get; set; }

    [JsonProperty("nmsdthoai", NullValueHandling = NullValueHandling.Ignore)]
    public object Nmsdthoai { get; set; }

    [JsonProperty("nmdctdtu", NullValueHandling = NullValueHandling.Ignore)]
    public object Nmdctdtu { get; set; }

    [JsonProperty("nmcmnd", NullValueHandling = NullValueHandling.Ignore)]
    public object Nmcmnd { get; set; }

    [JsonProperty("nmcks", NullValueHandling = NullValueHandling.Ignore)]
    public object Nmcks { get; set; }

    [JsonProperty("bhphap", NullValueHandling = NullValueHandling.Ignore)]
    public int Bhphap { get; set; }

    [JsonProperty("hddunlap", NullValueHandling = NullValueHandling.Ignore)]
    public object Hddunlap { get; set; }

    [JsonProperty("gchdgoc", NullValueHandling = NullValueHandling.Ignore)]
    public object Gchdgoc { get; set; }

    [JsonProperty("tbhgtngay", NullValueHandling = NullValueHandling.Ignore)]
    public object Tbhgtngay { get; set; }

    [JsonProperty("bhpldo", NullValueHandling = NullValueHandling.Ignore)]
    public object Bhpldo { get; set; }

    [JsonProperty("bhpcbo", NullValueHandling = NullValueHandling.Ignore)]
    public object Bhpcbo { get; set; }

    [JsonProperty("bhpngay", NullValueHandling = NullValueHandling.Ignore)]
    public object Bhpngay { get; set; }

    [JsonProperty("tdlhdgoc", NullValueHandling = NullValueHandling.Ignore)]
    public object Tdlhdgoc { get; set; }

    [JsonProperty("tgtphi", NullValueHandling = NullValueHandling.Ignore)]
    public object Tgtphi { get; set; }

    [JsonProperty("unhiem", NullValueHandling = NullValueHandling.Ignore)]
    public object Unhiem { get; set; }

    [JsonProperty("mstdvnunlhdon", NullValueHandling = NullValueHandling.Ignore)]
    public object Mstdvnunlhdon { get; set; }

    [JsonProperty("tdvnunlhdon", NullValueHandling = NullValueHandling.Ignore)]
    public object Tdvnunlhdon { get; set; }

    [JsonProperty("nbmdvqhnsach", NullValueHandling = NullValueHandling.Ignore)]
    public object Nbmdvqhnsach { get; set; }

    [JsonProperty("nbsqdinh", NullValueHandling = NullValueHandling.Ignore)]
    public object Nbsqdinh { get; set; }

    [JsonProperty("nbncqdinh", NullValueHandling = NullValueHandling.Ignore)]
    public object Nbncqdinh { get; set; }

    [JsonProperty("nbcqcqdinh", NullValueHandling = NullValueHandling.Ignore)]
    public object Nbcqcqdinh { get; set; }

    [JsonProperty("nbhtban", NullValueHandling = NullValueHandling.Ignore)]
    public object Nbhtban { get; set; }

    [JsonProperty("nmmdvqhnsach", NullValueHandling = NullValueHandling.Ignore)]
    public object Nmmdvqhnsach { get; set; }

    [JsonProperty("nmddvchden", NullValueHandling = NullValueHandling.Ignore)]
    public object Nmddvchden { get; set; }

    [JsonProperty("nmtgvchdtu", NullValueHandling = NullValueHandling.Ignore)]
    public object Nmtgvchdtu { get; set; }

    [JsonProperty("nmtgvchdden", NullValueHandling = NullValueHandling.Ignore)]
    public object Nmtgvchdden { get; set; }

    [JsonProperty("nbtnban", NullValueHandling = NullValueHandling.Ignore)]
    public object Nbtnban { get; set; }

    [JsonProperty("dcdvnunlhdon", NullValueHandling = NullValueHandling.Ignore)]
    public object Dcdvnunlhdon { get; set; }

    [JsonProperty("dksbke", NullValueHandling = NullValueHandling.Ignore)]
    public object Dksbke { get; set; }

    [JsonProperty("dknlbke", NullValueHandling = NullValueHandling.Ignore)]
    public object Dknlbke { get; set; }

    [JsonProperty("thtttoan", NullValueHandling = NullValueHandling.Ignore)]
    public string Thtttoan { get; set; }

    [JsonProperty("msttcgp", NullValueHandling = NullValueHandling.Ignore)]
    public string Msttcgp { get; set; }

    [JsonProperty("cqtcks", NullValueHandling = NullValueHandling.Ignore)]
    public string Cqtcks { get; set; }

    [JsonProperty("gchu", NullValueHandling = NullValueHandling.Ignore)]
    public string Gchu { get; set; }

    [JsonProperty("kqcht", NullValueHandling = NullValueHandling.Ignore)]
    public object Kqcht { get; set; }

    [JsonProperty("hdntgia", NullValueHandling = NullValueHandling.Ignore)]
    public object Hdntgia { get; set; }

    [JsonProperty("tgtkcthue", NullValueHandling = NullValueHandling.Ignore)]
    public object Tgtkcthue { get; set; }

    [JsonProperty("tgtkhac", NullValueHandling = NullValueHandling.Ignore)]
    public object Tgtkhac { get; set; }

    [JsonProperty("nmshchieu", NullValueHandling = NullValueHandling.Ignore)]
    public object Nmshchieu { get; set; }

    [JsonProperty("nmnchchieu", NullValueHandling = NullValueHandling.Ignore)]
    public object Nmnchchieu { get; set; }

    [JsonProperty("nmnhhhchieu", NullValueHandling = NullValueHandling.Ignore)]
    public object Nmnhhhchieu { get; set; }

    [JsonProperty("nmqtich", NullValueHandling = NullValueHandling.Ignore)]
    public object Nmqtich { get; set; }

    [JsonProperty("ktkhthue", NullValueHandling = NullValueHandling.Ignore)]
    public object Ktkhthue { get; set; }

    [JsonProperty("hdhhdvu", NullValueHandling = NullValueHandling.Ignore)]
    public object Hdhhdvu { get; set; }

    [JsonProperty("qrcode", NullValueHandling = NullValueHandling.Ignore)]
    public object Qrcode { get; set; }

    [JsonProperty("ttmstten", NullValueHandling = NullValueHandling.Ignore)]
    public object Ttmstten { get; set; }

    [JsonProperty("ladhddtten", NullValueHandling = NullValueHandling.Ignore)]
    public object Ladhddtten { get; set; }

    [JsonProperty("hdxkhau", NullValueHandling = NullValueHandling.Ignore)]
    public object Hdxkhau { get; set; }

    [JsonProperty("hdxkptquan", NullValueHandling = NullValueHandling.Ignore)]
    public object Hdxkptquan { get; set; }

    [JsonProperty("hdgktkhthue", NullValueHandling = NullValueHandling.Ignore)]
    public object Hdgktkhthue { get; set; }

    [JsonProperty("hdonLquans", NullValueHandling = NullValueHandling.Ignore)]
    public object HdonLquans { get; set; }

    [JsonProperty("tthdclquan", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Tthdclquan { get; set; }

    [JsonProperty("pdndungs", NullValueHandling = NullValueHandling.Ignore)]
    public object Pdndungs { get; set; }

    [JsonProperty("hdtbssrses", NullValueHandling = NullValueHandling.Ignore)]
    public object Hdtbssrses { get; set; }

    [JsonProperty("hdTrung", NullValueHandling = NullValueHandling.Ignore)]
    public object HdTrung { get; set; }

    [JsonProperty("isHDTrung", NullValueHandling = NullValueHandling.Ignore)]
    public object IsHDTrung { get; set; }
}

public class ThttlTsuat
{
    [JsonProperty("tsuat", NullValueHandling = NullValueHandling.Ignore)]
    public string Tsuat { get; set; }

    [JsonProperty("thtien", NullValueHandling = NullValueHandling.Ignore)]
    public int Thtien { get; set; }

    [JsonProperty("tthue", NullValueHandling = NullValueHandling.Ignore)]
    public int Tthue { get; set; }

    [JsonProperty("gttsuat", NullValueHandling = NullValueHandling.Ignore)]
    public object Gttsuat { get; set; }
}