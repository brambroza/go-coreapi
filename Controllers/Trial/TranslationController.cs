using System;
using System.Data;
using goalongapi.DB;
using goalongapi.Models;
using goalongapi.Models.Trial;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
 

namespace goalongapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationsController : ControllerBase
    {
        

        // ✅ โหลดคำแปลตามภาษา
        [HttpGet("{lang}")]
        public async Task<IActionResult> GetTranslationsByLang(string lang)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getTranslations] @lang='" + lang + "' ";
            dt = DB.DBConn.GetDataTable(_cmd);

             var translationDict = new Dictionary<string, string>();

                foreach (DataRow row in dt.Rows)
                {
                    var key = row["Key"]?.ToString();
                    var value = row["Value"]?.ToString();

                    if (!string.IsNullOrEmpty(key))
                    {
                        translationDict[key] = value ?? string.Empty;
                    }
                }

                return Ok(translationDict);

          
        }


       [HttpGet("[action]")]
        public async Task<IActionResult> GetTransLangAll()
        {
       
                     DataTable pivotTable = new System.Data.DataTable();
                        string _cmd;
                        _cmd = "exec dbo.[getTranslang_All]  ";
                        pivotTable = DB.DBConn.GetDataTable(_cmd);
 
               
                    DataTable langTable = DB.DBConn.GetDataTable("SELECT Code FROM dbo.Languages");
                    var langs = langTable.AsEnumerable()
                        .Select(r => r["Code"].ToString()!)
                        .ToList();

                    // ✅ 3. สร้าง Response Object
                    var response = new
                    {
                        languages = langs,
                        data = pivotTable.AsEnumerable().Select(row => new
                        {
                            key = row["Key"].ToString()!,
                            @namespace = row["Namespace"].ToString()!,
                            values = langs.ToDictionary(lang => lang, lang => row[lang]?.ToString() ?? string.Empty)
                        }).ToList()
                    };

                    return Ok(response);
                }




        // ✅ เพิ่มคำแปลใหม่
        [HttpPost]
        public async Task<IActionResult> setTranslation([FromBody] Translation tran)
        {
             MsgReturn msgretrun = new MsgReturn();


            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setLang ";
                _cmd += " @Id  =" + tran.Id + "";
                _cmd += ",@Key  ='" + tran.Key + "'";
                _cmd += ",@Lang ='" + tran.Lang + "'";
                _cmd += ",@Namespace ='" + tran.Namespace + "'";
                _cmd += " , @Value='" + tran.Value + "'";
             
               if (DB.DBConn.ExecuteOnly(_cmd))
                {
                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Save Success !!";
                    return Ok(msgretrun);
                }
                else
                {
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return BadRequest(msgretrun);
                }

            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return BadRequest(msgretrun);
            }


        }

       

       
    }
}
