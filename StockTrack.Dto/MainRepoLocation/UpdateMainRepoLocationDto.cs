using System.ComponentModel.DataAnnotations;

namespace StockTrack.Dto.MainRepoLocation
{
    public class UpdateMainRepoLocationDto
    {
        public int  Id { get; set; }
        [Required(ErrorMessage = "Lokasyon adı boş geçilemez.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Lokasyon adresi boş geçilemez.")]
        public string? Adress { get; set; }
    }
}
