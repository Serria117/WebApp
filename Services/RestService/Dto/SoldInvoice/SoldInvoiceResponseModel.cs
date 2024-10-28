using Newtonsoft.Json;

namespace WebApp.Services.RestService.Dto.SoldInvoice;

public class SoldInvoiceResponseModel
{
    [JsonProperty("datas", NullValueHandling = NullValueHandling.Ignore)]
    public List<SoldInvoiceModel> Datas { get; set; } = new();

    [JsonProperty("total")]
    public int Total { get; set; }

    [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
    public string? State { get; set; }

    [JsonProperty("time", NullValueHandling = NullValueHandling.Ignore)]
    public int Time { get; set; }

    [JsonProperty("error",NullValueHandling = NullValueHandling.Ignore)]
    public string? Error { get; set; }
}