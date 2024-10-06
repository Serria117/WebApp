using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Spire.Xls;
using WebApp.Mongo.DeserializedModel;
using WebApp.Mongo.DocumentModel;
using WebApp.Mongo.Mapper;
using WebApp.Mongo.MongoRepositories;
using WebApp.Payloads;
using WebApp.Services.InvoiceService.dto;
using WebApp.Services.RestService;
using WebApp.Services.RiskCompanyService;
using WebApp.Services.UserService;
using WebApp.SignalrConfig;
using WebApp.Utils;

namespace WebApp.Services.InvoiceService;

public interface IInvoiceAppService
{
    Task<AppResponse> GetInvoices(string taxCode, InvoiceParams invoiceParams);
    Task<AppResponse> SyncInvoices(string token, string from, string to);
    Task<byte[]> ExportExcel(string taxCode, string from, string to);
}

public class InvoiceAppService(IInvoiceMongoRepository mongoInvoice,
                               IRestAppService restService,
                               ILogger<InvoiceAppService> logger,
                               IRiskCompanyAppService riskService,
                               IHubContext<AppHub> hubContext,
                               IUserManager userManager) : AppServiceBase(userManager), IInvoiceAppService
{
    public async Task<AppResponse> GetInvoices(string taxCode, InvoiceParams param)
    {
        var invoiceList = await mongoInvoice.FindInvoices(taxCode: taxCode,
                                                          page: param.Page!.Value, size: param.Size!.Value,
                                                          from: param.From, to: param.To,
                                                          seller: param.SellerKeyword, invNo: param.InvoiceNumber, 
                                                          invoiceType: (int?) param.InvoiceType, 
                                                          invoiceStatus: (int?) param.Status, 
                                                          risk: param.Risk);

        return new AppResponse
        {
            Data = ((List<InvoiceDetailDoc>)invoiceList.Data)
                   .Select(inv => inv.ToDisplayModel())
                   .ToList(),
            Message = "Ok",
            TotalCount = invoiceList.Total,
            PageNumber = param.Page,
            PageSize = param.Size,
            Success = true,
            PageCount = invoiceList.PageCount
        };
    }

    public async Task<AppResponse> SyncInvoices(string token, string from, string to)
    {
        logger.LogInformation("Sync Invoices from {from} to {to} at {time}", from, to, DateTime.Now.ToLocalTime());
        var result = await restService.GetInvoiceListAsync(token, from, to);

        if (result is not { Success: true, Data: not null }) return AppResponse.ErrorResponse("Failed");

        var invoiceList = (List<InvoiceDisplayDto>)result.Data;
        var buyerTaxId = invoiceList.First().BuyerTaxCode;
        List<InvoiceDetailModel> invoicesToSave = [];
        var countAdd = 1;
        var existedInvoices =
            await mongoInvoice.GetExistingInvoiceIdsAsync(invoiceList.Select(inv => inv.Id).ToList(), buyerTaxId);
        Console.WriteLine(
            $"Found {existedInvoices.Count}/{invoiceList.Count} Invoices already existed in collection, those record will be ignored");

        var newInvoices = invoiceList.Where(invoice => !existedInvoices.Contains(invoice.Id)).ToList();
        Console.WriteLine($"{newInvoices.Count} Invoices will be retrieved and written.");
        if (newInvoices.Count == 0)
            return new AppResponse { Success = true, Message = "Already synced, no new invoices found" };
        foreach (var invoice in newInvoices)
        {
            var invDetail = await restService.GetInvoiceDetail(token, invoice);

            if (invDetail is not { Success: true, Data: InvoiceDetailModel })
            {
                logger.LogWarning($"Error: {invDetail.Message}");
                logger.LogInformation("Skipping...\n {data}", invDetail.Data);
                await hubContext.Clients.All.SendAsync("RetrieveList",
                                                       $"Failed to save invoice {invoice.InvoiceNumber} of {invoice.SellerTaxCode}, created at: {invoice.CreationDate:dd/MM/yyyy}");
                continue;
            }

            if (invDetail.Data is InvoiceDetailModel invoiceToAdd)
            {
                invoiceToAdd.Risk = riskService.IsInvoiceRisk(invoiceToAdd.Nbmst);
                invoicesToSave.Add(invoiceToAdd);
                Console.WriteLine(
                    $"{countAdd}/{newInvoices.Count} - Invoice {invoiceToAdd.Shdon} added to collection.");
                var completed = decimal.Divide(countAdd, newInvoices.Count) * 100;
                await hubContext.Clients.All.SendAsync("RetrieveList",
                                                       $"Saving {countAdd}/{newInvoices.Count} - {completed:F2}% completed");
            }
            countAdd++;
        }

        if (invoicesToSave.Count == 0) return AppResponse.ErrorResponse("Failed");
        try
        {
            var jsonOption = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            await hubContext.Clients.All.SendAsync("writeInvoices", "Writing to database...");
            var isInserted = await mongoInvoice.InsertInvoicesAsync(invoicesToSave
                                                                    .Select(i => i.DeserializeToBson(jsonOption))
                                                                    .ToList());
            if (isInserted)
            {
                logger.LogInformation("Finished syncing invoices at {time}", DateTime.Now.ToLocalTime());
                return new AppResponse { Message = $"Successfully inserted {invoicesToSave.Count} invoices." };
            }

            logger.LogWarning("Unable to save invoices. Operation terminated at {time}",
                              DateTime.Now.ToLocalTime());
            return AppResponse.ErrorResponse("Nothing to insert.");
        }
        catch (Exception e)
        {
            logger.LogError("Failed with Error: {mess}", e.Message);
            return AppResponse.ErrorResponse("Failed");
        }

    }

    public async Task<byte[]> ExportExcel(string taxCode, string from, string to)
    {
        var invoiceList = await mongoInvoice.FindInvoices(taxCode: taxCode,
                                                          page: 1, size: 10_000,
                                                          from: from, to: to,
                                                          seller: null, invNo: null, 
                                                          invoiceType: null, invoiceStatus: null, risk: null);
        var list = ((List<InvoiceDetailDoc>)invoiceList.Data).Select(inv => inv.ToDisplayModel())
                                                             .ToList();

        return GenerateExcelFile(list, from, to);
    }


    private static byte[] GenerateExcelFile(List<InvoiceDisplayDto> invoiceList, string from, string to)
    {
        var workbook = new Workbook
        {
            Version = ExcelVersion.Version2016
        };
        var sheetSummary = workbook.Worksheets[0];
        sheetSummary.Name = "Summary";
        var sheetDetail = workbook.Worksheets[1];
        sheetDetail.Name = "Details";

        sheetDetail.Range[1, 1].Value = $"{invoiceList[0].BuyerName.ToUpper()} - {invoiceList[0].BuyerTaxCode}";
        sheetDetail.Range[2, 1].Value = $"Chi tiết hóa đơn đầu vào - Từ {from} đến {to}";
        sheetDetail.Range[1, 1, 2, 1].Style.Font.IsBold = true;

        sheetSummary.Range[1, 1].Value = $"{invoiceList[0].BuyerName.ToUpper()} - {invoiceList[0].BuyerTaxCode}";
        sheetSummary.Range[2, 1].Value = $"Danh sách hóa đơn đầu vào - Từ {from} đến {to}";
        sheetSummary.Range[1, 1, 2, 1].Style.Font.IsBold = true;
        const int titleRow = 4;
        List<string> detailTitles =
        [
            "Số hóa đơn", //1
            "Ký hiệu", //2
            "Mã số thuế", //3
            "Tên người bán", //4
            "Hàng hóa/dịch vụ", //5
            "Đơn vị tính", //6
            "Đơn giá", //7
            "Giá mua trước thuế", //
            "Thuế suất",
            "Chiết khấu", //
            "Thuế GTGT", //
            "Ngày lập", //
            "Ngày ký", //
            "Ngày cấp mã", //
            "Trạng thái", //
            "Loại hóa đơn"
        ];

        List<string> summaryTitles =
        [
            "Số hóa đơn", //1
            "Ký hiệu", //2
            "Mã số thuế người bán", //3
            "Tên người bán", //4
            "Ngày lập", //5
            "Ngày ký", //6
            "Ngày cấp mã", //7
            "Giá mua trước thuế", //8
            "Thuế GTGT", //9
            "Thành tiền", //10
            "Trạng thái", //11
            "Loại hóa đơn", //12
            "Cảnh báo nhà cung cấp"
        ];

        for (var i = 0; i < detailTitles.Count; i++)
        {
            sheetDetail.Range[titleRow, i + 1].Value = detailTitles[i];
            sheetDetail.Range[titleRow, i + 1].BorderAround(LineStyleType.Thin);
        }

        for (var i = 0; i < summaryTitles.Count; i++)
        {
            sheetSummary.Range[titleRow, i + 1].Value = summaryTitles[i];
            sheetSummary.Range[titleRow, i + 1].BorderAround(LineStyleType.Thin);
        }

        sheetDetail.Range[titleRow, 1, titleRow, detailTitles.Count].Style.Font.IsBold = true;
        sheetSummary.Range[titleRow, 1, titleRow, summaryTitles.Count].Style.Font.IsBold = true;

        var startRow = 5;

        foreach (var inv in invoiceList.Where(inv => inv.GoodsDetail.Count != 0))
        {
            foreach (var item in inv.GoodsDetail)
            {
                #region Detail

                sheetDetail.Range[startRow, 1].Value2 = inv.InvoiceNumber;
                sheetDetail.Range[startRow, 2].Value2 = inv.InvoiceNotation;
                sheetDetail.Range[startRow, 3].Text = inv.SellerTaxCode;
                sheetDetail.Range[startRow, 4].Value2 = inv.SellerName;
                sheetDetail.Range[startRow, 5].Value2 = item.Name;
                sheetDetail.Range[startRow, 6].Value2 = item.UnitCount;

                sheetDetail.Range[startRow, 7].Value2 = item.UnitPrice;
                sheetDetail.Range[startRow, 7].NumberFormat = "#,##0";

                sheetDetail.Range[startRow, 8].Value2 = item.PreTaxPrice;
                sheetDetail.Range[startRow, 8].NumberFormat = "#,##0";

                sheetDetail.Range[startRow, 9].Value2 = item.Rate;
                sheetDetail.Range[startRow, 9].NumberFormat = "0.0%";

                sheetDetail.Range[startRow, 10].Value2 = item.Discount;
                sheetDetail.Range[startRow, 10].NumberFormat = "0.0%";

                sheetDetail.Range[startRow, 11].Value2 = item.Tax;
                sheetDetail.Range[startRow, 11].NumberFormat = "#,##0";
                sheetDetail.Range[startRow, 12].Value2 = inv.CreationDate?.ToLocalTime();
                sheetDetail.Range[startRow, 13].Value2 = inv.SigningDate?.ToLocalTime();
                sheetDetail.Range[startRow, 14].Value2 = inv.IssueDate?.ToLocalTime();
                sheetDetail.Range[startRow, 15].Value2 = inv.Status;
                sheetDetail.Range[startRow, 16].Value2 = inv.InvoiceType;

                #endregion

                #region Summary

                sheetSummary.Range[startRow, 1].Value2 = inv.InvoiceNumber;
                sheetSummary.Range[startRow, 2].Value2 = inv.InvoiceNotation;
                sheetSummary.Range[startRow, 3].Text = inv.SellerTaxCode;
                sheetSummary.Range[startRow, 4].Value2 = inv.SellerName;
                sheetSummary.Range[startRow, 5].Value2 = inv.CreationDate?.ToLocalTime();
                sheetSummary.Range[startRow, 6].Value2 = inv.SigningDate?.ToLocalTime();
                sheetSummary.Range[startRow, 7].Value2 = inv.IssueDate?.ToLocalTime();
                sheetSummary.Range[startRow, 8].Value2 = inv.TotalPrice;
                sheetSummary.Range[startRow, 8].NumberFormat = "#,##0";

                sheetSummary.Range[startRow, 9].Value2 = inv.Vat;
                sheetSummary.Range[startRow, 9].NumberFormat = "#,##0";

                sheetSummary.Range[startRow, 10].Value2 = inv.TotalPriceVat;
                sheetSummary.Range[startRow, 10].NumberFormat = "#,##0";

                sheetSummary.Range[startRow, 11].Value2 = inv.Status;
                sheetSummary.Range[startRow, 12].Value2 = inv.InvoiceType;
                sheetSummary.Range[startRow, 13].Value2 = inv.Risk is null or false ? "OK" : "Rủi ro";

                #endregion
            }

            startRow++;
        }

        sheetDetail.Range[4, 1, startRow - 1, 3].AutoFitColumns();
        sheetDetail.Range[4, 6, startRow - 1, 16].AutoFitColumns();

        sheetSummary.Range[4, 1, startRow - 1, 3].AutoFitColumns();
        sheetSummary.Range[4, 5, startRow - 1, 12].AutoFitColumns();

        #region Formula and filter

        sheetSummary.AutoFilters.Range = sheetSummary.Range[$"A{titleRow}:X{startRow - 1}"];
        sheetDetail.AutoFilters.Range = sheetDetail.Range[$"A{titleRow}:X{startRow - 1}"];

        for (var i = 8; i <= 10; i++)
        {
            sheetSummary.Range[titleRow - 1, i].FormulaR1C1 = $"=SUBTOTAL(9,R5C{i}:R{startRow - 1}C{i})";
            sheetSummary.Range[titleRow - 1, i].NumberFormat = "#,##0";
            sheetSummary.Range[titleRow - 1, i].Style.Font.IsBold = true;
        }

        for (var i = 7; i <= 11; i++)
        {
            sheetDetail.Range[titleRow - 1, i].FormulaR1C1 = $"=SUBTOTAL(9,R5C{i}:R{startRow - 1}C{i})";
            sheetDetail.Range[titleRow - 1, i].NumberFormat = "#,##0";
            sheetDetail.Range[titleRow - 1, i].Style.Font.IsBold = true;
        }

        #endregion

        foreach (var cell in sheetDetail.Range[4, 1, startRow - 1, 16])
        {
            cell.BorderAround(LineStyleType.Thin);
        }

        foreach (var cell in sheetSummary.Range[4, 1, startRow - 1, 12])
        {
            cell.BorderAround(LineStyleType.Thin);
        }

        using var stream = new MemoryStream();
        workbook.SaveToStream(stream, FileFormat.Version2016);
        return stream.ToArray();
    }
}