using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Spire.Xls;
using WebApp.Enums;
using WebApp.Mongo.DeserializedModel;
using WebApp.Mongo.DocumentModel;
using WebApp.Mongo.FilterBuilder;
using WebApp.Mongo.Mapper;
using WebApp.Mongo.MongoRepositories;
using WebApp.Payloads;
using WebApp.Services.InvoiceService.dto;
using WebApp.Services.NotificationService;
using WebApp.Services.RestService;
using WebApp.Services.RestService.Dto;
using WebApp.Services.RestService.Dto.SoldInvoice;
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
    Task<AppResponse> FindPurchaseInvoices(string taxCode, InvoiceRequestParam invoiceParams);

    /// <summary>
    /// Sync invoices from hoadondientu.gdt.gov.vn
    /// </summary>
    /// <param name="token">The access token from hoadondientu.gdt.gov.vn</param>
    /// <param name="from">Start date</param>
    /// <param name="to">End date</param>
    /// <returns>Success result if all invoices were synced</returns>
    Task<AppResponse> ExtractPurchaseInvoices(string token, string from, string to);


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
    Task<AppResponse> RecheckPurchaseInvoice(string token, string from, string to);

    Task<AppResponse> FindOne(string taxCode, string id);
    Task<AppResponse> ExtractSoldInvoice(SyncInvoiceRequest request);
    Task<AppResponse> FindSoldInvoices(string taxCode, InvoiceRequestParam invoiceParams);
}

