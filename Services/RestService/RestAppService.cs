using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using Newtonsoft.Json;
using RestSharp;
using WebApp.Authentication;
using WebApp.Enums;
using WebApp.Mongo.DeserializedModel;
using WebApp.Payloads;
using WebApp.Services.InvoiceService.dto;
using WebApp.Services.RestService.Dto;
using WebApp.Services.RestService.Dto.SoldInvoice;
using WebApp.SignalrConfig;
using WebApp.Utils;

namespace WebApp.Services.RestService;

public interface IRestAppService
{
    Task<AppResponse> Authenticate(InvoiceLoginModel login);
    Task<CaptchaModel?> GetCaptcha();
    Task<AppResponse> GetPurchaseInvoiceListInRange(string token, string from, string to);
    /// <summary>
    /// Attempt to get an invoice's detail of goods sold
    /// </summary>
    /// <param name="token">Bearer token to use in the request to hoadondientu service</param>
    /// <param name="invoice">The invoice object to get detail</param>
    /// <returns>A response object containing the result of the request</returns>
    Task<AppResponse> GetInvoiceDetail(string token, InvoiceDisplayDto invoice);

    Task<AppResponse> GetSoldInvoiceListAsync(string token, string from, string to);
}

public class RestAppService(IRestClient restClient,
                            RestSharpSetting setting,
                            ILogger<RestAppService> logger,
                            IHubContext<AppHub> hubContext) : IRestAppService
{
    #region Authentication
    public async Task<CaptchaModel?> GetCaptcha()
    {
        var request = new RestRequest("/captcha", Method.Get);
        request.AddHeader("Cookie", setting.Cookie);
        var response = await restClient.ExecuteAsync<CaptchaModel>(request);
        if (response is not { IsSuccessStatusCode: true, Data: not null }) return null;
        var data = response.Data;
        return data;
    }

    public async Task<AppResponse> Authenticate(InvoiceLoginModel login)
    {
        var request = new RestRequest("/security-taxpayer/authenticate", Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Cookie", setting.AuthCookie);
        var requestBody = new
        {
            username = login.Username,
            password = login.Password,
            cvalue = login.Cvalue,
            ckey = login.Ckey
        };
        request.AddBody(requestBody);

        var response = await restClient.ExecuteAsync<TokenModel>(request);

        if (response is { IsSuccessful: true, Data: not null })
        {
            return AppResponse.SuccessResponse(response.Data);
        }

        return new AppResponse
        {
            Success = false,
            Message = $"Error: {response.StatusCode}"
        };
    }
    
    #endregion

    #region SOLD INVOICE METHODS

    public async Task<AppResponse> GetSoldInvoiceListAsync(string token, string from, string to)
    {

        List<SoldInvoiceModel> invoicesList = [];
        
        var fromValue = DateTime.ParseExact(from, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None);
        var toValue = DateTime.ParseExact(to, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None);

        if (fromValue > toValue) throw new InvalidDataException("[From date] can not be greater than [To date]");

        var dateRanges = CommonUtil.SplitDateRange(fromValue, toValue);

        foreach (var dateRange in dateRanges)
        {
            var pageCount = 1;
            await Task.Delay(800);
            var result = await GetSoldInvoiceFromService(token, dateRange.GetFromDateString(), dateRange.GetToDateString());
            if (result == null) continue;
            invoicesList.AddRange(result.Datas);
            if (result.State == null) continue;
            var nextState = result.State;
            while (true)
            {
                pageCount++;
                var nextResult = await GetSoldInvoiceFromService(token,
                                                                     dateRange.GetFromDateString(),
                                                                     dateRange.GetToDateString(),
                                                                     nextState);
                if (nextResult == null) break;
                invoicesList.AddRange(nextResult.Datas);
                if (nextResult.State == null) break;
                nextState = nextResult.State;
            }
        }

        return AppResponse.SuccessResponse(invoicesList);
    }
    
    private async Task<SoldInvoiceResponseModel?> GetSoldInvoiceFromService(string token,
                                                                     string from, string to,
                                                                     string? state = null)
    {
        var request = new RestRequest("/query/invoices/sold", Method.Get);
        request.AddHeader("Cookie", setting.Cookie);
        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddQueryParameter("sort", "tdlap:desc,khmshdon:asc,shdon:desc");
        request.AddQueryParameter("size", 50);
        request.AddQueryParameter("search", $"tdlap=ge={from}T00:00:00;tdlap=le={to}T23:59:59");

        if (state is not null)
        {
            request.AddQueryParameter("state", state);
        }
        var response = await restClient.ExecuteAsync<SoldInvoiceResponseModel>(request);
        if (response.IsSuccessful)
        {
            Console.WriteLine($"Successfully retrieved  invoice from {from} to {to}");
            return response.Data;

        }
        var json = response.Content;
        var data = JsonConvert.DeserializeObject<SoldInvoiceResponseModel>(json!);
        Console.WriteLine(response.ErrorMessage);
        return data;
    }
    
    

    #endregion

    #region PURCHASE INVOICE METHODS

    public async Task<AppResponse> GetPurchaseInvoiceListInRange(string token, string from, string to)
    {
        try
        {
            logger.LogInformation("Starting Get Invoice List at {time}", DateTime.Now.ToLocalTime());
            int[] types = [5, 6, 8];
            List<InvoiceModel> invoicesList = [];
            var fromValue = DateTime.ParseExact(from, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None);
            var toValue = DateTime.ParseExact(to, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None);

            if (fromValue > toValue) throw new InvalidDataException("[From date] can not be greater than [To date]");

            var dateRanges = CommonUtil.SplitDateRange(fromValue, toValue);

            foreach (var type in types)
            {
                var endpoint = type switch
                {
                    8 => "/sco-query/invoices/purchase",
                    _ => "/query/invoices/purchase"
                };
                var displayType = type switch
                {
                    5 => "Hóa đơn được cấp mã",
                    6 => "Hóa đơn không cấp mã",
                    8 => "Hóa đơn từ máy tính tính tiền",
                    _ => string.Empty
                };
                foreach (var dateRange in dateRanges)
                {
                    var pageCount = 1;
                    await Task.Delay(800);
                    var result = await GetPurchaseInvoiceFromService(token, endpoint,
                                                        dateRange.GetFromDateString(),
                                                        dateRange.GetToDateString(), type);
                    await hubContext.Clients.All.SendAsync(HubName.PurchaseInvoice,
                                                           $"Tải thông tin {displayType} - Từ ngày: {dateRange.GetFromDateString()} đến ngày {dateRange.GetToDateString()}\n Trang: {pageCount}");
                    Console.WriteLine(
                        $"Get invoice type {type} of page {pageCount} - from {dateRange.GetFromDateString()} to {dateRange.GetToDateString()}");
                    if (result == null) continue;
                    invoicesList.AddRange(result.Datas);
                    if (result.State == null) continue;
                    var nextState = result.State;
                    while (true)
                    {
                        pageCount++;
                        var nextResult = await GetPurchaseInvoiceFromService(token, endpoint,
                                                                dateRange.GetFromDateString(),
                                                                dateRange.GetToDateString(),
                                                                type, nextState);
                        await hubContext.Clients.All.SendAsync(HubName.PurchaseInvoice,
                                                               $"Tải thông tin {displayType} - Từ ngày: {dateRange.GetFromDateString()} đến ngày {dateRange.GetToDateString()}\n Trang: {pageCount}");
                        Console.WriteLine(
                            $"Get invoice type {type} of page {pageCount} - from {dateRange.GetFromDateString()} to {dateRange.GetToDateString()}");
                        if (nextResult == null) break;
                        invoicesList.AddRange(nextResult.Datas);
                        if (nextResult.State == null) break;
                        nextState = nextResult.State;
                    }
                }
            }

            logger.LogInformation("Finished getting Invoice List at: {time}", DateTime.Now.ToLocalTime());
            return AppResponse.SuccessResponse(invoicesList.Select(x => x.ToDisplayModel()).ToList());
        }
        catch (Exception e)
        {
            logger.LogWarning("Interupted with error [{err}] at {time}", e.Message, DateTime.Now.ToLocalTime());
            return AppResponse.Error(e.Message);
        }
    }

    /// <summary>
    /// Get the list of purchase invoice in date range that limited by the external service. The invoices in the list has no goods detail
    /// </summary>
    /// <param name="token"></param>
    /// <param name="endpoint"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="type"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task<InvoiceResponseModel?> GetPurchaseInvoiceFromService(string token, string endpoint,
                                                               string from, string to,
                                                               int type, string? state = null)
    {
        var request = new RestRequest(endpoint, Method.Get);
        request.AddHeader("Cookie", setting.Cookie);
        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddQueryParameter("sort", "tdlap:desc,khmshdon:asc,shdon:desc");
        request.AddQueryParameter("size", 50);
        request.AddQueryParameter("search", $"tdlap=ge={from}T00:00:00;tdlap=le={to}T23:59:59;ttxly=={type}");

        if (state is not null)
        {
            request.AddQueryParameter("state", state);
        }

        var response = await restClient.ExecuteAsync<InvoiceResponseModel>(request);
        if (!response.IsSuccessful) throw new Exception($"Error: {response.StatusCode}");
        return response.IsSuccessful ? response.Data : null;
    }

    public async Task<AppResponse> GetInvoiceDetail(string token, InvoiceDisplayDto invoice)
    {
        var endpoint = invoice.InvoiceTypeNumber switch
        {
            8 => "/sco-query/invoices/detail",
            _ => "/query/invoices/detail"
        };
        var request = new RestRequest(endpoint, Method.Get);
        request.AddHeader("Cookie", setting.Cookie);
        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddQueryParameter("nbmst", invoice.SellerTaxCode);
        request.AddQueryParameter("khhdon", invoice.InvoiceNotation);
        request.AddQueryParameter("shdon", invoice.InvoiceNumber);
        request.AddQueryParameter("khmshdon", invoice.InvoiceGroupNotation.ToString());

        Console.WriteLine(
            $"Extracting {invoice.InvoiceNumber} - {invoice.SellerTaxCode} " +
            $"- {invoice.CreationDate:dd/MM/yyyy} " +
            $"- {invoice.SellerName}");
        Console.WriteLine($"Invoice status: {invoice.StatusNumber} - {invoice.Status}");
        Console.WriteLine($"Invoice type: {invoice.InvoiceType}");

        await Task.Delay(800); //delay before each call to avoid rejection

        var response = await restClient.ExecuteAsync<InvoiceDetailModel>(request);

        var statusCode = response.StatusCode;
        var retryCount = 0;
        const int delay = 20;
        while (statusCode == HttpStatusCode.TooManyRequests)
        {
            if (retryCount > 5) break;
            Console.WriteLine($"Too many requests. Retrying after {delay} seconds...");
            await hubContext.Clients.All.SendAsync(
                "429", $"Too many requests. Retry {retryCount + 1}/5 after {delay} seconds...");
            await Task.Delay(delay * 1000);
            response = await restClient.ExecuteAsync<InvoiceDetailModel>(request);
            statusCode = response.StatusCode;
            retryCount++;
            Console.WriteLine($"Retry {retryCount} completed");
            
        }

        if (retryCount > 5)
        {
            return new AppResponse
            {
                Code = "429",
                Success = false,
                Message = "429 - Too many request",
                Data = null
            };
        }
        
        if (response.StatusCode != HttpStatusCode.OK)
        {
            logger.LogWarning("Something wrong with the response {}", response.StatusCode);
            return new AppResponse
            {
                Success = false,
                Message = $"{response.StatusCode.ToString()} - {response.Content}",
                Data = $"Failed to retrieve invoice [{invoice.InvoiceNumber}] of [{invoice.SellerName}]"
            };
        }

        if (response is { Content: not null, Data: null })
        {
            return new AppResponse
            {
                Success = true,
                Message = "99 - auto-deserialize failed. Invoice object will be store as string and attempted to be deserialized using JSON converter",
                Data = response.Content,
            };
        }

        //Console.WriteLine($"{response.Content} successfully retrieved");
        return AppResponse.SuccessResponse(response.Data!);
    }
    #endregion
}