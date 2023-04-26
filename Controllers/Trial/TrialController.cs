using coreapi.Models;
using coreapi.Models.Trial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http; 
using System.Data;

namespace coreapi.Controllers
{ 
    public class TrialController : ApiController
    {
        // GET: api/Trial

        [Route("api/CheckDupEmail")]
        [HttpGet]

        public IHttpActionResult checkEmail(string email)
        {
            MsgReturn msgReturn = new MsgReturn();

            try
            {
                DataTable dt = new System.Data.DataTable();
                string _cmd;
                _cmd = "exec dbo.[getCheckEmail]    @Email='" + email + "'";
                dt = DB.DBConn.GetDataTableSystem(_cmd  );

                if (dt.Rows.Count >  0  )
                {
                    msgReturn.ReturnCode = "201";
                    msgReturn.Msg = "Email นี้ถูกใช้งานไปแล้ว..";
                    return Ok(msgReturn);
                }
                else
                {
                    msgReturn.ReturnCode = "200";
                    msgReturn.Msg = "Email ใช้ได้";
                    return Ok(msgReturn);
                }
                    

              
            }
            catch
            {
                msgReturn.ReturnCode = "404";
                msgReturn.Msg = "บันทึกผิดพลาด";
                return Ok(msgReturn);
            }


        }



        [Route("api/SetCmp")] // trail
        [HttpPost]
        public IHttpActionResult UserSignUp(CmpData cmp)
        {
            MsgReturn msgReturn = new MsgReturn();

            string _cmd = "";
            _cmd = "exec  dbo.SetCmp"; 
            _cmd += " @CmpId =" + cmp.CmpId; 
            _cmd += ",@CmpNameTH  ='" + cmp.CmpNameTH + "'"; 
            _cmd += ",@CmpAddressTH  ='" + cmp.CmpAddressTH + "'";
            _cmd += ",@TaxIdTH ='" + cmp.TaxIdTH + "'";
            _cmd += ",@CmpNameEN  ='" + cmp.CmpNameEN + "'"; 
            _cmd += ",@CmpAddressEN  ='" + cmp.CmpAddressEN + "'"; 
            _cmd += ",@TaxIdEN ='" + cmp.TaxIdEN + "'";
            _cmd += ",@TelNo ='" + cmp.TelNo + "'";
            _cmd += ",@Mobile ='" + cmp.Mobile + "'";
            _cmd += ",@FaxNo ='" + cmp.FaxNo + "'";
            _cmd += ",@Website ='" + cmp.Website + "'";
            _cmd += ",@VatType =" + cmp.VatType  ;
            _cmd += ",@StateActive =" + cmp.StateActive  ;
            _cmd += ",@BarnchId =" + cmp.BarnchId  ;
            _cmd += ",@BranchCode ='" + cmp.BranchCode + "'";
            _cmd += ",@BranchName ='" + cmp.BranchName + "'";
            _cmd += ",@Userlogin ='" + cmp.Userlogin + "'";
            if (DB.DBConn.ExecuteOnlySystem(_cmd))
            {
                DataTable _dt = new System.Data.DataTable();
                _cmd = "exec dbo.getCmpid '" + cmp.Userlogin + "'";
                  _dt = DB.DBConn.GetDataTableSystem(_cmd);
                if (_dt.Rows.Count >  0 )
                {
                    msgReturn.CmpId =  _dt.Rows[0][0].ToString(); 
                }

                msgReturn.ReturnCode = "200";
                msgReturn.Msg = "บันทึกสำเร็จ";
                return Ok(msgReturn);
            }
            else
            {

                msgReturn.ReturnCode = "404";
                msgReturn.Msg = "บันทึกผิดพลาด";
                return Ok(msgReturn);

            }



        }

    }
}
