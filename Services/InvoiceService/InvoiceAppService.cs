using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Spire.Xls;
using WebApp.Mongo.DeserializedModel;
using WebApp.Mongo.DocumentModel;
using WebApp.Mongo.Mapper;
using WebApp.Mongo.MongoRepositories;
using WebApp.Payloads;
using WebApp.Services.InvoiceService.dto;
using WebApp.Services.RestService;
using WebApp.Services.RestService.Dto;
using WebApp.Services.RiskCompanyService;
using WebApp.Services.UserService;
using WebApp.SignalrConfig;
using WebApp.Utils;

namespace WebApp.Services.InvoiceService;

public interface IInvoiceAppService
{
    /// <summary>
    /// Find invoices by organization and query parameters
    /// </summary>
    /// <param name="taxCode"></param>
    /// <param name="invoiceParams"></param>
    /// <returns>The invoice list</returns>
    Task<AppResponse> GetInvoices(string taxCode, InvoiceParams invoiceParams);

    /// <summary>
    /// Sync invoices from hoadondientu.gdt.gov.vn
    /// </summary>
    /// <param name="token">The access token from hoadondientu.gdt.gov.vn</param>
    /// <param name="from">Start date</param>
    /// <param name="to">End date</param>
    /// <returns>Success result if all invoices were synced</returns>
    Task<AppResponse> SyncInvoices(string token, string from, string to);


    /// <summary>
    /// Export invoice list to excel file
    /// </summary>
    /// <param name="taxCode">Organization taxcode</param>
    /// <param name="from">Start date</param>
    /// <param name="to">End date</param>
    /// <returns>The byte array of created excel file to download</returns>
    Task<byte[]> ExportExcel(string taxCode, string from, string to);

    /// <summary>
    /// Recheck the saved invoices in the database and attempt to update their status if any change.
    /// </summary>
    /// <param name="token">The access token from hoadondientu.gdt.gov.vn</param>
    /// <param name="from">Start date</param>
    /// <param name="to">End date</param>
    /// <returns>The result of checking process.</returns>
    Task<AppResponse> RecheckInvoiceStatus(string token, string from, string to);

    Task<AppResponse> FindOne(string taxCode, string id);
}

