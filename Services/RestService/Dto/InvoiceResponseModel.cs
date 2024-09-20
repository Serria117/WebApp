using Newtonsoft.Json;

namespace WebApp.Services.RestService.Dto;

public class InvoiceResponseModel
{
    [JsonProperty("datas", NullValueHandling = NullValueHandling.Include)]
    public List<InvoiceModel> Datas { get; set; } = [];

    [JsonProperty("total")]
    public int Total { get; set; }

    [JsonProperty("state", NullValueHandling = NullValueHandling.Include)]
    public string? State { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? Error { get; set; }
}