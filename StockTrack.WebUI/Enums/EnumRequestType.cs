using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace StockTrack.WebUI.Enums
{
    public enum EnumRequestType
    {
        [Display(Name = "Kargo")]
        Kargo = 1,

        [Display(Name = "Kurulum")]
        Kurulum = 2,

        [Display(Name = "Servis")]
        Servis = 3
    }

    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var member = enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault();
            if (member == null)
            {
                return enumValue.ToString();
            }
            var displayAttribute = member.GetCustomAttribute<DisplayAttribute>();
            return displayAttribute?.GetName() ?? enumValue.ToString();
        }
    }

}