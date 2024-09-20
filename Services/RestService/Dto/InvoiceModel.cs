using Newtonsoft.Json;

namespace WebApp.Services.RestService.Dto;

public class InvoiceModel
    {
        [JsonProperty("nbmst", NullValueHandling = NullValueHandling.Include)]
        public string? Nbmst { get; set; }

        [JsonProperty("khmshdon", NullValueHandling = NullValueHandling.Include)]
        public int? Khmshdon { get; set; }

        [JsonProperty("khhdon", NullValueHandling = NullValueHandling.Include)]
        public string? Khhdon { get; set; }

        [JsonProperty("shdon", NullValueHandling = NullValueHandling.Include)]
        public int? Shdon { get; set; }

        [JsonProperty("cqt", NullValueHandling = NullValueHandling.Include)]
        public string? Cqt { get; set; }

        [JsonProperty("cttkhac", NullValueHandling = NullValueHandling.Include)]
        public List<object?> Cttkhac { get; set; } = [];

        [JsonProperty("dvtte", NullValueHandling = NullValueHandling.Include)]
        public string? Dvtte { get; set; }

        [JsonProperty("hdon", NullValueHandling = NullValueHandling.Include)]
        public string? Hdon { get; set; }

        [JsonProperty("hsgcma", NullValueHandling = NullValueHandling.Include)]
        public string? Hsgcma { get; set; }

        [JsonProperty("hsgoc", NullValueHandling = NullValueHandling.Include)]
        public string? Hsgoc { get; set; }

        [JsonProperty("hthdon", NullValueHandling = NullValueHandling.Include)]
        public int? Hthdon { get; set; }

        [JsonProperty("htttoan", NullValueHandling = NullValueHandling.Include)]
        public int? Htttoan { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Include)]
        public string? Id { get; set; }

        [JsonProperty("idtbao", NullValueHandling = NullValueHandling.Include)]
        public object? Idtbao { get; set; }

        [JsonProperty("khdon", NullValueHandling = NullValueHandling.Include)]
        public object? Khdon { get; set; }

        [JsonProperty("khhdgoc", NullValueHandling = NullValueHandling.Include)]
        public object? Khhdgoc { get; set; }

        [JsonProperty("khmshdgoc", NullValueHandling = NullValueHandling.Include)]
        public object? Khmshdgoc { get; set; }

        [JsonProperty("lhdgoc", NullValueHandling = NullValueHandling.Include)]
        public object? Lhdgoc { get; set; }

        [JsonProperty("mhdon", NullValueHandling = NullValueHandling.Include)]
        public string? Mhdon { get; set; }

        [JsonProperty("mtdiep", NullValueHandling = NullValueHandling.Include)]
        public object? Mtdiep { get; set; }

        [JsonProperty("mtdtchieu", NullValueHandling = NullValueHandling.Include)]
        public string? Mtdtchieu { get; set; }

        [JsonProperty("nbdchi", NullValueHandling = NullValueHandling.Include)]
        public string? Nbdchi { get; set; }

        [JsonProperty("nbhdktngay", NullValueHandling = NullValueHandling.Include)]
        public object? Nbhdktngay { get; set; }

        [JsonProperty("nbhdktso", NullValueHandling = NullValueHandling.Include)]
        public object? Nbhdktso { get; set; }

        [JsonProperty("nbhdso", NullValueHandling = NullValueHandling.Include)]
        public object? Nbhdso { get; set; }

        [JsonProperty("nblddnbo", NullValueHandling = NullValueHandling.Include)]
        public object? Nblddnbo { get; set; }

        [JsonProperty("nbptvchuyen", NullValueHandling = NullValueHandling.Include)]
        public object? Nbptvchuyen { get; set; }

        [JsonProperty("nbstkhoan", NullValueHandling = NullValueHandling.Include)]
        public object? Nbstkhoan { get; set; }

        [JsonProperty("nbten", NullValueHandling = NullValueHandling.Include)]
        public string? Nbten { get; set; }

        [JsonProperty("nbtnhang", NullValueHandling = NullValueHandling.Include)]
        public object? Nbtnhang { get; set; }

        [JsonProperty("nbtnvchuyen", NullValueHandling = NullValueHandling.Include)]
        public object? Nbtnvchuyen { get; set; }

        [JsonProperty("nbttkhac", NullValueHandling = NullValueHandling.Include)]
        public List<object?> Nbttkhac { get; set; } = [];

        [JsonProperty("ncma", NullValueHandling = NullValueHandling.Include)]
        public DateTime? Ncma { get; set; }

        [JsonProperty("ncnhat", NullValueHandling = NullValueHandling.Include)]
        public DateTime? Ncnhat { get; set; }

        [JsonProperty("ngcnhat", NullValueHandling = NullValueHandling.Include)]
        public string? Ngcnhat { get; set; }

        [JsonProperty("nky", NullValueHandling = NullValueHandling.Include)]
        public DateTime? Nky { get; set; }

        [JsonProperty("nmdchi", NullValueHandling = NullValueHandling.Include)]
        public string? Nmdchi { get; set; }

        [JsonProperty("nmmst", NullValueHandling = NullValueHandling.Include)]
        public string? Nmmst { get; set; }

        [JsonProperty("nmstkhoan", NullValueHandling = NullValueHandling.Include)]
        public object? Nmstkhoan { get; set; }

        [JsonProperty("nmten", NullValueHandling = NullValueHandling.Include)]
        public string? Nmten { get; set; }

        [JsonProperty("nmtnhang", NullValueHandling = NullValueHandling.Include)]
        public object? Nmtnhang { get; set; }

        [JsonProperty("nmtnmua", NullValueHandling = NullValueHandling.Include)]
        public object? Nmtnmua { get; set; }

        [JsonProperty("nmttkhac", NullValueHandling = NullValueHandling.Include)]
        public List<object?> Nmttkhac { get; set; } = [];

        [JsonProperty("ntao", NullValueHandling = NullValueHandling.Include)]
        public DateTime? Ntao { get; set; }

        [JsonProperty("ntnhan", NullValueHandling = NullValueHandling.Include)]
        public DateTime? Ntnhan { get; set; }

        [JsonProperty("pban", NullValueHandling = NullValueHandling.Include)]
        public string? Pban { get; set; }

        [JsonProperty("ptgui", NullValueHandling = NullValueHandling.Include)]
        public int? Ptgui { get; set; }

        [JsonProperty("shdgoc", NullValueHandling = NullValueHandling.Include)]
        public object? Shdgoc { get; set; }

        [JsonProperty("tchat", NullValueHandling = NullValueHandling.Include)]
        public int? Tchat { get; set; }

        [JsonProperty("tdlap", NullValueHandling = NullValueHandling.Include)]
        public DateTime? Tdlap { get; set; }

        [JsonProperty("tgia", NullValueHandling = NullValueHandling.Include)]
        public double? Tgia { get; set; }

        [JsonProperty("tgtcthue", NullValueHandling = NullValueHandling.Include)]
        public double? Tgtcthue { get; set; }

        [JsonProperty("tgtthue", NullValueHandling = NullValueHandling.Include)]
        public double? Tgtthue { get; set; }

        [JsonProperty("tgtttbchu", NullValueHandling = NullValueHandling.Include)]
        public string? Tgtttbchu { get; set; }

        [JsonProperty("tgtttbso", NullValueHandling = NullValueHandling.Include)]
        public double? Tgtttbso { get; set; }

        [JsonProperty("thdon", NullValueHandling = NullValueHandling.Include)]
        public string? Thdon { get; set; }

        [JsonProperty("thlap", NullValueHandling = NullValueHandling.Include)]
        public int? Thlap { get; set; }

        [JsonProperty("thttlphi", NullValueHandling = NullValueHandling.Include)]
        public List<object?> Thttlphi { get; set; } = [];

        [JsonProperty("thttltsuat", NullValueHandling = NullValueHandling.Include)]
        public List<Thttltsuat> Thttltsuat { get; set; } = [];

        [JsonProperty("tlhdon", NullValueHandling = NullValueHandling.Include)]
        public string? Tlhdon { get; set; }

        [JsonProperty("ttcktmai", NullValueHandling = NullValueHandling.Include)]
        public object? Ttcktmai { get; set; }

        [JsonProperty("tthai", NullValueHandling = NullValueHandling.Include)]
        public int? Tthai { get; set; }

        [JsonProperty("ttkhac", NullValueHandling = NullValueHandling.Include)]
        public List<object?> Ttkhac { get; set; } = [];

        [JsonProperty("tttbao", NullValueHandling = NullValueHandling.Include)]
        public int? Tttbao { get; set; }

        [JsonProperty("ttttkhac", NullValueHandling = NullValueHandling.Include)]
        public List<object?> Ttttkhac { get; set; } = [];

        [JsonProperty("ttxly", NullValueHandling = NullValueHandling.Include)]
        public int? Ttxly { get; set; }

        [JsonProperty("tvandnkntt", NullValueHandling = NullValueHandling.Include)]
        public string? Tvandnkntt { get; set; }

        [JsonProperty("mhso", NullValueHandling = NullValueHandling.Include)]
        public object? Mhso { get; set; }

        [JsonProperty("ladhddt", NullValueHandling = NullValueHandling.Include)]
        public int? Ladhddt { get; set; }

        [JsonProperty("mkhang", NullValueHandling = NullValueHandling.Include)]
        public object? Mkhang { get; set; }

        [JsonProperty("nbsdthoai", NullValueHandling = NullValueHandling.Include)]
        public object? Nbsdthoai { get; set; }

        [JsonProperty("nbdctdtu", NullValueHandling = NullValueHandling.Include)]
        public object? Nbdctdtu { get; set; }

        [JsonProperty("nbfax", NullValueHandling = NullValueHandling.Include)]
        public object? Nbfax { get; set; }

        [JsonProperty("nbwebsite", NullValueHandling = NullValueHandling.Include)]
        public object? Nbwebsite { get; set; }

        [JsonProperty("nbcks", NullValueHandling = NullValueHandling.Include)]
        public string? Nbcks { get; set; }

        [JsonProperty("nmsdthoai", NullValueHandling = NullValueHandling.Include)]
        public object? Nmsdthoai { get; set; }

        [JsonProperty("nmdctdtu", NullValueHandling = NullValueHandling.Include)]
        public object? Nmdctdtu { get; set; }

        [JsonProperty("nmcmnd", NullValueHandling = NullValueHandling.Include)]
        public object? Nmcmnd { get; set; }

        [JsonProperty("nmcks", NullValueHandling = NullValueHandling.Include)]
        public object? Nmcks { get; set; }

        [JsonProperty("bhphap", NullValueHandling = NullValueHandling.Include)]
        public int? Bhphap { get; set; }

        [JsonProperty("hddunlap", NullValueHandling = NullValueHandling.Include)]
        public object? Hddunlap { get; set; }

        [JsonProperty("gchdgoc", NullValueHandling = NullValueHandling.Include)]
        public object? Gchdgoc { get; set; }

        [JsonProperty("tbhgtngay", NullValueHandling = NullValueHandling.Include)]
        public object? Tbhgtngay { get; set; }

        [JsonProperty("bhpldo", NullValueHandling = NullValueHandling.Include)]
        public object? Bhpldo { get; set; }

        [JsonProperty("bhpcbo", NullValueHandling = NullValueHandling.Include)]
        public object? Bhpcbo { get; set; }

        [JsonProperty("bhpngay", NullValueHandling = NullValueHandling.Include)]
        public object? Bhpngay { get; set; }

        [JsonProperty("tdlhdgoc", NullValueHandling = NullValueHandling.Include)]
        public object? Tdlhdgoc { get; set; }

        [JsonProperty("tgtphi", NullValueHandling = NullValueHandling.Include)]
        public object? Tgtphi { get; set; }

        [JsonProperty("unhiem", NullValueHandling = NullValueHandling.Include)]
        public object? Unhiem { get; set; }

        [JsonProperty("mstdvnunlhdon", NullValueHandling = NullValueHandling.Include)]
        public object? Mstdvnunlhdon { get; set; }

        [JsonProperty("tdvnunlhdon", NullValueHandling = NullValueHandling.Include)]
        public object? Tdvnunlhdon { get; set; }

        [JsonProperty("nbmdvqhnsach", NullValueHandling = NullValueHandling.Include)]
        public object? Nbmdvqhnsach { get; set; }

        [JsonProperty("nbsqdinh", NullValueHandling = NullValueHandling.Include)]
        public object? Nbsqdinh { get; set; }

        [JsonProperty("nbncqdinh", NullValueHandling = NullValueHandling.Include)]
        public object? Nbncqdinh { get; set; }

        [JsonProperty("nbcqcqdinh", NullValueHandling = NullValueHandling.Include)]
        public object? Nbcqcqdinh { get; set; }

        [JsonProperty("nbhtban", NullValueHandling = NullValueHandling.Include)]
        public object? Nbhtban { get; set; }

        [JsonProperty("nmmdvqhnsach", NullValueHandling = NullValueHandling.Include)]
        public object? Nmmdvqhnsach { get; set; }

        [JsonProperty("nmddvchden", NullValueHandling = NullValueHandling.Include)]
        public object? Nmddvchden { get; set; }

        [JsonProperty("nmtgvchdtu", NullValueHandling = NullValueHandling.Include)]
        public object? Nmtgvchdtu { get; set; }

        [JsonProperty("nmtgvchdden", NullValueHandling = NullValueHandling.Include)]
        public object? Nmtgvchdden { get; set; }

        [JsonProperty("nbtnban", NullValueHandling = NullValueHandling.Include)]
        public object? Nbtnban { get; set; }

        [JsonProperty("dcdvnunlhdon", NullValueHandling = NullValueHandling.Include)]
        public object? Dcdvnunlhdon { get; set; }

        [JsonProperty("dksbke", NullValueHandling = NullValueHandling.Include)]
        public object? Dksbke { get; set; }

        [JsonProperty("dknlbke", NullValueHandling = NullValueHandling.Include)]
        public object? Dknlbke { get; set; }

        [JsonProperty("thtttoan", NullValueHandling = NullValueHandling.Include)]
        public string? Thtttoan { get; set; }

        [JsonProperty("msttcgp", NullValueHandling = NullValueHandling.Include)]
        public string? Msttcgp { get; set; }

        [JsonProperty("cqtcks", NullValueHandling = NullValueHandling.Include)]
        public string? Cqtcks { get; set; }

        [JsonProperty("gchu", NullValueHandling = NullValueHandling.Include)]
        public string? Gchu { get; set; }

        [JsonProperty("kqcht", NullValueHandling = NullValueHandling.Include)]
        public object? Kqcht { get; set; }

        [JsonProperty("hdntgia", NullValueHandling = NullValueHandling.Include)]
        public object? Hdntgia { get; set; }

        [JsonProperty("tgtkcthue", NullValueHandling = NullValueHandling.Include)]
        public object? Tgtkcthue { get; set; }

        [JsonProperty("tgtkhac", NullValueHandling = NullValueHandling.Include)]
        public object? Tgtkhac { get; set; }

        [JsonProperty("nmshchieu", NullValueHandling = NullValueHandling.Include)]
        public object? Nmshchieu { get; set; }

        [JsonProperty("nmnchchieu", NullValueHandling = NullValueHandling.Include)]
        public object? Nmnchchieu { get; set; }

        [JsonProperty("nmnhhhchieu", NullValueHandling = NullValueHandling.Include)]
        public object? Nmnhhhchieu { get; set; }

        [JsonProperty("nmqtich", NullValueHandling = NullValueHandling.Include)]
        public object? Nmqtich { get; set; }

        [JsonProperty("ktkhthue", NullValueHandling = NullValueHandling.Include)]
        public object? Ktkhthue { get; set; }

        [JsonProperty("hdhhdvu", NullValueHandling = NullValueHandling.Include)]
        public List<Hdhhdvu>? Hdhhdvu { get; set; }

        [JsonProperty("qrcode", NullValueHandling = NullValueHandling.Include)]
        public object? Qrcode { get; set; }

        [JsonProperty("ttmstten", NullValueHandling = NullValueHandling.Include)]
        public object? Ttmstten { get; set; }

        [JsonProperty("ladhddtten", NullValueHandling = NullValueHandling.Include)]
        public object? Ladhddtten { get; set; }

        [JsonProperty("hdxkhau", NullValueHandling = NullValueHandling.Include)]
        public object? Hdxkhau { get; set; }

        [JsonProperty("hdxkptquan", NullValueHandling = NullValueHandling.Include)]
        public object? Hdxkptquan { get; set; }

        [JsonProperty("hdgktkhthue", NullValueHandling = NullValueHandling.Include)]
        public object? Hdgktkhthue { get; set; }

        [JsonProperty("hdonLquans", NullValueHandling = NullValueHandling.Include)]
        public object? HdonLquans { get; set; }

        [JsonProperty("tthdclquan", NullValueHandling = NullValueHandling.Include)]
        public bool? Tthdclquan { get; set; }

        [JsonProperty("pdndungs", NullValueHandling = NullValueHandling.Include)]
        public object? Pdndungs { get; set; }

        [JsonProperty("hdtbssrses", NullValueHandling = NullValueHandling.Include)]
        public object? Hdtbssrses { get; set; }

        [JsonProperty("hdTrung", NullValueHandling = NullValueHandling.Include)]
        public object? HdTrung { get; set; }

        [JsonProperty("isHDTrung", NullValueHandling = NullValueHandling.Include)]
        public object? IsHDTrung { get; set; }

        public override string? ToString()
        {
            return $"InvNo: {Shdon} - Supplier: {Nbmst} - On: {Tdlap}";
        }
    }

    public class Thttltsuat
    {
        [JsonProperty("tsuat", NullValueHandling = NullValueHandling.Include)]
        public string? Tsuat { get; set; }

        [JsonProperty("thtien", NullValueHandling = NullValueHandling.Include)]
        public double? Thtien { get; set; }

        [JsonProperty("tthue", NullValueHandling = NullValueHandling.Include)]
        public double? Tthue { get; set; }

        [JsonProperty("gttsuat", NullValueHandling = NullValueHandling.Include)]
        public string? Gttsuat { get; set; }
    }

    public class Hdhhdvu
    {
        [JsonProperty("idhdon", NullValueHandling = NullValueHandling.Ignore)]
        public string? Idhdon { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("dgia", NullValueHandling = NullValueHandling.Ignore)]
        public int Dgia { get; set; }

        [JsonProperty("dvtinh", NullValueHandling = NullValueHandling.Ignore)]
        public string? Dvtinh { get; set; }

        [JsonProperty("ltsuat", NullValueHandling = NullValueHandling.Ignore)]
        public string? Ltsuat { get; set; }

        [JsonProperty("sluong", NullValueHandling = NullValueHandling.Ignore)]
        public int Sluong { get; set; }

        [JsonProperty("stbchu", NullValueHandling = NullValueHandling.Ignore)]
        public object? Stbchu { get; set; }

        [JsonProperty("stckhau", NullValueHandling = NullValueHandling.Ignore)]
        public object? Stckhau { get; set; }

        [JsonProperty("stt", NullValueHandling = NullValueHandling.Ignore)]
        public int Stt { get; set; }

        [JsonProperty("tchat", NullValueHandling = NullValueHandling.Ignore)]
        public int Tchat { get; set; }

        [JsonProperty("ten", NullValueHandling = NullValueHandling.Ignore)]
        public string? Ten { get; set; }

        [JsonProperty("thtcthue", NullValueHandling = NullValueHandling.Ignore)]
        public object? Thtcthue { get; set; }

        [JsonProperty("thtien", NullValueHandling = NullValueHandling.Ignore)]
        public int Thtien { get; set; }

        [JsonProperty("tlckhau", NullValueHandling = NullValueHandling.Ignore)]
        public object? Tlckhau { get; set; }

        [JsonProperty("tsuat", NullValueHandling = NullValueHandling.Ignore)]
        public double Tsuat { get; set; }

        [JsonProperty("tthue", NullValueHandling = NullValueHandling.Ignore)]
        public object? Tthue { get; set; }

        [JsonProperty("sxep", NullValueHandling = NullValueHandling.Ignore)]
        public int Sxep { get; set; }

        [JsonProperty("ttkhac", NullValueHandling = NullValueHandling.Ignore)]
        public List<object>? Ttkhac { get; set; }

        [JsonProperty("dvtte", NullValueHandling = NullValueHandling.Ignore)]
        public object? Dvtte { get; set; }

        [JsonProperty("tgia", NullValueHandling = NullValueHandling.Ignore)]
        public object? Tgia { get; set; }
    }
