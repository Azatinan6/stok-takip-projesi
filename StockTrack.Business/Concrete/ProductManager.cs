using Microsoft.EntityFrameworkCore;
using StockTrack.Business.Abstract;
using StockTrack.DataAccess.Abstract;
using StockTrack.DataAccess.Context;
using StockTrack.DataAccess.EntityFramework;
using StockTrack.Entity.Enitities;

namespace StockTrack.Business.Concrete
{
    public class ProductManager : GenericManager<Product>, IProductService
    {
        private readonly AppDbContext _appDbContext;
        public ProductManager(IGenericDal<Product> genericDal, AppDbContext appDbContext) : base(genericDal)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Product?> TGetExistingCategoryProductAsync(int categoryId, string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                return null;

            var normalized = productName.Trim().ToLower();

            return await _appDbContext.Products
                .Where(p => p.CategoryId == categoryId && p.Name.ToLower() == normalized)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> TExistsProductAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return await _appDbContext.Products.AnyAsync(p => p.Name.ToLower() == name.Trim().ToLower());
        }
    }
}

