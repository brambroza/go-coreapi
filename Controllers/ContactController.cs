using System;
using System.Data;
using goalongapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace goalongapi.Controllers
{
    [ApiController]
    [Authorize]
    public class ContactController : ControllerBase
    {
        [HttpGet]
        [Route("Contact")]
        public IActionResult Get([FromQuery] string cmpid)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getContact @CmpId='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            List<ContactList> contactLists = new List<ContactList>();

            foreach (DataRow r in dt.Rows)
            {
                var contactList = new ContactList();

                contactList.UpdUser = r["UpdUser"].ToString();
                contactList.ContactName = r["ContactName"].ToString();
                contactList.ContactPhone = r["ContactPhone"].ToString();
                contactList.ContactEmail = r["ContactEmail"].ToString();
                contactList.ContactPosition = r["ContactPosition"].ToString();
                contactList.ContactLineId = r["ContactLineId"].ToString();
                contactList.Remark = r["Remark"].ToString();
                contactList.CmpId = r["CmpId"].ToString();
                contactList.ContactId = r["ContactId"].ToString();
                contactList.ImgPath = r["ImgPath"].ToString();
                contactList.DocNo = r["DocNo"].ToString();
                contactList.DocType = r["DocType"].ToString();

                contactLists.Add(contactList);
            }

            return Ok(new { contacts = contactLists });
        }

        [HttpPost]
        [Route("Contact")]
        public IActionResult Post([FromBody] ContactList customer)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setContact";
                _cmd += " @UpdUser  ='" + customer.UpdUser + "'";
                _cmd += ",@ContactId  ='" + customer.ContactId + "'";
                _cmd += ",@ContactName  ='" + customer.ContactName + "'";
                _cmd += ",@ContactPhone  ='" + customer.ContactPhone + "'";
                _cmd += ",@ContactEmail  ='" + customer.ContactEmail + "'";
                _cmd += ",@ContactPosition  ='" + customer.ContactPosition + "'";
                _cmd += ",@ContactLineId  ='" + customer.ContactLineId + "'";
                _cmd += ",@ImgPath  ='" + customer.ImgPath + "'";
                _cmd += ",@CmpId  ='" + customer.CmpId + "'";
                _cmd += ",@Remark  ='" + customer.Remark + "'";
                _cmd += ",@DocNo  ='" + customer.DocNo + "'";
                _cmd += ",@DocType  ='" + customer.DocType + "'";

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
                    return Ok(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return Ok(msgretrun);
            }
        }

        [HttpDelete]
        [Route("Contact")]
        public void Delete([FromQuery] string contactId, [FromQuery] string cmpid)
        {
            string _cmd = "";
            _cmd =
                "delete from dbo.Contact where  ContactId='"
                + contactId
                + "' and cmpid='"
                + cmpid
                + "'";
            DB.DBConn.ExecuteOnly(_cmd);
        }



        [HttpPost]
        [Route("ContactSocail")]
        public IActionResult Post([FromBody] ContactSocail customer)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setContactFormLiffEdit";
                _cmd += " @SocialId  ='" + customer.SocialId + "'";
                _cmd += ",@CmpId  ='" + customer.CmpId + "'";
                _cmd += ",@Name  ='" + customer.Name + "'";
                _cmd += ",@Branch  ='" + customer.Branch + "'";
                _cmd += ",@Phone  ='" + customer.PhoneNo + "'";
                _cmd += ",@Position  ='" + customer.Position + "'";
                _cmd += ",@Surname  ='" + customer.Surname + "'";
                _cmd += ",@Nickname  ='" + customer.Nickname + "'";
                _cmd += ",@Email  ='" + customer.Email + "'";

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
                    return Ok(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return Ok(msgretrun);
            }
        }


        [HttpDelete]
        [Route("delContactSocail")]
        public IActionResult delContactSocail([FromQuery] string socialid,
            [FromQuery] string cmpid)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.delContactFormLiffEdit";
                _cmd += " @SocialId  ='" + socialid + "'";
                _cmd += ",@CmpId  ='" + cmpid + "'";


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
                    return Ok(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return Ok(msgretrun);
            }
        }



    }
}
