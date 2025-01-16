using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using coreapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace coreapi.Controllers
{
    [ApiController]
    [Authorize]
    public class DashboardSaleController : ControllerBase
    {
        [HttpGet]
        [Route("getCongratulations")]
        public IActionResult Congratulations(
            [FromQuery] string cmpid,
            [FromQuery] string user,
            [FromQuery] string year
        )
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.DashboardSale_Congratulations @User='"
                + user
                + "'  ,@CmpId='"
                + cmpid
                + "' , @Year='"
                + year
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }

        [HttpGet]
        [Route("getTotalSaleOrderWon")]
        public IActionResult TotalSaleOrderWon([FromQuery] string cmpid, [FromQuery] string user)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.DashboardSale_TotalSaleOrder_Won @User='"
                + user
                + "'  ,@CmpId='"
                + cmpid
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }

        [HttpGet]
        [Route("getTotalQuotation")]
        public IActionResult TotalQuotation([FromQuery] string cmpid, [FromQuery] string user)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.DashboardSale_TotalQuotation @User='"
                + user
                + "'  ,@CmpId='"
                + cmpid
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }

        [HttpGet]
        [Route("getOpportunity")]
        public IActionResult Opportunity([FromQuery] string cmpid, [FromQuery] string user)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.DashboardSale_Opportunity @User='" + user + "'  ,@CmpId='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }

        [HttpGet]
        [Route("getSaleCustGroup")]
        public IActionResult SaleCustGroup([FromQuery] string cmpid, [FromQuery] string user)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.DashboardSale_SaleCustGroup @User='"
                + user
                + "'  ,@CmpId='"
                + cmpid
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }

        [HttpGet]
        [Route("getSaleBestTopMonthly")]
        public IActionResult salebesttopmonthly([FromQuery] string cmpid, [FromQuery] string user)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.DashboardSale_SaleTopMonthly @User='"
                + user
                + "'  ,@CmpId='"
                + cmpid
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }

        [HttpGet]
        [Route("getSaleOverviewMonthly")]
        public IActionResult saleOverviewMonthly([FromQuery] string cmpid, [FromQuery] string user)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.DashboardSale_SaleOverView @User='" + user + "'  ,@CmpId='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }

        [HttpGet]
        [Route("getSaleLastCustomer")]
        public IActionResult saleLastCustomer([FromQuery] string cmpid, [FromQuery] string user)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.DashboardSale_SaleLastCustomer @User='"
                + user
                + "'  ,@CmpId='"
                + cmpid
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }

        [HttpGet]
        [Route("getSaleYear")]
        public IActionResult SaleYear([FromQuery] string cmpid, [FromQuery] string user)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.DashboardSale_SaleYear @User='" + user + "'  ,@CmpId='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            var groupedData = dt.AsEnumerable()
                .GroupBy(row => row.Field<int>("QuoYear"))
                .Select(yearGroup => new
                {
                    name = yearGroup.Key.ToString(),
                    data = yearGroup
                        .GroupBy(row => row.Field<string>("JobTypeName"))
                        .Select(jobGroup => new
                        {
                            name = jobGroup.Key,
                            data = Enumerable
                                .Range(1, 12)
                                .Select(month =>
                                    jobGroup
                                        .Where(row => row.Field<int>("QuoMonth") == month)
                                        .Sum(row => row.Field<decimal>("QuotationGrandAmt"))
                                )
                                .ToArray(),
                        })
                        .ToList(),
                })
                .ToList();

            // Convert to JSON
            string jsonResult = JsonConvert.SerializeObject(groupedData, Formatting.Indented);

            /*   string JSONString = string.Empty;
              JSONString = JsonConvert.SerializeObject(dt); */
            return Ok(jsonResult);
        }
    }
}
