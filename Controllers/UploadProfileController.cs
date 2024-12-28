using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using coreapi.Models;
using goalongapi.Datatools.Product;
using goalongapi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NohWebApi.Controllers
{
    [ApiController]
    //[Authorize]

    public class UploadProfileController : ControllerBase
    {
        private readonly IWebHostEnvironment webHostEnvironment;

        private readonly IProductService productService;
        private readonly ILogger<UploadProfileController> _logger;

        public UploadProfileController(
            IWebHostEnvironment webHostEnvironment,
            ILogger<UploadProfileController> logger,
            IProductService productService
        )
        {
            _logger = logger;
            this.productService = productService;
            this.webHostEnvironment = webHostEnvironment;
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<string>> UploadImageCmpProfile(List<IFormFile> formFiles)
        {
            if (formFiles == null || formFiles.Count == 0)
            {
                _logger.LogWarning("No files received.");
                return BadRequest("No files received.");
            }

            try
            {
                (string errorMessage, string imageName) = await productService.UploadImage(
                    formFiles
                );
                if (!String.IsNullOrEmpty(errorMessage))
                {
                    _logger.LogError($"Error uploading image: {errorMessage}");
                    return BadRequest(errorMessage);
                }

                _logger.LogInformation($"Image successfully uploaded: {imageName}");
                return Ok(new { ImageName = imageName });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception during file upload: {ex}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<string>> uploadallfile(List<IFormFile> formFiles)
        {
            (string errorMessage, string imageName) = await productService.uploadallfile(formFiles);
            if (!String.IsNullOrEmpty(errorMessage))
            {
                return BadRequest();
            }

            return Ok(new { ImageName = imageName });
        }

        [HttpPost("[action]")]
        public IActionResult movefile(fileinfo fileinfos)
        {
            try
            {
                string formfilepath =
                    $"{webHostEnvironment.WebRootPath}/allfileupload/" + fileinfos.filename;
                string tofilepath =
                    $"{webHostEnvironment.WebRootPath}/"
                    + fileinfos.pathto
                    + "/"
                    + fileinfos.filename;
                string uploadPath = $"{webHostEnvironment.WebRootPath}/" + fileinfos.pathto + "/";

                if (IsValidPaths(uploadPath))
                {
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }
                }

                if (System.IO.File.Exists(tofilepath))
                {
                    System.IO.File.Delete(tofilepath);
                }

                System.IO.File.Move(formfilepath, tofilepath);

                return Ok(fileinfos.pathto + fileinfos.filename);
            }
            catch (Exception e)
            {
                try
                {
                    string formfilepath =
                        $"{webHostEnvironment.WebRootPath}/reqfromcust/fileall/"
                        + fileinfos.filename;
                    string tofilepath =
                        $"{webHostEnvironment.WebRootPath}/"
                        + fileinfos.pathto
                        + "/"
                        + fileinfos.filename;
                    string uploadPath =
                        $"{webHostEnvironment.WebRootPath}/" + fileinfos.pathto + "/";

                    if (IsValidPaths(uploadPath))
                    {
                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }
                    }

                    if (System.IO.File.Exists(tofilepath))
                    {
                        System.IO.File.Delete(tofilepath);
                    }

                    System.IO.File.Move(formfilepath, tofilepath);

                    return Ok(fileinfos.pathto + fileinfos.filename);
                }
                catch (Exception ex)
                {
                    return NotFound();
                }
            }
        }

        [HttpDelete("[action]")]
        public IActionResult removefile([FromQuery] string filepath)
        {
            try
            {
                string fullpath = $"{webHostEnvironment.WebRootPath}/{filepath}";
                if (System.IO.File.Exists(fullpath))
                {
                    System.IO.File.Delete(fullpath);
                }

                return Ok();
            }
            catch (Exception e)
            {
                return NotFound();
            }
        }

        private bool IsValidPaths(string path)
        {
            try
            {
                // This will check for any invalid characters in the path
                Path.GetFullPath(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
