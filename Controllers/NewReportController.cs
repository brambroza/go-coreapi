using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Web.Http; 
using coreapi.Pdf;
 
using coreapi.Models;

namespace coreapi.Controllers
{
   
    public class NewReportController : ApiController
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        // GET: api/NewReport
        // GET: api/NewReport
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/NewReport/5
        public string Get(string id)
        {

            string reportpath = Path.Combine($"{webHostEnvironment.WebRootPath}/Reports"  , "quotation.rpt");
            string pdfpath = Path.Combine( $"{webHostEnvironment.WebRootPath}/Reports" ,   id + ".pdf");

            ExportPdfNew x = new ExportPdfNew(reportpath, pdfpath, id);

            return "OK";

        }

        // PUT: api/NewReport/5
        public string Get(string id , string custcode , string sdate  , string edate , string statewait , string statefinish)
        {

            try
            {
                string reportpath = Path.Combine($"{webHostEnvironment.WebRootPath}/Reports", "problemservicereport.rpt");
                string pdfpath = Path.Combine($"{webHostEnvironment.WebRootPath}/Reports", id + ".pdf");
                
                ExportPdfProblemreport x = new ExportPdfProblemreport(reportpath, pdfpath, custcode, sdate, edate, statewait, statefinish);

               return  pdfpath ;

            }catch(Exception e)
            {
               return  e.Message;
            }
          

        }



        // POST: api/NewReport
       

        // PUT: api/NewReport/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/NewReport/5
        public void Delete(int id)
        {
        }
    }
}
