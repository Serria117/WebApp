using WebApp.Mongo.DeserializedModel;
using WebApp.Mongo.DocumentModel;
using WebApp.Services.InvoiceService.dto;
using WebApp.Services.RestService.Dto;

namespace WebApp.Utils;

public static class InvoiceExtension
{
    public static InvoiceDisplayDto ToDisplayModel(this InvoiceDetailDoc doc)
    {
        return new InvoiceDisplayDto
        {
            Id = doc.Id ?? string.Empty,
            StatusNumber = doc.Tthai,
            BuyerName = doc.Nmten ?? string.Empty,
            BuyerTaxCode = doc.Nmmst ?? string.Empty,
            SellerName = doc.Nbten ?? string.Empty,
            SellerTaxCode = doc.Nbmst ?? string.Empty,
            InvoiceNotation = doc.Khhdon ?? string.Empty,
            InvoiceGroupNotation = doc.Khmshdon,
            InvoiceNumber = doc.Shdon?.ToString(),
            TotalPrice = doc.Tgtcthue,
            Vat = doc.Tgtthue,
            TotalPriceVat = doc.Tgtttbso,
            CreationDate = doc.Tdlap?.ToLocalTime(),
            SigningDate = doc.Nky?.ToLocalTime(),
            IssueDate = doc.Ncma?.ToLocalTime(),
            Risk = doc.Risk ?? false,
            Status = doc.Tthai switch
            {
                1 => "Hóa đơn mới",
                2 => "Hóa đơn thay thế",
                3 => "Hóa đơn bị thay thế",
                4 => "Hóa đơn điều chỉnh",
                5 => "Hóa đơn bị điều chỉnh",
                6 => "Hóa đơn hủy",
                _ => string.Empty
            },
            InvoiceType = doc.Ttxly switch
            {
                5 => "Hóa đơn cấp mã",
                6 => "Hóa đơn không cấp mã",
                8 => "Hóa đơn từ máy tính tiền",
                _ => string.Empty
            },
            InvoiceTypeNumber = doc.Ttxly,
            GoodsDetail = doc.Hdhhdvu.Select(g => new Goods
            {
                Name = g.Ten,
                UnitCount = g.Dvtinh,
                UnitPrice = g.Dgia,
                Quantity = g.Sluong,
                PreTaxPrice = g.Thtien,
                Rate = g.Tsuat,
                Discount = g.Stckhau,
                Tax = g.Thtien is null || g.Tsuat is null  
                    ? 0 
                    : Math.Round(g.Thtien.Value * g.Tsuat.Value, 0)
            }).ToList(),
        };
    }
    
    public static InvoiceDisplayDto ToDisplayModel(this InvoiceDetailModel doc)
    {
        return new InvoiceDisplayDto
        {
            Id = doc.Id ?? string.Empty,
            StatusNumber = doc.Tthai,
            BuyerName = doc.Nmten ?? string.Empty,
            BuyerTaxCode = doc.Nmmst ?? string.Empty,
            SellerName = doc.Nbten ?? string.Empty,
            SellerTaxCode = doc.Nbmst ?? string.Empty,
            InvoiceNotation = doc.Khhdon ?? string.Empty,
            InvoiceGroupNotation = doc.Khmshdon,
            InvoiceNumber = doc.Shdon?.ToString(),
            TotalPrice = doc.Tgtcthue,
            Vat = doc.Tgtthue,
            TotalPriceVat = doc.Tgtttbso,
            CreationDate = doc.Tdlap?.ToLocalTime(),
            SigningDate = doc.Nky?.ToLocalTime(),
            IssueDate = doc.Ncma?.ToLocalTime(),
            Status = doc.Tthai switch
            {
                1 => "Hóa đơn mới",
                2 => "Hóa đơn thay thế",
                3 => "Hóa đơn bị thay thế",
                4 => "Hóa đơn điều chỉnh",
                5 => "Hóa đơn bị điều chỉnh",
                6 => "Hóa đơn hủy",
                _ => string.Empty
            },
            InvoiceType = doc.Ttxly switch
            {
                5 => "Hóa đơn cấp mã",
                6 => "Hóa đơn không cấp mã",
                8 => "Hóa đơn từ máy tính tiền",
                _ => string.Empty
            },
            InvoiceTypeNumber = doc.Ttxly,
            GoodsDetail = doc.Hdhhdvu.Select(h => new Goods
            {
                Name = h.Ten,
                UnitCount = h.Dvtinh,
                UnitPrice = h.Dgia,
                Quantity = h.Sluong,
                PreTaxPrice = h.Thtien,
                Rate = h.Tsuat,
                Discount = h.Stckhau,
                Tax = Math.Round(h.Thtien * h.Tsuat, 0)
            }).ToList(),
        };
    }
    
    public static InvoiceDisplayDto ToDisplayModel(this InvoiceModel inv)
    {
        return new InvoiceDisplayDto
        {
            Id = inv.Id ?? string.Empty,
            StatusNumber = inv.Tthai,
            BuyerName = inv.Nmten ?? string.Empty,
            BuyerTaxCode = inv.Nmmst ?? string.Empty,
            SellerName = inv.Nbten ?? string.Empty,
            SellerTaxCode = inv.Nbmst ?? string.Empty,
            InvoiceNotation = inv.Khhdon ?? string.Empty,
            InvoiceGroupNotation = inv.Khmshdon,
            InvoiceNumber = inv.Shdon?.ToString(),
            TotalPrice = inv.Tgtcthue,
            Vat = inv.Tgtthue,
            TotalPriceVat = inv.Tgtttbso,
            CreationDate = inv.Tdlap?.ToLocalTime(),
            SigningDate = inv.Nky?.ToLocalTime(),
            IssueDate = inv.Ncma?.ToLocalTime(),
            Status = inv.Tthai switch
            {
                1 => "Hóa đơn mới",
                2 => "Hóa đơn thay thế",
                3 => "Hóa đơn bị thay thế",
                4 => "Hóa đơn điều chỉnh",
                5 => "Hóa đơn bị điều chỉnh",
                6 => "Hóa đơn hủy",
                _ => string.Empty
            },
            InvoiceType = inv.Ttxly switch
            {
                5 => "Hóa đơn cấp mã",
                6 => "Hóa đơn không cấp mã",
                8 => "Hóa đơn từ máy tính tiền",
                _ => string.Empty
            },
            InvoiceTypeNumber = inv.Ttxly,
        };
    }
}
