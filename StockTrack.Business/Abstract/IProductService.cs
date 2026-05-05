using StockTrack.Entity.Enitities;

namespace StockTrack.Business.Abstract
{
    public interface IProductService:IGenericService<Product>
    {
        Task<bool> TExistsProductAsync(string name);
        Task<Product?> TGetExistingCategoryProductAsync(int categoryId, string productName);

    }
}
