using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using StockTrack.Business.Abstract;
using StockTrack.Business.Concrete;
using StockTrack.Business.Extension.Dashboard;
using StockTrack.Business.Extension.ImageManagement;
using StockTrack.Business.Extension.Login;
using StockTrack.Business.Extension.UserManagement;
using StockTrack.DataAccess.Abstract;
using StockTrack.DataAccess.Context;
using StockTrack.DataAccess.EntityFramework;
using StockTrack.DataAccess.Repositories;
using StockTrack.Entity.Enitities;
using StockTrack.WebUI.CustomIdentityErrorDescriber;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connection = builder.Configuration.GetConnectionString("SqlConnection");
    options.UseSqlServer(connection);
});

builder.Services.AddSession();

builder.Services.AddIdentity<AppUser, AppRole>(opt =>
{
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireLowercase = false;
    opt.Password.RequireUppercase = false;

    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // 5 dakika kilitli kalacak
    opt.Lockout.MaxFailedAccessAttempts = 5; // 
    opt.Lockout.AllowedForNewUsers = true; // Yeni kullanýcýlar için de aktif olsun
})
    .AddRoleManager<RoleManager<AppRole>>()
    .AddErrorDescriber<CustomIdentityErrorDescriber>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(config =>
{
    config.LoginPath = new PathString("/Auth/Login");
    config.LogoutPath = new PathString("/Auth/Logout");
    config.Cookie = new CookieBuilder
    {
        Name = "StockTrack",
        HttpOnly = true,
        SameSite = SameSiteMode.Strict,
        SecurePolicy = CookieSecurePolicy.SameAsRequest // proje canlýya taþýnýldýnda  Always olaný seçilir
    };
    config.SlidingExpiration = true;
    config.ExpireTimeSpan = TimeSpan.FromDays(1);
    config.AccessDeniedPath = new PathString("/Auth/AccessDenied");
});



builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddScoped<ICargoNameService, CargoNameManager>();
builder.Services.AddScoped<ICargoNameDal, EfCargoNameDal>();

builder.Services.AddScoped<ICargoDefinitionDal, EfCargoDefinitionDal>();
builder.Services.AddScoped<ICargoDefinitionService, CargoDefinitionManager>();

builder.Services.AddScoped<IMailSettingService, MailSettingManager>();
builder.Services.AddScoped<IMailSettingDal, EfMailSettingDal>();

builder.Services.AddScoped<ICategoryService, CategoryManager>();
builder.Services.AddScoped<ICategoryDal, EfCategoryDal>();

builder.Services.AddScoped<IHospitalService, HospitalManager>();
builder.Services.AddScoped<IHospitalDal, EfHospitalDal>();

builder.Services.AddScoped<IProductService, ProductManager>();
builder.Services.AddScoped<IProductDal, EfProductDal>();

builder.Services.AddScoped<ILocationListService, LocationListManager>();
builder.Services.AddScoped<ILocationListDal, EfLocationListDal>();

builder.Services.AddScoped<IMainRepoLocationService, MainRepoLocationManager>();
builder.Services.AddScoped<IMainRepoLocationDal, EfMainRepoLocationDal>();

builder.Services.AddScoped(typeof(IGenericDal<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IGenericService<>), typeof(GenericManager<>));


builder.Services.AddControllersWithViews()
    .AddNToastNotifyToastr(new ToastrOptions()
    {
        PositionClass = ToastPositions.TopRight,
        TimeOut = 2000
    });

builder.Services.AddHttpClient();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
//app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();

app.UseRouting();
app.UseStaticFiles();


app.MapStaticAssets();

app.UseSession();


app.UseAuthentication();

app.UseAuthorization();

app.UseStatusCodePagesWithReExecute("/Error/StatusCode", "?code={0}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();
