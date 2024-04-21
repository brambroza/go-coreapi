using System.ComponentModel.DataAnnotations;

namespace goalongapi.Datatools.Product
{
    public class ProductRequest
    {
        public int? ProductId { get; set; }
        [Required]
        [MaxLength(100, ErrorMessage = "Name, maximum length 100")]
        public string Name { get; set; } = null!;

        [Range(0, 10000)]
        public int Stock { get; set; }
        [Range(0, 10000)]
        public decimal Price { get; set; }
        public int CategoryId { get; set; }

        public List<IFormFile>? FormFiles {get; set;}




    }


    public class UploadImageCmpProfile
    {
        public string cmpid {get;set;}
        public IFormFile? FormFiles {get;set;}
    }
}