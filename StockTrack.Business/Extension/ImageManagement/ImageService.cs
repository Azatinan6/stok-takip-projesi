using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace StockTrack.Business.Extension.ImageManagement
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;

        public ImageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveImageAsync(IFormFile imageFile, string uploadName = null)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var extension = Path.GetExtension(imageFile.FileName);

            if (string.IsNullOrWhiteSpace(uploadName))
                uploadName = "uploads";

            var safeFileName = MakeFileNameSafe(Path.GetFileNameWithoutExtension(imageFile.FileName));
            var newFileName = $"{safeFileName}_{timestamp}{extension}";

            var folderPath = Path.Combine(_environment.WebRootPath, uploadName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, newFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return newFileName;
        }

        private string MakeFileNameSafe(string fileName)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(c, '_');

            return fileName;
        }
    }
}
