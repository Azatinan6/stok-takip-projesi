using System.ComponentModel.DataAnnotations;

namespace StockTrack.WebUI.Enums
{
    public enum EnumStatusType
    {
        // --- MEVCUT TEMEL STATÜLER (Eski verilerin bozulmaması için) ---
        [Display(Name = "Talep Edildi")]
        Talep = 1,

        [Display(Name = "Onay Bekliyor")] // (PDF'teki 'Bekliyor' mantığını bu karşılayacak)
        OnayBekliyor = 2,

        [Display(Name = "Paketlendi")]
        Paketlendi = 3,

        [Display(Name = "Kargoya Verildi")]
        Kargoda = 4,

        [Display(Name = "Teslim Edildi / Tamamlandı")] // (PDF'teki 'Teslim Edildi' mantığı)
        Tamamlandı = 5,

        [Display(Name = "İptal Edildi")]
        İptal = 6,

        [Display(Name = "Kurulum Bekleniyor")]
        KurulumBekliyor = 7,

        // --- PDF'TEN GELEN YENİ "GÖNDERİM" STATÜLERİ ---
        [Display(Name = "Ofisten Teslim Alınacak")]
        OfistenTeslimAlinacak = 11,

        [Display(Name = "İade Geldiği Zaman Kargoya Verilecek")]
        IadeGeldigindeKargolanacak = 13,

        // --- PDF'TEN GELEN YENİ "İADE" STATÜLERİ ---
        [Display(Name = "İade Bekleniyor")]
        IadeBekleniyor = 20,

        [Display(Name = "İade Kargo Teslim Alındı")]
        IadeTeslimAlindi = 21,

        [Display(Name = "İade Kargo Teslim Alınamadı")]
        IadeTeslimAlinamadi = 22,

        [Display(Name = "İade Eksik Teslim Alındı")]
        IadeEksikTeslimAlindi = 23,

        [Display(Name = "İade Fazla Teslim Alındı")]
        IadeFazlaTeslimAlindi = 24
    }
}