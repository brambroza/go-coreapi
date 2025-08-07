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
                                values = langs.Select(lang => new
                                {
                                    lang = lang,
                                    value = row[lang]?.ToString() ?? string.Empty
                                }).ToList()
                            }).ToList()
                        };

                    return Ok(response);
                }




        // ✅ เพิ่มคำแปลใหม่
        [HttpPost("[action]")]
        public async Task<IActionResult> setTranslation([FromBody] TranslationItem tran)
        {
             MsgReturn msgretrun = new MsgReturn();

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();


            try
            {
                string _cmd = "";

                for (int i = 0; i < tran.Values.Count; i++)
                {
                    _cmd = "exec  dbo.setLang ";
                    _cmd += " @key  ='" + tran.Key + "'";
                    _cmd += ",@lang ='" + tran.Values[i].Lang + "'";
                    _cmd += ",@namespace ='" + tran.Namespace + "'";
                    _cmd += " , @value=N'" + tran.Values[i].Value + "'";
                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        msgretrun.ReturnCode = "400";
                        msgretrun.Msg = "Error !!";
                        return Ok(msgretrun);
                    }
                }
 

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                msgretrun.ReturnCode = "200";
                msgretrun.Msg = "Save Success !!";
                return Ok(msgretrun);
                
            }
            catch
            {
                 DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return Ok(msgretrun);
            }


        }

       

       
    }
}