public class InvoiceAppService(IInvoiceMongoRepository mongoInvoice,
                               IRestAppService restService,
                               ILogger<InvoiceAppService> logger,
                               IRiskCompanyAppService riskService,
                               IHubContext<AppHub> hub,
                               IUserManager userManager) : AppServiceBase(userManager), IInvoiceAppService
{
    public async Task<AppResponse> GetInvoices(string taxCode, InvoiceParams invoiceParams)
    {
        var invoiceList = await mongoInvoice.FindInvoices(taxCode: taxCode,
                                                          page: invoiceParams.Page!.Value,
                                                          size: invoiceParams.Size!.Value,
                                                          from: invoiceParams.From, to: invoiceParams.To,
                                                          seller: invoiceParams.SellerKeyword,
                                                          invNo: invoiceParams.InvoiceNumber,
                                                          invoiceType: (int?)invoiceParams.InvoiceType,
                                                          invoiceStatus: (int?)invoiceParams.Status,
                                                          risk: invoiceParams.Risk);

        return new AppResponse
        {
            Data = invoiceList.Data.Select(inv => inv.ToDisplayModel()).ToList(),
            Message = "Ok",
            TotalCount = invoiceList.Total,
            PageNumber = invoiceParams.Page,
            PageSize = invoiceParams.Size,
            Success = true,
            PageCount = invoiceList.PageCount
        };
    }

    public async Task<AppResponse> RecheckInvoiceStatus(string token, string from, string to)
    {
        var resultFromRest = await restService.GetInvoiceListAsync(token, from, to);
        var total = 0L;
        List<InvoiceDisplayDto> updateList = [];
        if (resultFromRest is { Success: true, Data: List<InvoiceDisplayDto> invoiceList })
        {
            foreach (var inv in invoiceList)
            {
                var result = await mongoInvoice.UpdateInvoiceStatus(inv.Id, inv.StatusNumber!.Value);
                if (result <= 0) continue;
                total += result;
                updateList.Add(inv);
            }
        }

        return new AppResponse
        {
            Message = total > 0 ? $"{total:N0} invoices updated." : "Found no invoice need to be updated.",
            Data = updateList,
        };
    }

    public async Task<AppResponse> SyncInvoices(string token, string from, string to)
    {
        logger.LogInformation("Sync Invoices from {from} to {to} at {time}", from, to, DateTime.Now.ToLocalTime());
        var result = await restService.GetInvoiceListAsync(token, from, to);

        if (result is not { Success: true, Data: not null })
        {
            logger.LogWarning("Invoice not found. {message}", result.Message);
            return AppResponse.Error("Invoice not found");
        }

        var invoiceList = (List<InvoiceDisplayDto>)result.Data;

        if (invoiceList.Count == 0)
        {
            await hub.Clients.All.SendAsync("RetrieveList", "No new invoices found");
            return AppResponse.SuccessResponse("No new invoices found");
        }

        var buyerTaxId = invoiceList.First().BuyerTaxCode;
        List<InvoiceDetailModel> invoicesToSave = [];
        List<string> unDeserializedInvoices = [];
        var countAdd = 1;
        var existedInvoices =
            await mongoInvoice.GetExistingInvoiceIdsAsync(invoiceList.Select(inv => inv.Id).ToList(), buyerTaxId);
        logger.LogInformation(
            "Found {existed}/{total} Invoices already existed in collection, those record will be ignored",
            existedInvoices.Count, invoiceList.Count);

        var newInvoices = invoiceList.Where(invoice => !existedInvoices.Contains(invoice.Id)).ToList();
        logger.LogInformation("{count} Invoices will be retrieved and written.", newInvoices.Count);

        if (newInvoices.Count == 0)
            return new AppResponse { Success = true, Message = "Already synced, no new invoices found" };

        foreach (var invoice in newInvoices)
        {
            var invDetail = await restService.GetInvoiceDetail(token, invoice);

            //If code 419 is hit, write anything that has already been retrieved and stop
            if (invDetail.Code == "429")
            {
                logger.LogWarning("Server has reach rate limit. Writing {retrieved}/{total} invoices to database", 
                                  unDeserializedInvoices.Count + invoicesToSave.Count,
                                  invoiceList.Count);
                await hub.Clients.All.SendAsync("RetrieveList", "Some invoices could not be synced right now because the external server has hit rate limit.");
                await hub.Clients.All.SendAsync("RetrieveList", "Attempting to write the current retrieved invoices.");
                return await WriteInvoices(invoicesToSave, unDeserializedInvoices, newInvoices.Count);
            }

            if (invDetail is not { Success: true })
            {
                logger.LogWarning("Error: {message}", invDetail.Message);
                logger.LogInformation("Skipping...\n {data}", invoice.InvoiceNumber);
                await hub.Clients.All.SendAsync("RetrieveList",
                                                       $"Failed to save invoice {invoice.InvoiceNumber} of {invoice.SellerTaxCode}, created at: {invoice.CreationDate:dd/MM/yyyy}");
                continue;
            }

            if (invDetail is { Success: true, Data: InvoiceDetailModel invoiceToAdd })
            {
                invoiceToAdd.Risk = riskService.IsInvoiceRisk(invoiceToAdd.Nbmst);
                invoicesToSave.Add(invoiceToAdd);
                logger.LogInformation(
                    "{count}/{new} - Invoice {invNum} added to collection.",
                    countAdd, newInvoices.Count, invoiceToAdd.Shdon);
                var completed = decimal.Divide(countAdd, newInvoices.Count) * 100;
                await hub.Clients.All.SendAsync("RetrieveList",
                                                       $"Saving {countAdd}/{newInvoices.Count} - {completed:F2}% completed");
            }

            if (invDetail is { Success: true, Message: not null, Data: not null } && invDetail.Message.Contains("99"))
            {
                unDeserializedInvoices.Add((string)invDetail.Data);
                var completed = decimal.Divide(countAdd, newInvoices.Count) * 100;
                await hub.Clients.All.SendAsync("RetrieveList",
                                                       $"Saving {countAdd}/{newInvoices.Count} - {completed:F2}% completed");
            }

            Console.WriteLine($"Undeserializable count: {unDeserializedInvoices.Count}");
            countAdd++;
        }

        //save to mongodb
        //if (invoicesToSave.Count == 0) return AppResponse.ErrorResponse("Failed");

        return await WriteInvoices(invoicesToSave, unDeserializedInvoices, newInvoices.Count);
    }


    public async Task<byte[]> ExportExcel(string taxCode, string from, string to)
    {
        var invoiceList = await mongoInvoice.FindInvoices(taxCode: taxCode,
                                                          page: 1, size: 10_000,
                                                          from: from, to: to,
                                                          seller: null, invNo: null,
                                                          invoiceType: null, invoiceStatus: null, risk: null);
        var list = invoiceList.Data.Select(inv => inv.ToDisplayModel())
                              .ToList();

        return GenerateExcelFile(list, from, to);
    }

    private async Task<AppResponse> WriteInvoices(List<InvoiceDetailModel> invoicesToSave,
                                                  List<string> unDeserializedInvoices, int total)
    {
        var totalSync = invoicesToSave.Count + unDeserializedInvoices.Count;
        try
        {
            var jsonOption = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            await hub.Clients.All.SendAsync("RetrieveList", "Writing to database...");
            var isInserted = await mongoInvoice.InsertInvoicesAsync(invoicesToSave
                                                                    .Select(i => i.DeserializeToBson(jsonOption))
                                                                    .ToList());
            var isInsered2 = true;
            if (unDeserializedInvoices.Count > 0)
            {
                logger.LogWarning("{} undeserializable invoices, trying to save...", unDeserializedInvoices.Count);
                isInsered2 = await mongoInvoice.InsertInvoicesAsync(unDeserializedInvoices
                                                                    .Select(i => i.DeserializeToBson(jsonOption))
                                                                    .ToList());
            }

            switch (isInserted)
            {
                case false when !isInsered2:
                    logger.LogWarning("Unable to save invoices. Operation terminated at {time}",
                                      DateTime.Now.ToLocalTime());
                    return AppResponse.Error("Nothing to insert.");
                case true:
                    logger.LogInformation("Finished syncing invoices at {time}", DateTime.Now.ToLocalTime());
                    break;
            }

            if (isInsered2)
            {
                logger.LogInformation("Finished syncing undeserializable invoices at {time}",
                                      DateTime.Now.ToLocalTime());
            }

            return new AppResponse
            {
                Success = true,
                Code = totalSync == total ? "200" : "207",
                Message = totalSync == total
                    ? $"All {total} invoices saved successfully"
                    : $"{totalSync}/{total} invoices saved successfully. Please try again later to sync the remaining invoices.",
                Data = new
                {
                    Total = total,
                    Success = totalSync,
                    Remaining = total - totalSync
                }
            };
        }
        catch (Exception e)
        {
            logger.LogError("Failed with Error: {mess}", e.Message);
            return AppResponse.Error500("Warning: saving invoices to database unsuccessfully due to an error occured.");
        }
    }

    public async Task<AppResponse> FindOne(string taxCode, string id)
    {
        var found = await mongoInvoice.FindOneAsync(x => x.Id == id && x.Nmmst == taxCode);
        return found != null 
            ? AppResponse.SuccessResponse(found.ToDisplayModel()) 
            : AppResponse.Error404("No invoice was found.");
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
            "Cảnh báo nhà cung cấp" //13
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

        var detailRow = 5;
        var summaryRow = 5;

        foreach (var inv in invoiceList.Where(inv => inv.GoodsDetail.Count != 0))
        {
            foreach (var item in inv.GoodsDetail)
            {
                #region Detail

                var unitPrice = item.UnitPrice;
                var preTaxPrice = item.PreTaxPrice;
                var vat = item.Tax;
                if (item.Name is not null
                    && (item.Name.Contains("chiết khấu", StringComparison.CurrentCultureIgnoreCase)
                        || item.Name.Contains("giảm giá", StringComparison.CurrentCultureIgnoreCase)))
                {
                    unitPrice = -unitPrice;
                    preTaxPrice = -preTaxPrice;
                    vat = -vat;
                }

                sheetDetail.Range[detailRow, 1].Value2 = inv.InvoiceNumber;
                sheetDetail.Range[detailRow, 2].Value2 = inv.InvoiceNotation;
                sheetDetail.Range[detailRow, 3].Text = inv.SellerTaxCode;
                sheetDetail.Range[detailRow, 4].Value2 = inv.SellerName;
                sheetDetail.Range[detailRow, 5].Value2 = item.Name;
                sheetDetail.Range[detailRow, 6].Value2 = item.UnitCount;

                sheetDetail.Range[detailRow, 7].Value2 = unitPrice;
                sheetDetail.Range[detailRow, 7].NumberFormat = "#,##0";

                sheetDetail.Range[detailRow, 8].Value2 = preTaxPrice;
                sheetDetail.Range[detailRow, 8].NumberFormat = "#,##0";

                sheetDetail.Range[detailRow, 9].Value2 = item.Rate;
                sheetDetail.Range[detailRow, 9].NumberFormat = "0.0%";

                sheetDetail.Range[detailRow, 10].Value2 = item.Discount;

                sheetDetail.Range[detailRow, 11].Value2 = vat;
                sheetDetail.Range[detailRow, 11].NumberFormat = "#,##0";
                sheetDetail.Range[detailRow, 12].Value2 = inv.CreationDate?.ToLocalTime();
                sheetDetail.Range[detailRow, 12].Style.NumberFormat = "dd/mm/yyyy";

                sheetDetail.Range[detailRow, 13].Value2 = inv.SigningDate?.ToLocalTime();
                sheetDetail.Range[detailRow, 13].Style.NumberFormat = "dd/mm/yyyy";

                sheetDetail.Range[detailRow, 14].Value2 = inv.IssueDate?.ToLocalTime();
                sheetDetail.Range[detailRow, 13].Style.NumberFormat = "dd/mm/yyyy";

                sheetDetail.Range[detailRow, 15].Value2 = inv.Status;
                sheetDetail.Range[detailRow, 16].Value2 = inv.InvoiceType;

                #endregion

                detailRow++;
            }

            #region Summary

            sheetSummary.Range[summaryRow, 1].Value2 = inv.InvoiceNumber;
            sheetSummary.Range[summaryRow, 2].Value2 = inv.InvoiceNotation;
            sheetSummary.Range[summaryRow, 3].Text = inv.SellerTaxCode;
            sheetSummary.Range[summaryRow, 4].Value2 = inv.SellerName;
            sheetSummary.Range[summaryRow, 5].Value2 = inv.CreationDate?.ToLocalTime();
            sheetSummary.Range[summaryRow, 5].Style.NumberFormat = "dd/mm/yyyy";
            sheetSummary.Range[summaryRow, 6].Value2 = inv.SigningDate?.ToLocalTime();
            sheetSummary.Range[summaryRow, 6].Style.NumberFormat = "dd/mm/yyyy";
            sheetSummary.Range[summaryRow, 7].Value2 = inv.IssueDate?.ToLocalTime();
            sheetSummary.Range[summaryRow, 7].Style.NumberFormat = "dd/mm/yyyy";
            sheetSummary.Range[summaryRow, 8].Value2 = inv.TotalPrice;
            sheetSummary.Range[summaryRow, 8].NumberFormat = "#,##0";

            sheetSummary.Range[summaryRow, 9].Value2 = inv.Vat;
            sheetSummary.Range[summaryRow, 9].NumberFormat = "#,##0";

            sheetSummary.Range[summaryRow, 10].Value2 = inv.TotalPriceVat;
            sheetSummary.Range[summaryRow, 10].NumberFormat = "#,##0";

            sheetSummary.Range[summaryRow, 11].Value2 = inv.Status;
            sheetSummary.Range[summaryRow, 12].Value2 = inv.InvoiceType;
            sheetSummary.Range[summaryRow, 13].Value2 = inv.Risk is null or false ? "OK" : "Rủi ro";

            #endregion

            summaryRow++;
        }

        sheetDetail.Range[4, 1, detailRow - 1, 3].AutoFitColumns();
        sheetDetail.Range[4, 6, detailRow - 1, 16].AutoFitColumns();

        sheetSummary.Range[4, 1, summaryRow - 1, 3].AutoFitColumns();
        sheetSummary.Range[4, 5, summaryRow - 1, 13].AutoFitColumns();

        #region Formula and filter

        sheetSummary.AutoFilters.Range = sheetSummary.Range[$"A{titleRow}:X{detailRow - 1}"];
        sheetDetail.AutoFilters.Range = sheetDetail.Range[$"A{titleRow}:X{summaryRow - 1}"];

        for (var i = 8; i <= 10; i++)
        {
            sheetSummary.Range[titleRow - 1, i].FormulaR1C1 = $"=SUBTOTAL(9,R5C{i}:R{summaryRow - 1}C{i})";
            sheetSummary.Range[titleRow - 1, i].NumberFormat = "#,##0";
            sheetSummary.Range[titleRow - 1, i].Style.Font.IsBold = true;
        }

        for (var i = 7; i <= 11; i++)
        {
            sheetDetail.Range[titleRow - 1, i].FormulaR1C1 = $"=SUBTOTAL(9,R5C{i}:R{detailRow - 1}C{i})";
            sheetDetail.Range[titleRow - 1, i].NumberFormat = "#,##0";
            sheetDetail.Range[titleRow - 1, i].Style.Font.IsBold = true;
        }

        #endregion

        foreach (var cell in sheetDetail.Range[4, 1, detailRow - 1, 16])
        {
            cell.BorderAround(LineStyleType.Thin);
        }

        foreach (var cell in sheetSummary.Range[4, 1, summaryRow - 1, 13])
        {
            cell.BorderAround(LineStyleType.Thin);
        }

        using var stream = new MemoryStream();
        workbook.SaveToStream(stream, FileFormat.Version2016);
        return stream.ToArray();
    }
}