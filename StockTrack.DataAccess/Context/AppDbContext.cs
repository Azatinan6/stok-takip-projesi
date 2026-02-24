using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StockTrack.Entity.Enitities;

namespace StockTrack.DataAccess.Context
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, int>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<ProductSerialNumber> ProductSerialNumbers { get; set; }
        public DbSet<MailSetting> MailSettings { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Hospital> Hospitals { get; set; }
        public DbSet<CargoDefinition> CargoDefinitions { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<LocationList> LocationLists { get; set; }
        public DbSet<MainRepoLocation> MainRepoLocations { get; set; }
        public DbSet<RequestForm> RequestForms { get; set; }
        public DbSet<RequestProduct> RequestProducts { get; set; }
        public DbSet<ProductInvoice> ProductInvoices { get; set; }
        public DbSet<CargoName> CargoNames { get; set; }
        public DbSet<StatusType> StatusTypes { get; set; }
        public DbSet<RequestFormType> RequestFormTypes { get; set; }
        public DbSet<RequestFormDetail> RequestFormDetails { get; set; }
        public DbSet<ProductMainRepoLocation> ProductMainRepoLocations { get; set; }
        public DbSet<PersonDetail> PersonDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PersonDetail>()
                .HasKey(pd => new { pd.RequestFormDetailId, pd.AppUserId });

            modelBuilder.Entity<ProductMainRepoLocation>()
                .HasKey(pm => new { pm.MainRepoLocationId, pm.ProductId });
        }


    }
}
