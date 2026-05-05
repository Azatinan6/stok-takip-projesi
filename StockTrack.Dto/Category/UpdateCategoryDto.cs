using System.ComponentModel.DataAnnotations;

namespace StockTrack.Dto.Category
{
    public class UpdateCategoryDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Kategori adı boş geçilemez.")]
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
