using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO; 
using System.Threading.Tasks;
using System.Web;  
using Microsoft.AspNetCore.Http;

namespace NohWebApi.Controllers
{ 
    
    public class FileUploadController : ApiController
    {

        //private readonly IWebHostEnvironment webHostEnvironment;

        //// POST: api/FileUpload
        //[Route("api/FileUpload")]
        //[HttpPost]
        //public async Task<HttpResponseMessage> PostFormData()
        //{
        //    // Check if the request contains multipart/form-data.
        //    if (!Request.Content.IsMimeMultipartContent())
        //    {
        //        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        //    }

        //    string root = System.Web.Hosting.HostingEnvironment.MapPath("~/Image/Signature");
        //    var provider = new MultipartFormDataStreamProvider(root);

        //    try
        //    {
        //        // Read the form data.
        //        await Request.Content.ReadAsMultipartAsync(provider);

        //        // This illustrates how to get the file names.
        //        int x = 0;
        //        foreach (MultipartFileData file in provider.FileData)
        //        {
        //            x += +1;
        //            var newname = DateTime.Now.ToString("yyyyMMddmmsss");
        //            string pdfpath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Image/Signature"), newname + x + ".png");
        //            File.Move(file.LocalFileName, pdfpath);

        //            var orname = file.Headers.ContentDisposition.Name.ToString();
        //            string[] subs = orname.Split('|');

        //            //foreach (var sub in subs)
        //            //{
        //            //    Console.WriteLine($"Substring: {sub}");
        //            //}

        //            string cmd = "";
        //            cmd = "exec  dbo.sp_savefileSignature @filename='" + newname + x + "' , @name='" +  subs[0].Replace("\"","") + "', @id=" + subs[1].Replace("\"", "");
        //            DB.DBConn.ExecuteOnly(cmd);
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK);
        //    }
        //    catch (System.Exception e)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
        //    }
        //}

         

        //[AllowAnonymous]
        //[Route("api/filecustpo")]
        //[HttpPost]
        //public async Task<bool> Upload()
        //{
        //    try
        //    {
        //        var fileuploadPath = System.Web.HttpContext.Current.Server.MapPath("~/FileAttach/CustPO");  //ConfigurationManager.AppSettings["FileUploadLocation"];

        //        var provider = new MultipartFormDataStreamProvider(fileuploadPath);
        //        var content = new StreamContent(System.Web.HttpContext.Current.Request.GetBufferlessInputStream(true));
        //        foreach (var header in Request.Content.Headers)
        //        {
        //            content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        //        }

        //        await content.ReadAsMultipartAsync(provider);

        //        //string uploadingFileName = provider.FileData.Select(x => x.LocalFileName).FirstOrDefault();
        //        //string originalFileName = String.Concat(fileuploadPath, "\\" + (provider.Contents[0].Headers.ContentDisposition.FileName).Trim(new Char[] { '"' }));
        //        //string _name = String.Concat((provider.Contents[0].Headers.ContentDisposition.Name)).Trim(new Char[] { '"' });
        //        //originalFileName = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/FileAttach/CustPO"), _name + ".jpg");

        //        string _cmd = "";

        //        int countfile = provider.FileData.Count();
        //        if (countfile > 0 )
        //        {
        //            DB.DBConn.SqlConnectionOpen();
        //            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
        //            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

        //            try
        //            {



        //                for (var i = 0; i < countfile; i++)
        //                {
        //                    string uploadingFileName1 = provider.FileData[i].LocalFileName;
        //                    string originalFileName1 = String.Concat(fileuploadPath, "\\" + (provider.Contents[i].Headers.ContentDisposition.FileName).Trim(new Char[] { '"' }));
        //                    string _filename = String.Concat((provider.Contents[i].Headers.ContentDisposition.FileName).Trim(new Char[] { '"' }));
        //                    string nameinfo = String.Concat((provider.Contents[i].Headers.ContentDisposition.Name).Trim(new Char[] { '"' }));
        //                    string[] namesplit = nameinfo.Split('|');

        //                    if (File.Exists(originalFileName1))
        //                    {
        //                        File.Delete(originalFileName1);
        //                    }

        //                    File.Move(uploadingFileName1, originalFileName1);