public class InvoiceAppService(IInvoiceMongoRepository mongoPurchaseInvoice,
                               ISoldInvoiceMongoRepository mongoSoldInvoice,
                               IRestAppService restService,
                               ILogger<InvoiceAppService> logger,
                               IRiskCompanyAppService riskService,
                               IHubContext<AppHub> hub,
                               INotificationAppService notificationService,
                               IUserManager userManager) : AppServiceBase(userManager), IInvoiceAppService
{
    #region Sold Invoices

    public async Task<AppResponse> ExtractSoldInvoice(SyncInvoiceRequest request)
    {
        var result = await restService.GetSoldInvoiceListAsync(request.Token, request.From, request.To);
        var total = 0;
        var inserted = 0;
        if (result.Data is List<SoldInvoiceModel> invoices)
        {
            total = invoices.Count;
            var docs = invoices.Select(x => JsonConvert.SerializeObject(x).ToSoldInvoiceBson()).ToList();
            inserted = await mongoSoldInvoice.InsertInvoicesAsync(docs);
        }

        return new AppResponse
        {
            Success = true,
            Code = "200",
            Data = new
            {
                Total = total,
                Inserted = inserted,
            },
            Message = $"Inserted {inserted} of {total}",
        };
    }

    public async Task<AppResponse> FindSoldInvoices(string taxCode, InvoiceRequestParam invoiceParams)
    {
        invoiceParams.Valid();
        //filter by seller taxid
        var filter = InvoiceFilterBuilder.StartBuilder()
                                         .WithSeller(taxCode)
                                         .HasNameKeyword(invoiceParams.NameKeyword)
                                         .FromDate(invoiceParams.From)
                                         .ToDate(invoiceParams.To)
                                         .WithInvoiceNumber(invoiceParams.InvoiceNumber)
                                         .Build<SoldInvoiceDoc>();

        var result = await mongoSoldInvoice.FindInvoices(filter,
                                                         invoiceParams.Page!.Value,
                                                         invoiceParams.Size!.Value);

        return new AppResponse
        {
            Success = true,
            Data = result.Data.Select(x => x.ToDisplayModel()).ToList(),
            Code = "200",
            PageCount = result.PageCount,
            PageNumber = result.Page,
            PageSize = result.Size,
            TotalCount = result.Total,
            Message = "Ok"
        };
    }

    #endregion

    #region Purchase Invoice

    public async Task<AppResponse> FindPurchaseInvoices(string taxCode, InvoiceRequestParam invoiceParams)
    {
        invoiceParams.Valid();

        var filter = InvoiceFilterBuilder.StartBuilder()
                                         .FromDate(invoiceParams.From)
                                         .ToDate(invoiceParams.To)
                                         .WithBuyer(taxCode)
                                         .WithInvoiceNumber(invoiceParams.InvoiceNumber)
                                         .HasNameKeyword(invoiceParams.NameKeyword)
                                         .WithRisk(invoiceParams.Risk)
                                         .WithStatus(invoiceParams.Status)
                                         .WithType(invoiceParams.InvoiceType)
                                         .Build<InvoiceDetailDoc>();
        var invoiceList = await mongoPurchaseInvoice.FindInvoices(filter: filter,
                                                                  page: invoiceParams.Page!.Value,
                                                                  size: invoiceParams.Size!.Value);
        await notificationService.SendNotificationAsync(UserId!,
                                                        HubName.PurchaseInvoice,
                                                        $"Found {invoiceList.Total} invoice(s)");
        //await Task.Delay(1000);
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

    public async Task<AppResponse> RecheckPurchaseInvoice(string token, string from, string to)
    {
        var resultFromRest = await restService.GetPurchaseInvoiceListInRange(token, from, to);
        var total = 0L;
        List<InvoiceDisplayDto> updateList = [];
        if (resultFromRest is { Success: true, Data: List<InvoiceDisplayDto> invoiceList })
        {
            foreach (var inv in invoiceList)
            {
                var result = await mongoPurchaseInvoice.UpdateInvoiceStatus(inv.Id, inv.StatusNumber!.Value);
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

    public async Task<AppResponse> ExtractPurchaseInvoices(string token, string from, string to)
    {
        logger.LogInformation("Sync Invoices from {from} to {to} at {time}",
                              from, to, DateTime.Now.ToLocalTime());
        var result = await restService.GetPurchaseInvoiceListInRange(token, from, to);

        if (result is not { Success: true, Data: not null })
        {
            logger.LogWarning("Invoice not found. {message}", result.Message);
            return AppResponse.Error("Invoice not found");
        }

        var invoiceList = (List<InvoiceDisplayDto>)result.Data;

        if (invoiceList.Count == 0)
        {
            await notificationService.SendNotificationAsync(UserId, "RetrieveList", "No new invoices found");
            return AppResponse.SuccessResponse("No new invoices found");
        }

        var buyerTaxId = invoiceList.First().BuyerTaxCode;
        List<InvoiceDetailModel> invoicesToSave = [];
        List<string> unDeserializedInvoices = [];
        var countAdd = 1;
        var existedInvoices =
            await mongoPurchaseInvoice.GetExistingInvoiceIdsAsync(invoiceList.Select(inv => inv.Id).ToList(),
                                                                  buyerTaxId);
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
                /*await hub.Clients.All.SendAsync("RetrieveList",
                                                "Some invoices could not be synced right now because the external server has hit rate limit.");
                await hub.Clients.All.SendAsync("RetrieveList", "Attempting to write the current retrieved invoices.");*/

                await notificationService.SendNotificationAsync(UserId,
                                                                HubName.PurchaseInvoice,
                                                                "Some invoices could not be synced right now because the external server has hit rate limit.");
                return await WriteInvoices(invoicesToSave, unDeserializedInvoices, newInvoices.Count);
            }

            if (invDetail is not { Success: true })
            {
                logger.LogWarning("Error: {message}", invDetail.Message);
                logger.LogInformation("Skipping...\n {data}", invoice.InvoiceNumber);
                await notificationService.SendNotificationAsync(UserId,
                                                                HubName.PurchaseInvoice,
                                                                $"Failed to save invoice {invoice.InvoiceNumber} of {invoice.SellerTaxCode}, created at: {invoice.CreationDate:dd/MM/yyyy}");
                continue;
            }

            if (invDetail is { Success: true, Data: InvoiceDetailModel invoiceToAdd })
            {
                invoiceToAdd.Risk = riskService.IsInvoiceRisk(invoiceToAdd.Nbmst);
                invoicesToSave.Add(invoiceToAdd);
                logger.LogInformation(
                    "{count}/{new} - Invoice {invNum} added to collection.",
                    countAdd, newInvoices.Count, invoiceToAdd.Shdon
                );
                var completed = decimal.Divide(countAdd, newInvoices.Count) * 100;
                /*await hub.Clients.All.SendAsync("RetrieveList",
                                                $"Download: {countAdd}/{newInvoices.Count} - {completed:F2}% completed");*/
                await notificationService.SendNotificationAsync(UserId,
                                                                HubName.PurchaseInvoice,
                                                                $"Download: {countAdd}/{newInvoices.Count} - {completed:F2}% completed");
            }

            if (invDetail is { Success: true, Message: not null, Data: not null } && invDetail.Message.Contains("99"))
            {
                unDeserializedInvoices.Add((string)invDetail.Data);
                var completed = decimal.Divide(countAdd, newInvoices.Count) * 100;
                await notificationService.SendNotificationAsync(UserId,
                                                                HubName.PurchaseInvoice,
                                                                $"Download: {countAdd}/{newInvoices.Count} - {completed:F2}% completed");
            }

            Console.WriteLine($"Undeserializable count: {unDeserializedInvoices.Count}");
            countAdd++;
        }

        return await WriteInvoices(invoicesToSave, unDeserializedInvoices, newInvoices.Count);
    }

    #endregion

    public async Task<byte[]> ExportExcel(string taxCode, string from, string to)
    {
        var purchaseFilter = InvoiceFilterBuilder.StartBuilder()
                                                 .FromDate(from)
                                                 .ToDate(to)
                                                 .WithBuyer(taxCode)
                                                 .Build<InvoiceDetailDoc>();
        var purchaseResult = await mongoPurchaseInvoice.FindInvoices(filter: purchaseFilter,
                                                                     page: 1, size: int.MaxValue);
        var purchaseList = purchaseResult.Data.Select(inv => inv.ToDisplayModel())
                                         .ToList();
        logger.LogInformation("Number of purchase found: {}", purchaseList.Count);
        var soldFilter = InvoiceFilterBuilder.StartBuilder()
                                             .FromDate(from)
                                             .ToDate(to)
                                             .WithSeller(taxCode)
                                             .Build<SoldInvoiceDoc>();

        var soldResult = await mongoSoldInvoice.FindInvoices(filter: soldFilter,
                                                             page: 1, size: int.MaxValue);
        var soldList = soldResult.Data.Select(inv => inv.ToDisplayModel())
                                 .ToList();
        logger.LogInformation("Number of sold found: {}", soldList.Count);
        await notificationService.SendNotificationAsync(UserId,
                                                        HubName.InvoiceCount,
                                                        $"Đang kết xuất dữ liệu của {purchaseList.Count} hóa đơn đầu vào và {soldList.Count} hóa đơn đầu ra.");

        var file = GenerateExcelFile(purchaseList, soldList, from, to);
        await notificationService.SendNotificationAsync(UserId,
                                                        HubName.InvoiceCount,
                                                        "Finished.");
        return file;
    }

    public async Task<AppResponse> FindOne(string taxCode, string id)
    {
        var found = await mongoPurchaseInvoice.FindOneAsync(x => x.Id == id && x.Nmmst == taxCode);
        return found != null
            ? AppResponse.SuccessResponse(found.ToDisplayModel())
            : AppResponse.Error404("No invoice was found.");
    }

    #region Private method

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
            await notificationService.SendNotificationAsync(UserId, HubName.PurchaseInvoice, "Writing to database...");
            var isInserted = await mongoPurchaseInvoice.InsertInvoicesAsync(invoicesToSave
                                                                            .Select(
                                                                                i => i.ToPurchaseInvoiceDetailBson(
                                                                                    jsonOption))
                                                                            .ToList());
            var isInsered2 = true;
            if (unDeserializedInvoices.Count > 0)
            {
                logger.LogWarning("{} undeserializable invoices, trying to save...", unDeserializedInvoices.Count);
                isInsered2 = await mongoPurchaseInvoice.InsertInvoicesAsync(unDeserializedInvoices
                                                                            .Select(
                                                                                i => i.ToPurchaseInvoiceDetailBson(
                                                                                    jsonOption))
                                                                            .ToList());
            }

            switch (isInserted)
            {
                case false when !isInsered2:
                    logger.LogWarning("Unable to save invoices. Operation terminated at {time}",
                                      DateTime.Now.ToLocalTime());
                    return AppResponse.Error("Nothing to insert.");
                case true:
                    logger.LogInformation("Finished syncing invoices at {time}",
                                          DateTime.Now.ToLocalTime());
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


    private static byte[] GenerateExcelFile(List<InvoiceDisplayDto> purchaseList,
                                            List<InvoiceDisplayDto> soldList,
                                            string from, string to)
    {
        var workbook = new Workbook
        {
            Version = ExcelVersion.Version2016
        };
        var orgName = string.Empty + purchaseList[0].BuyerName.ToUpper();
        var orgTaxId = string.Empty + purchaseList[0].BuyerTaxCode.ToUpper();

        var shPurchaseSummary = workbook.Worksheets[1];
        shPurchaseSummary.Name = "Purchase_Summary";

        var shPurchaseDetail = workbook.Worksheets[2];
        shPurchaseDetail.Name = "Purchase_Details";

        var shSoldSummary = workbook.Worksheets[0];
        shSoldSummary.Name = "Sold_Summary";

        shPurchaseDetail.Range[1, 1].Value = $"{orgName} - {orgTaxId}";
        shPurchaseDetail.Range[2, 1].Value = $"Chi tiết hóa đơn đầu vào - Từ {from} đến {to}";
        shPurchaseDetail.Range[1, 1, 2, 1].Style.Font.IsBold = true;

        shPurchaseSummary.Range[1, 1].Value = $"{purchaseList[0].BuyerName.ToUpper()} - {purchaseList[0].BuyerTaxCode}";
        shPurchaseSummary.Range[2, 1].Value = $"Danh sách hóa đơn đầu vào - Từ {from} đến {to}";
        shPurchaseSummary.Range[1, 1, 2, 1].Style.Font.IsBold = true;

        shSoldSummary.Range[1, 1].Value = $"{orgName} - {orgTaxId}";
        shSoldSummary.Range[2, 1].Value = $"Chi tiết hóa đơn đầu ra - Từ {from} đến {to}";
        shSoldSummary.Range[1, 1, 2, 1].Style.Font.IsBold = true;

        const int titleRow = 4;

        List<string> soldSummaryTitles =
        [
            "Số hóa đơn", //1
            "Ký hiệu", //2
            "MST người mua", //3 
            "Tên người mua", //4
            "Ngày lập", //5
            "Ngày ký", //6
            "Ngày cấp mã", //7
            "Giá mua trước thuế", //8
            "Thuế GTGT", //9
            "Thành tiền", //10
            "Trạng thái", //11
        ];

        List<string> purchaseDetailTitles =
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

        List<string> purchaseSummaryTitles =
        [
            "Số hóa đơn", //1
            "Ký hiệu", //2
            "MST người bán", //3
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

        for (var i = 0; i < purchaseDetailTitles.Count; i++)
        {
            shPurchaseDetail.Range[titleRow, i + 1].Value = purchaseDetailTitles[i];
            shPurchaseDetail.Range[titleRow, i + 1].BorderAround(LineStyleType.Thin);
        }

        for (var i = 0; i < purchaseSummaryTitles.Count; i++)
        {
            shPurchaseSummary.Range[titleRow, i + 1].Value = purchaseSummaryTitles[i];
            shPurchaseSummary.Range[titleRow, i + 1].BorderAround(LineStyleType.Thin);
        }

        for (var i = 0; i < soldSummaryTitles.Count; i++)
        {
            shSoldSummary.Range[titleRow, i + 1].Value = soldSummaryTitles[i];
            shSoldSummary.Range[titleRow, i + 1].BorderAround(LineStyleType.Thin);
        }

        shPurchaseDetail.Range[titleRow, 1, titleRow, purchaseDetailTitles.Count].Style.Font.IsBold = true;
        shPurchaseSummary.Range[titleRow, 1, titleRow, purchaseSummaryTitles.Count].Style.Font.IsBold = true;
        shSoldSummary.Range[titleRow, 1, titleRow, purchaseSummaryTitles.Count].Style.Font.IsBold = true;
        var detailRow = 5;
        var purchaseSummaryRow = 5;

        foreach (var inv in purchaseList.Where(inv => inv.GoodsDetail.Count != 0))
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

                shPurchaseDetail.Range[detailRow, 1].Value2 = inv.InvoiceNumber;
                shPurchaseDetail.Range[detailRow, 2].Value2 = inv.InvoiceNotation;
                shPurchaseDetail.Range[detailRow, 3].Text = inv.SellerTaxCode;
                shPurchaseDetail.Range[detailRow, 4].Value2 = inv.SellerName;
                shPurchaseDetail.Range[detailRow, 5].Value2 = item.Name;
                shPurchaseDetail.Range[detailRow, 6].Value2 = item.UnitCount;

                shPurchaseDetail.Range[detailRow, 7].Value2 = unitPrice;
                shPurchaseDetail.Range[detailRow, 7].NumberFormat = "#,##0";

                shPurchaseDetail.Range[detailRow, 8].Value2 = preTaxPrice;
                shPurchaseDetail.Range[detailRow, 8].NumberFormat = "#,##0";

                shPurchaseDetail.Range[detailRow, 9].Value2 = item.Rate;
                shPurchaseDetail.Range[detailRow, 9].NumberFormat = "0.0%";

                shPurchaseDetail.Range[detailRow, 10].Value2 = item.Discount;

                shPurchaseDetail.Range[detailRow, 11].Value2 = vat;
                shPurchaseDetail.Range[detailRow, 11].NumberFormat = "#,##0";
                shPurchaseDetail.Range[detailRow, 12].Value2 = inv.CreationDate?.ToLocalTime();
                shPurchaseDetail.Range[detailRow, 12].Style.NumberFormat = "dd/mm/yyyy";

                shPurchaseDetail.Range[detailRow, 13].Value2 = inv.SigningDate?.ToLocalTime();
                shPurchaseDetail.Range[detailRow, 13].Style.NumberFormat = "dd/mm/yyyy";

                shPurchaseDetail.Range[detailRow, 14].Value2 = inv.IssueDate?.ToLocalTime();
                shPurchaseDetail.Range[detailRow, 13].Style.NumberFormat = "dd/mm/yyyy";

                shPurchaseDetail.Range[detailRow, 15].Value2 = inv.Status;
                shPurchaseDetail.Range[detailRow, 16].Value2 = inv.InvoiceType;

                #endregion

                detailRow++;
            }

            #region Summary

            shPurchaseSummary.Range[purchaseSummaryRow, 1].Value2 = inv.InvoiceNumber;
            shPurchaseSummary.Range[purchaseSummaryRow, 2].Value2 = inv.InvoiceNotation;
            shPurchaseSummary.Range[purchaseSummaryRow, 3].Text = inv.SellerTaxCode;
            shPurchaseSummary.Range[purchaseSummaryRow, 4].Value2 = inv.SellerName;
            shPurchaseSummary.Range[purchaseSummaryRow, 5].Value2 = inv.CreationDate?.ToLocalTime();
            shPurchaseSummary.Range[purchaseSummaryRow, 5].Style.NumberFormat = "dd/mm/yyyy";
            shPurchaseSummary.Range[purchaseSummaryRow, 6].Value2 = inv.SigningDate?.ToLocalTime();
            shPurchaseSummary.Range[purchaseSummaryRow, 6].Style.NumberFormat = "dd/mm/yyyy";
            shPurchaseSummary.Range[purchaseSummaryRow, 7].Value2 = inv.IssueDate?.ToLocalTime();
            shPurchaseSummary.Range[purchaseSummaryRow, 7].Style.NumberFormat = "dd/mm/yyyy";
            shPurchaseSummary.Range[purchaseSummaryRow, 8].Value2 = inv.TotalPrice;
            shPurchaseSummary.Range[purchaseSummaryRow, 8].NumberFormat = "#,##0";

            shPurchaseSummary.Range[purchaseSummaryRow, 9].Value2 = inv.Vat;
            shPurchaseSummary.Range[purchaseSummaryRow, 9].NumberFormat = "#,##0";

            shPurchaseSummary.Range[purchaseSummaryRow, 10].Value2 = inv.TotalPriceVat;
            shPurchaseSummary.Range[purchaseSummaryRow, 10].NumberFormat = "#,##0";

            shPurchaseSummary.Range[purchaseSummaryRow, 11].Value2 = inv.Status;
            shPurchaseSummary.Range[purchaseSummaryRow, 12].Value2 = inv.InvoiceType;
            shPurchaseSummary.Range[purchaseSummaryRow, 13].Value2 = inv.Risk is null or false ? "OK" : "Rủi ro";

            #endregion

            purchaseSummaryRow++;
        }

        var soldSummaryRow = 5;
        foreach (var inv in soldList)
        {
            shSoldSummary.Range[soldSummaryRow, 1].Value2 = inv.InvoiceNumber;
            shSoldSummary.Range[soldSummaryRow, 2].Value2 = inv.InvoiceNotation;
            shSoldSummary.Range[soldSummaryRow, 3].Value2 = inv.BuyerTaxCode;
            shSoldSummary.Range[soldSummaryRow, 4].Value2 = inv.BuyerName;
            shSoldSummary.Range[soldSummaryRow, 5].Value2 = inv.CreationDate?.ToLocalTime();
            shSoldSummary.Range[soldSummaryRow, 5].Style.NumberFormat = "dd/mm/yyyy";
            shSoldSummary.Range[soldSummaryRow, 6].Value2 = inv.SigningDate?.ToLocalTime();
            shSoldSummary.Range[soldSummaryRow, 6].Style.NumberFormat = "dd/mm/yyyy";
            shSoldSummary.Range[soldSummaryRow, 7].Value2 = inv.IssueDate?.ToLocalTime();
            shSoldSummary.Range[soldSummaryRow, 7].Style.NumberFormat = "dd/mm/yyyy";
            shSoldSummary.Range[soldSummaryRow, 8].Value2 = inv.TotalPrice;
            shSoldSummary.Range[soldSummaryRow, 8].NumberFormat = "#,##0";
            shSoldSummary.Range[soldSummaryRow, 9].Value2 = inv.Vat;
            shSoldSummary.Range[soldSummaryRow, 9].NumberFormat = "#,##0";
            shSoldSummary.Range[soldSummaryRow, 10].Value2 = inv.TotalPriceVat;
            shSoldSummary.Range[soldSummaryRow, 10].NumberFormat = "#,##0";
            shSoldSummary.Range[soldSummaryRow, 11].Value2 = inv.Status;

            soldSummaryRow++;
        }

        shPurchaseDetail.Range[4, 1, detailRow - 1, 3].AutoFitColumns();
        shPurchaseDetail.Range[4, 6, detailRow - 1, 16].AutoFitColumns();

        shPurchaseSummary.Range[4, 1, purchaseSummaryRow - 1, 3].AutoFitColumns();
        shPurchaseSummary.Range[4, 5, purchaseSummaryRow - 1, 13].AutoFitColumns();

        shSoldSummary.Range[4, 1, soldSummaryRow - 1, 3].AutoFitColumns();
        shSoldSummary.Range[4, 5, soldSummaryRow - 1, 11].AutoFitColumns();

        #region Formula and filter

        shPurchaseSummary.AutoFilters.Range = shPurchaseSummary.Range[$"A{titleRow}:X{detailRow - 1}"];
        shPurchaseDetail.AutoFilters.Range = shPurchaseDetail.Range[$"A{titleRow}:X{purchaseSummaryRow - 1}"];
        shSoldSummary.AutoFilters.Range = shSoldSummary.Range[$"A{titleRow}:X{soldSummaryRow - 1}"];

        shPurchaseSummary.Range[3, 1].FormulaR1C1 =
            $"\"Tổng số hóa đơn: \"&COUNT(A{titleRow + 1}:A{purchaseSummaryRow - 1})";
        shPurchaseDetail.Range[3, 1].FormulaR1C1 =
            $"\"Tổng số hóa đơn: \"&COUNT(UNIQUE(A{titleRow + 1}:A{detailRow - 1}))";
        shSoldSummary.Range[3, 1].FormulaR1C1 = $"\"Tổng số hóa đơn: \"&COUNT(A{titleRow + 1}:A{soldSummaryRow - 1})";

        for (var i = 8; i <= 10; i++)
        {
            shPurchaseSummary.Range[titleRow - 1, i].FormulaR1C1 = $"=SUBTOTAL(9,R5C{i}:R{purchaseSummaryRow - 1}C{i})";
            shPurchaseSummary.Range[titleRow - 1, i].NumberFormat = "#,##0";
            shPurchaseSummary.Range[titleRow - 1, i].Style.Font.IsBold = true;
        }

        for (var i = 7; i <= 11; i++)
        {
            shPurchaseDetail.Range[titleRow - 1, i].FormulaR1C1 = $"=SUBTOTAL(9,R5C{i}:R{detailRow - 1}C{i})";
            shPurchaseDetail.Range[titleRow - 1, i].NumberFormat = "#,##0";
            shPurchaseDetail.Range[titleRow - 1, i].Style.Font.IsBold = true;
        }

        for (var i = 8; i <= 10; i++)
        {
            shSoldSummary.Range[titleRow - 1, i].FormulaR1C1 = $"=SUBTOTAL(9,R5C{i}:R{detailRow - 1}C{i})";
            shSoldSummary.Range[titleRow - 1, i].NumberFormat = "#,##0";
            shSoldSummary.Range[titleRow - 1, i].Style.Font.IsBold = true;
        }

        #endregion

        foreach (var cell in shPurchaseDetail.Range[4, 1, detailRow - 1, 16])
        {
            cell.BorderAround(LineStyleType.Thin);
        }

        foreach (var cell in shPurchaseSummary.Range[4, 1, purchaseSummaryRow - 1, 13])
        {
            cell.BorderAround(LineStyleType.Thin);
        }

        foreach (var cell in shSoldSummary.Range[4, 1, soldSummaryRow - 1, 11])
        {
            cell.BorderAround(LineStyleType.Thin);
        }

        using var stream = new MemoryStream();
        workbook.SaveToStream(stream, FileFormat.Version2016);
        return stream.ToArray();
    }

    #endregion
}