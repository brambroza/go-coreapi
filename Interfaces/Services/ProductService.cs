using goalongapi.Data;
using goalongapi.Entities;
using goalongapi.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Services
{
    public class ProductService : IProductService
    {
        private readonly DatabaseContext databaseContext;
        private readonly IUploadFileService uploadFileService;
        public ProductService(DatabaseContext databaseContext, IUploadFileService uploadFileService)
        {
            this.uploadFileService = uploadFileService;
            this.databaseContext = databaseContext;
        }

        public async Task Create(Product product)
        {            

            databaseContext.Products.Add(product);
            await databaseContext.SaveChangesAsync();
        }

        public async Task Delete(Product product)
        {
            databaseContext.Products.Remove(product);
            await databaseContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Product>> FindAll()
        {
            return await databaseContext.Products.Include(p => p.Category)
            .OrderByDescending(p => p.ProductId).ToListAsync();
        }

        public async Task<Product> FindById(int id)
        {
            return await databaseContext.Products.Include(p => p.Category)
                                                 .SingleOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task<IEnumerable<Product>> Search(string name)
        {
            return await databaseContext.Products.Include(p => p.Category)
                .Where(p => p.Name.ToLower().Contains(name)).ToListAsync();
        }

        public async Task Update(Product product)
        {
            databaseContext.Products.Update(product);
            await databaseContext.SaveChangesAsync();
        }

        public async Task<(string errorMessage, string imageName)> UploadImage(List<IFormFile> formFiles)
        {
            string errorMesage = String.Empty;
            string imageName = String.Empty;
            if (uploadFileService.IsUpload(formFiles))
            {
                   errorMesage = uploadFileService.Validation(formFiles);
                if (String.IsNullOrEmpty(errorMesage))
                {
                    imageName = (await uploadFileService.UploadImages(formFiles))[0];
                }  
            }
            return (errorMesage, imageName);
        }


        public async Task<(string errorMessage, List<string> imageName)> UploadMultiFiles(List<IFormFile> formFiles)
        {
            string errorMesage = String.Empty;
            List<string> imageName = new List<string>();
            if (uploadFileService.IsUpload(formFiles))
            {
                   errorMesage = uploadFileService.Validation(formFiles);
                if (String.IsNullOrEmpty(errorMesage))
                {
                    imageName =  await uploadFileService.UploadImages(formFiles)  ;
                }  
            }
            return (errorMesage, imageName);
        }

        public async Task<(string errorMessage, string filenames)> uploadallfile(List<IFormFile> formFiles)
        {
            string errorMesage = String.Empty;
            string filenames = String.Empty;
            if (uploadFileService.IsUpload(formFiles))
            {
                   errorMesage = uploadFileService.Validation(formFiles);
                if (String.IsNullOrEmpty(errorMesage))
                {
                    filenames = (await uploadFileService.Uploadfilemulti(formFiles))[0];
                }  
            }
            return (errorMesage, filenames);
        }


    }
}