        //                    _cmd = "exec dbo.SetProject_File ";
        //                    _cmd += " @UpdUser='" + namesplit[1] + "'";
        //                    _cmd += " ,@Seq =" + i + "";
        //                    _cmd += " ,@ProjectNo ='" + namesplit[0] + "'";
        //                    _cmd += " ,@FileName ='" + _filename + "'";
        //                    _cmd += " ,@FilePath ='" + fileuploadPath + "'";
        //                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
        //                    {
        //                        DB.DBConn.Tran.Rollback();
        //                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
        //                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
        //                        return false;
        //                    };


        //                }

        //                DB.DBConn.Tran.Commit();
        //                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
        //                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

        //            }
        //            catch (Exception ex)
        //            {
        //                DB.DBConn.Tran.Rollback();
        //                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
        //                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

        //            }


        //        }



        //        //if (File.Exists(originalFileName))
        //        //{
        //        //    File.Delete(originalFileName);
        //        //}

        //        //File.Move(uploadingFileName, originalFileName);


        //        return true;
        //    }
        //    catch (Exception )
        //    {
        //        return false;
        //    }

        //}



        //[AllowAnonymous]
        //[Route("api/bomfileupload")]
        //[HttpPost]
        //public async Task<bool> bomfile()
        //{
        //    try
        //    {
        //        var fileuploadPath = System.Web.HttpContext.Current.Server.MapPath("~/FileAttach/BomFile");  //ConfigurationManager.AppSettings["FileUploadLocation"];

        //        var provider = new MultipartFormDataStreamProvider(fileuploadPath);
        //        var content = new StreamContent(System.Web.HttpContext.Current.Request.GetBufferlessInputStream(true));
        //        foreach (var header in Request.Content.Headers)
        //        {
        //            content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        //        }

        //        await content.ReadAsMultipartAsync(provider);

            
        //        string _cmd = "";

        //        int countfile = provider.FileData.Count();
        //        if (countfile > 0)
        //        {
        //            DB.DBConn.SqlConnectionOpen();
        //            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
        //            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

        //            try
        //            {

        //                for (var i = 0; i < countfile; i++)
        //                {
        //                    string uploadingFileName1 = provider.FileData[i].LocalFileName;
        //                    string originalFileName1 = String.Concat(fileuploadPath, "\\" + (provider.Contents[i].Headers.ContentDisposition.FileName).Trim(new Char[] { '"' }));
        //                    string _filename = String.Concat((provider.Contents[i].Headers.ContentDisposition.FileName).Trim(new Char[] { '"' }));
        //                    string nameinfo = String.Concat((provider.Contents[i].Headers.ContentDisposition.Name).Trim(new Char[] { '"' }));
        //                    string[] namesplit = nameinfo.Split('|');

        //                    if (File.Exists(originalFileName1))
        //                    {
        //                        File.Delete(originalFileName1);
        //                    }

        //                    File.Move(uploadingFileName1, originalFileName1);


        //                    //_cmd = "exec dbo.SetProject_File ";
        //                    //_cmd += " @UpdUser='" + namesplit[1] + "'";
        //                    //_cmd += " ,@Seq =" + i + "";
        //                    //_cmd += " ,@ProjectNo ='" + namesplit[0] + "'";
        //                    //_cmd += " ,@FileName ='" + _filename + "'";
        //                    //_cmd += " ,@FilePath ='" + fileuploadPath + "'";
        //                    //if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
        //                    //{
        //                    //    DB.DBConn.Tran.Rollback();
        //                    //    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
        //                    //    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
        //                    //    return false;
        //                    //};


        //                }

        //                DB.DBConn.Tran.Commit();
        //                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
        //                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

        //            }
        //            catch (Exception ex)
        //            {
        //                DB.DBConn.Tran.Rollback();
        //                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
        //                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

        //            }


        //        }



             


        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }

        //}



        //[Route("api/upload")]
        //[HttpPost]
        //public async Task<IHttpActionResult> UploadFile(IFormFile file)
        //{
        //    if (file == null || file.Length == 0)
        //    {
        //        return BadRequest("File not provided.");
        //    }

        //    // Save the file to the server
        //    var fileuploadPath = System.Web.HttpContext.Current.Server.MapPath("~/FileAttach/BomFile");  //ConfigurationManager.AppSettings["FileUploadLocation"];

        //    var path = Path.Combine(Directory.GetCurrentDirectory(), fileuploadPath, file.FileName);
        //    using (var stream = new FileStream(path, FileMode.Create))
        //    {
        //        await file.CopyToAsync(stream);
        //    }

        //    return Ok();
        //}



    }
   

}
