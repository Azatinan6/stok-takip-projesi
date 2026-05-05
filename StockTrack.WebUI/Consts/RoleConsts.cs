namespace StockTrack.WebUI.Consts
{
    public static class RoleConsts
    {
        public const string Admin = "Admin";
        public const string Yonetici = "Yonetici";
        public const string Moderator = "Moderator";
        public const string Kullanici = "Kullanici";

        public static readonly List<string> Roles = new()
    {
        Admin,
        Yonetici,
        Moderator,
        Kullanici
    };
    }
}
