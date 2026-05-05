using System.Collections.Generic;

namespace StockTrack.Dto.Set
{
    public class SetEditDto
    {
        public int Id { get; set; } // Düzenlenen Setin ID'si
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public List<int> ProductIds { get; set; } // Setin içindeki ürünlerin ID listesi
    }
}