using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace StockTrack.Dto.MainRepoLocation
{
    public class CreateProductToDepo
    {
            [Required(ErrorMessage = "Depo seçimi zorunludur.")]
            public int MainRepoLocationId { get; set; }

            [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
            public int CategoryId { get; set; }

            [Required(ErrorMessage = "Ürün seçimi zorunludur.")]
            public int ProductId { get; set; }

            [Required(ErrorMessage = "Toplam adet zorunludur.")]
            [Range(1, int.MaxValue, ErrorMessage = "Toplam adet 1 veya daha büyük olmalıdır.")]
            public int Quantity { get; set; }

            // İrsaliye bilgileri
            [StringLength(50, ErrorMessage = "İrsaliye numarası en fazla 50 karakter olmalıdır.")]
            public string? InvoiceNumber { get; set; }

            public DateTime? InvoiceDate { get; set; }

            [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olmalıdır.")]
            public string? Description { get; set; }
        

    }
}
