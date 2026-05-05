using System;
using System.Collections.Generic;

namespace StockTrack.Dto.Dashboard
{
    public class HospitalDashboardDto
    {
        // --- 1. TEPE METRİKLER (KPI) ---
        public string? HospitalName { get; set; }
        public int TotalSent { get; set; } // Toplam Gönderim
        public int TotalReturned { get; set; } // Toplam İade
        public int ReturnRate { get; set; } // İade Oranı (%)
        public int CriticalStockCount { get; set; } // Kritik Stok Ürün Sayısı

        // (Önceki 30 güne göre artış oranları - Şimdilik örnek hesaplama için)
        public int SentIncrease { get; set; }
        public int ReturnIncrease { get; set; }

        // --- 2. GRAFİK VERİLERİ ---
        // Pasta ve Donut grafikler için etiket ve sayı tutucu
        public List<ChartItemDto> TopSentProducts { get; set; } = new();
        public List<ChartItemDto> TopReturnedProducts { get; set; } = new();
        public List<ChartItemDto> ReturnReasons { get; set; } = new();

        // --- 3. ALT TABLO VERİLERİ ---
        public List<TransactionDto> RecentSent { get; set; } = new(); // Son 5 Gönderim
        public List<TransactionDto> RecentReturns { get; set; } = new(); // Son 5 İade
        public List<CriticalStockDto> CriticalStocks { get; set; } = new(); // Kritik Stoktaki Ürünler

        // --- 4. AKILLI UYARILAR VE RİSK ANALİZİ ---
        public List<string> SmartAlerts { get; set; } = new();
        public string RiskLevel { get; set; } // Düşük, Orta, Yüksek
        public List<string> Highlights { get; set; } = new(); // Öne Çıkanlar
        public List<string> Recommendations { get; set; } = new(); // Önerilen Aksiyonlar
    }

    // Alt Sınıflar (Veri Taşıyıcılar)
    public class ChartItemDto
    {
        public string Label { get; set; }
        public int Value { get; set; }
        public int Percentage { get; set; }
        public string ColorCode { get; set; } // Grafikteki renk
    }

    public class TransactionDto
    {
        public DateTime Date { get; set; }
        public string ProductName { get; set; }
        public string Detail { get; set; } // Gönderim için Miktar, İade için Neden yazacak
    }

    public class CriticalStockDto
    {
        public string ProductName { get; set; }
        public int CurrentStock { get; set; }
        public int AlertLevel { get; set; }
    }
}