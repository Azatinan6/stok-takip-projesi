using System.ComponentModel.DataAnnotations;

namespace StockTrack.WebUI.Enums
{
    public enum EnumStatusType
    {
        [Display(Name = "Talep Edildi")]
        Talep = 1,

        [Display(Name = "Onay Bekliyor")]
        OnayBekliyor = 2,

        [Display(Name = "Paketlendi")]
        Paketlendi = 3,

        [Display(Name = "Kargoya Verildi")]
        Kargoda = 4,

        [Display(Name = "Tamamlandı")]
        Tamamlandı = 5,

        [Display(Name = "İptal Edildi")]
        İptal = 6,
        [Display(Name = "Kurulum Bekleniyor")]
        KurulumBekliyor = 7

    }
}
