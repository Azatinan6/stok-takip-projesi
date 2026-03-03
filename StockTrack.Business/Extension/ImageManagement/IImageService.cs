using Microsoft.AspNetCore.Http;

namespace StockTrack.Business.Extension.ImageManagement
{
    public interface IImageService
    {
        Task<string> SaveImageAsync(IFormFile imageFile, string uploadName = null);
    }
}
