using System.ComponentModel.DataAnnotations;

namespace StockTrack.Dto.LocationList
{
    public class CreateLocationListDto
    {
        [Required(ErrorMessage = "Hastane adı boş geçilemez.")]
        [StringLength(100, ErrorMessage = "Hastane adı en fazla 100 karakter olabilir.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Adres boş geçilemez.")]
        [StringLength(300, ErrorMessage = "Adres en fazla 300 karakter olabilir.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Yetkili kişi boş geçilemez.")]
        [StringLength(50, ErrorMessage = "Yetkili kişi adı en fazla 50 karakter olabilir.")]
        public string ContactPerson { get; set; }

        [Required(ErrorMessage = "Telefon boş geçilemez.")]
        [StringLength(20, ErrorMessage = "Telefon numarası en fazla 20 karakter olabilir.")]
        public string Phone { get; set; }
    }
}
