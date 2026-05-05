using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace StockTrack.Dto.Product
{
    public class CreateProductDto
    {

        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Ürün adı en az 2, en fazla 100 karakter olmalıdır.")]
        public string Name { get; set; }
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Model adı en az 2, en fazla 100 karakter olmalıdır.")]
        public string Model { get; set; }

        [Range(0, 1000, ErrorMessage = "WarningThreshold değeri 0 ile 1000 arasında olmalıdır.")]
        public int? WarningThreshold { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olmalıdır.")]
        public string? Description { get; set; }


        [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
        public int CategoryId { get; set; }
        public IFormFile? PhotoUrl { get; set; }
    }
}

