using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using goalongapi.Models;

namespace goalongapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuotationController : ControllerBase
    {
        [HttpGet("all")]
        public ActionResult<DataTable> GetAllQuotations()
        {
            try
            {
                string _cmd = "exec dbo.getQuotationDetail @QuotationNo=''";
                DataTable datatable = DB.DBConn.GetDataTable(_cmd);

                if (datatable.Rows.Count == 0)
                {
                    return NotFound(new { Message = "No quotations found." });
                }

                return Ok(datatable);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public ActionResult<DataTable> GetQuotation(string id)
        {
            try
            {
                string _cmd = $"exec dbo.getQuotationDetail @QuotationNo='{id}'";
                DataTable datatable = DB.DBConn.GetDataTable(_cmd);

                if (datatable.Rows.Count == 0)
                {
                    return NotFound(new { Message = $"No quotation found for ID {id}." });
                }

                return Ok(datatable);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
        }
    }
}
