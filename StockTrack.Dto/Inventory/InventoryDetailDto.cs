using System;
using System.Collections.Generic;

namespace StockTrack.Dto.Inventory
{
    public class InventoryDetailDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public bool IsArcBox { get; set; }

        // Güncel Stok Adeti
        public int CurrentStock { get; set; }

        public string? StatusDetailsText { get; set; }

        // Alt Listeler (Hareketler ve Seri Numaraları)
        public List<MovementDto> InboundMovements { get; set; } = new List<MovementDto>();
        public List<MovementDto> OutboundMovements { get; set; } = new List<MovementDto>();
        public List<ArcBoxSerialDto> SerialNumbers { get; set; } = new List<ArcBoxSerialDto>();
    }

    public class MovementDto
    {
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
        public int MovementStatusId { get; set; }
        public string? Description { get; set; }

        // YENİ EKLENEN ALANLAR: Tablodaki eksik kolonlar için
        public string? LocationName { get; set; }
        public int OldStock { get; set; }
        public int NewStock { get; set; }
        public string? SerialNumber { get; set; } // Hatanın sebebi bu satırın olmaması!
        public string? EthMac { get; set; }
        public string? WlanMac { get; set; }
        public string? ProductStatusName { get; set; }
        public string? MovementStatusName { get; set; } // Örn: "Kurulum", "Hastaneye Kargo"
    }

    public class ArcBoxSerialDto
    {
        public int Id { get; set; } // MAC girmek için lazım olacak
        public string SerialNumber { get; set; }
        public string? EthMac { get; set; }
        public string? WlanMac { get; set; }

        // YENİ EKLENEN ALANLAR
        public string? WarehouseName { get; set; }
        public string? StatusName { get; set; } // Örn: "Yeni", "Kullanılmış"
        public string ImageUrl { get; set; }      // Görsel Yolu

        public string? Description { get; set; }
    }
}