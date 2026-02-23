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
    }

    public class ArcBoxSerialDto
    {
        public string SerialNumber { get; set; }
        public string? EthMac { get; set; }
        public string? WlanMac { get; set; }
    }
}