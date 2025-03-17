namespace WebApp.Services.InvoiceService.dto;

public class InvoiceDisplayDto
{
    public string Id { get; set; } = string.Empty;
    public string SellerTaxCode { get; set; } = string.Empty;
    public string? SellerAddress { get; set; }
    public string SellerName { get; set;} = string.Empty;
    public string BuyerTaxCode { get; set; } = string.Empty;
    public string? BuyerAddress { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string? InvoiceNumber { get; set; } //Số hóa đơn
    public string InvoiceNotation { get; set; } = string.Empty; //Ký hiệu hóa đơn
    public int? InvoiceGroupNotation { get; set; } //Ký hiệu mẫu số
    public string? VerifyCode { get; set; }
    public List<Goods> GoodsDetail { get; set; } = []; //Hàng hóa
    public double? TotalPrice { get; set; }
    public double? Vat { get; set; }
    public double? TotalPriceVat { get; set; }
    
    public double? ChietKhau { get; set; }
    public double? Phi { get; set; }
    public string? TotalInWord { get; set; }
    public DateTime? CreationDate { get; set; } //Ngày lập
    public DateTime? SigningDate { get; set; } //Ngày ký
    public DateTime? IssueDate { get; set; } //Ngày cấp mã
    public string? Status { get; set; } //Trạng thái
    public int? StatusNumber { get; set; }
    public string? InvoiceType { get; set; } //Loại hóa đơn
    public int? InvoiceTypeNumber { get; set; } //Mã loại hóa đơn

    public bool? Risk { get; set; } =  false;
    
    public string? SellerSignature { get; set; }
    
}

public class Goods
{
    public string? Name { get; set; }
    public string? UnitCount { get; set; } //Đơn vị tính
    public double? UnitPrice { get; set; } //Đơn giá
    public double? Quantity { get; set; } //Số lượng
    public decimal? Rate { get; set; } //Thuế suất
    public decimal? PreTaxPrice { get; set; } //Giá trước thuế
    public double? Discount { get; set; } //Chiết khấu
    public decimal? Tax { get; set; } //Tiền thuế
    public string? TaxType { get; set; } //Loại thuế suất
}