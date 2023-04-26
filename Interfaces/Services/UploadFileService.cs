using goalongapi.Interfaces;

namespace goalongapi.Services
{
    public class UploadFileService : IUploadFileService
    {
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment webHostEnvironment;
        public UploadFileService(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.configuration = configuration;
        }

        public bool IsUpload(List<IFormFile> formFiles)
        {
            return formFiles != null && formFiles.Sum(f => f.Length) > 0;
        }

        public async Task<List<string>> UploadImages(List<IFormFile> formFiles)
        {
            List<string> listFileName = new List<string>();
            string uploadPath = $"{webHostEnvironment.WebRootPath}/images/";

            foreach (var formFile in formFiles)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
                string fullPath = uploadPath + fileName;
                using (var stream = File.Create(fullPath))
                {
                    await formFile.CopyToAsync(stream);
                }
                listFileName.Add(fileName);
            }
            return listFileName;
        }

        public string? Validation(List<IFormFile> formFiles)
        {
            foreach (var formFile in formFiles)
            {
                if (!ValidationExtension(formFile.FileName))
                {
                    return "Invalid file extension";
                }

                if ((!ValidationSize(formFile.Length)))
                {
                    return "Invalid file size";
                }
            }

            return null;
        }

        public bool ValidationExtension(string fileName)
        {
            string[] permittedExtensions = { ".jpg", ".png" };
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            if (String.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
            {
                return false;
            }
            return true;
        }

        public bool ValidationSize(long fileSize) => configuration.GetValue<long>("FileSizeLimit") >= fileSize;
    }
}