namespace goalongapi.Interfaces
{
    public interface IUploadFileService
    {
         bool IsUpload(List<IFormFile> formFiles);
         string Validation(List<IFormFile> formFiles);
         Task<List<string>> UploadImages(List<IFormFile> formFiles);

    }
}