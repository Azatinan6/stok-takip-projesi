using StockTrack.Business.Abstract;
using StockTrack.DataAccess.Abstract;
using StockTrack.DataAccess.Context;
using StockTrack.Entity.Enitities;
using System.Text.RegularExpressions;

namespace StockTrack.Business.Concrete
{
    public class MainRepoLocationManager : GenericManager<MainRepoLocation>, IMainRepoLocationService
    {        
        public MainRepoLocationManager(IGenericDal<MainRepoLocation> genericDal) : base(genericDal)
        {
        }

        //public Task<string> SetPageUrl(string text)
        //{
        //    // Türkçe karakterleri İngilizce karşılıklarına çevir
        //    var replaceMap = new (string, string)[]
        //    {
        //    ("ç", "c"), ("Ç", "c"),
        //    ("ğ", "g"), ("Ğ", "g"),
        //    ("ı", "i"), ("I", "i"),
        //    ("ö", "o"), ("Ö", "o"),
        //    ("ş", "s"), ("Ş", "s"),
        //    ("ü", "u"), ("Ü", "u")
        //    };

        //    foreach (var (turkish, latin) in replaceMap)
        //    {
        //        text = text.Replace(turkish, latin);
        //    }

        //    // Tüm harfleri küçük yap
        //    text = text.ToLowerInvariant();

        //    // Boşlukları tireye dönüştür
        //    text = Regex.Replace(text, @"\s+", "-");

        //    // Sadece İngilizce harf, rakam ve tire bırak
        //    text = Regex.Replace(text, @"[^a-z0-9\-]", "");

        //    // Birden fazla tireyi tek tireye indir
        //    text = Regex.Replace(text, @"-+", "-");

        //    // Baştaki ve sondaki tireleri kaldır
        //    text = text.Trim('-');

        //    return text;
        //}
    }
}

