using System.Collections.Generic;

namespace StockTrack.Dto.Set
{
    public class SetCreateDto
    {
        public string Name { get; set; } // Setin Adı
        public bool IsActive { get; set; } = true; // Aktif mi?

        // Ekranda "+" butonuna basarak eklenecek tüm ürünlerin ID'lerini bu listede toplayacağız!
        public List<int> ProductIds { get; set; }
    }
}