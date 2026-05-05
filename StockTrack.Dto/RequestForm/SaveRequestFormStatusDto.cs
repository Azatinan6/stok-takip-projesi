using System.ComponentModel.DataAnnotations;

namespace StockTrack.Dto.RequestForm
{
      public class SaveRequestFormStatusDto
    {
        // Talep formunun ID'si zorunludur.
        [Required(ErrorMessage = "Talep Form ID'si zorunludur.")]
        public int RequestFormDetailId { get; set; }
        [Required(ErrorMessage = "Durum seçimi zorunludur.")]
        public int StatusId { get; set; }

        [StringLength(500, ErrorMessage = "İptal sebebi en fazla 500 karakter olabilir.")]
        public string? CancelledDesc { get; set; }
    }
}
