using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using Newtonsoft.Json;
using RestSharp;
using WebApp.Mongo.DeserializedModel;
using WebApp.Payloads;
using WebApp.Services.InvoiceService.dto;
using WebApp.Services.RestService.Dto;
using WebApp.SignalrConfig;
using WebApp.Utils;

namespace WebApp.Services.RestService;

public interface IRestAppService
{
    Task<AppResponse> Authenticate(InvoiceLoginModel login);
    Task<CaptchaModel?> GetCapcha();
    Task<AppResponse> GetInvoiceListAsync(string token, string from, string to);
    Task<AppResponse> GetInvoiceDetail(string token, InvoiceDisplayDto invoice);
}

public class RestAppService(IRestClient restClient,
                            RestSharpSetting setting,
                            IMongoClient mongo,
                            ILogger<RestAppService> logger,
                            IHubContext<AppHub> hubContext) : IRestAppService
{
    public async Task<CaptchaModel?> GetCapcha()
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

    public async Task<AppResponse> GetInvoiceListAsync(string token, string from, string to)
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
                var enpoint = type switch
                {
                    8 => "/sco-query/invoices/purchase",
                    _ => "/query/invoices/purchase"
                };
                foreach (var dateRange in dateRanges)
                {
                    var pageCount = 1;
                    await Task.Delay(800);
                    var result = await GetInvoice(token, enpoint,
                                                  dateRange.GetFromDateString(),
                                                  dateRange.GetToDateString(), type);
                    await hubContext.Clients.All.SendAsync("RetrieveList",
                                                           $"Get invoice type {type} of page {pageCount} - from {dateRange.GetFromDateString()} to {dateRange.GetToDateString()}");
                    Console.WriteLine(
                        $"Get invoice type {type} of page {pageCount} - from {dateRange.GetFromDateString()} to {dateRange.GetToDateString()}");
                    if (result == null) continue;
                    invoicesList.AddRange(result.Datas);
                    if (result.State == null) continue;
                    var nextState = result.State;
                    while (true)
                    {
                        pageCount++;
                        var nextResult = await GetInvoice(token, enpoint,
                                                          dateRange.GetFromDateString(), dateRange.GetToDateString(),
                                                          type, nextState);
                        await hubContext.Clients.All.SendAsync("RetrieveList",
                                                               $"Get invoice type {type} of page {pageCount} - from {dateRange.GetFromDateString()} to {dateRange.GetToDateString()}");
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
            return AppResponse.ErrorResponse(e.Message);
        }
    }

    private async Task<InvoiceResponseModel?> GetInvoice(string token, string endpoint,
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
            $"Extracting {invoice.InvoiceNumber} - {invoice.SellerTaxCode} - {invoice.CreationDate:dd/MM/yyyy} - {invoice.SellerName}");
        Console.WriteLine($"Invoice status: {invoice.StatusNumber} - {invoice.Status}");
        Console.WriteLine($"Invoice type: {invoice.InvoiceType}");

        await Task.Delay(800); //delay before each call to avoid rejection

        var response = await restClient.ExecuteAsync<InvoiceDetailModel>(request);

        var statusCode = response.StatusCode;
        var retryCount = 0;
        const int delay = 20;
        while (statusCode == HttpStatusCode.TooManyRequests)
        {
            Console.WriteLine($"Too many requests. Retrying after {delay} seconds...");
            await hubContext.Clients.All.SendAsync(
                "429", $"Too many requests. Retry {retryCount + 1}/5 after {delay} seconds...");
            await Task.Delay(delay * 1000);
            response = await restClient.ExecuteAsync<InvoiceDetailModel>(request);
            statusCode = response.StatusCode;
            retryCount++;
            Console.WriteLine($"Retry {retryCount} completed");
            if (retryCount > 5) break;
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
                Message = "99 - auto-deserialize failed",
                Data = response.Content,
            };
        } 
        //Console.WriteLine($"{response.Content} successfully retrieved");
        return AppResponse.SuccessResponse(response.Data!);
    }
}