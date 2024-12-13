using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks;
using coreapi.Hubs;
using coreapi.Models;
using goalongapi.Installers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace coreapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        [HttpGet("[action]")]
        public IActionResult getOrganizationTeam([FromQuery] string cmpid)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[org_getOrganization_Team] @CmpId=" + cmpid + "  ";
            dt = DB.DBConn.GetDataTable(_cmd);

            List<OrganizationTeam> organizations = new List<OrganizationTeam>();

            foreach (DataRow row in dt.Rows)
            {
                var organization = new OrganizationTeam()
                {
                    Id = row["Id"].ToString(),
                    CmpId = row["CmpId"].ToString(),
                    JobDescription = int.Parse(row["JobDescription"].ToString()),
                    UpdUser = row["UpdUser"].ToString(),
                    TeamName = row["TeamName"].ToString(),
                    Approvedocby = row["Approvedocby"].ToString(),
                    TeamAll = row["TeamAll"].ToString(),
                };
                organizations.Add(organization);
            }

            return Ok(organizations);
        }

        [HttpGet("[action]")]
        public IActionResult getOrganizationById([FromQuery] string cmpid, [FromQuery] string Id)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[org_getOrganizationById] @CmpId='" + cmpid + "' , @Id='" + Id + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            var organizations = BuildHierarchy(dt);

            return Ok(organizations);
        }

        [HttpGet("[action]")]
        public IActionResult getOrganizationByIdEdit(
            [FromQuery] string cmpid,
            [FromQuery] string Id,
            [FromQuery] int AccountID
        )
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.[org_getOrganizationByIdEdit] @CmpId='"
                + cmpid
                + "' , @Id='"
                + Id
                + "' , @AccountID="
                + AccountID;
            dt = DB.DBConn.GetDataTable(_cmd);
            var org = new OrganizationAction();
            foreach (DataRow row in dt.Rows)
            {
                org.AccountID = int.Parse(row["AccountID"].ToString());
                org.CmpId = row["CmpId"].ToString();
                org.Id = row["TeamId"].ToString();
                org.ParrentID = int.Parse(row["ParrentID"].ToString());
                org.StateApprove = int.Parse(row["StateApprove"].ToString());
                org.UpdUser = row["UpdUser"].ToString();
                org.Position = row["Position"].ToString();
            }
            return Ok(org);
        }

        [HttpGet("[action]")]
        public IActionResult getOrganizationTable([FromQuery] string cmpid, [FromQuery] string Id)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[org_getOrganizationTable] @CmpId='" + cmpid + "' , @Id='" + Id + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            var orgs = new List<OrganizationTable>();
            foreach (DataRow row in dt.Rows)
            {
                var org = new OrganizationTable();
                org.AccountID = int.Parse(row["AccountID"].ToString());
                org.CmpId = row["CmpId"].ToString();
                org.Id = row["TeamId"].ToString();
                org.ParrentID = int.Parse(row["ParrentID"].ToString());
                org.StateApprove = int.Parse(row["StateApprove"].ToString());
                org.FullName = row["FullName"].ToString();
                org.Position = row["Position"].ToString();
                org.ImgPath = row["ImgPath"].ToString();
                orgs.Add(org);
            }
            return Ok(orgs);
        }

        [HttpGet("[action]")]
        public IActionResult getOrganization([FromQuery] string cmpid)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[org_getOrganization] @CmpId='" + cmpid + "' ";
            dt = DB.DBConn.GetDataTable(_cmd);

            var organizations = BuildHierarchy(dt);

            return Ok(organizations);
        }

        private List<Organization> BuildHierarchy(
            DataTable dt,
            string? teamId = "",
            int? parentId = null
        )
        {
            // Filter rows based on parentId
            string filter = parentId == null ? "ParrentID = 0" : $"ParrentID = {parentId}";

            if (teamId != "")
            {
                filter += $" AND TeamId = '{teamId.Replace("'", "''")}'";
            }

            var rows = dt.Select(filter);

            if (rows.Length > 0)
            {
                List<Organization> result = new List<Organization>();
                foreach (DataRow row in rows)
                {
                    var organization = new Organization()
                    {
                        Id = row["TeamId"].ToString(),
                        CmpId = row["CmpId"].ToString(),
                        AccountID = int.Parse(row["AccountID"].ToString()),
                        Position = row["Position"].ToString(),
                        FullName = row["FullName"].ToString(),
                        ImgPath = row["ImgPath"].ToString(),
                        StateApprove = int.Parse(row["StateApprove"].ToString()),
                        children = BuildHierarchy(
                            dt,
                            row["TeamId"].ToString(),
                            int.Parse(row["AccountID"].ToString())
                        ) // Recursively populate children
                        ,
                    };

                    result.Add(organization);
                }

                return result;
            }

            return null;
        }

        [HttpPost("[action]")]
        public IActionResult setOrganizationTeam(OrganizationTeam mt)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.org_setOrganization_Team";
                _cmd += " @UpdUser  ='" + mt.UpdUser + "'";
                _cmd += ",@Id ='" + mt.Id + "'";
                _cmd += ",@TeamName ='" + mt.TeamName + "'";
                _cmd += ",@JobDescription  =" + mt.JobDescription + "";
                _cmd += ",@CmpId  ='" + mt.CmpId + "'";

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
                    return NotFound(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }

        [HttpPost("[action]")]
        public IActionResult setOrganization(OrganizationAction mt)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.org_setOrganization";
                _cmd += " @Id  ='" + mt.Id + "'";
                _cmd += ",@CmpId ='" + mt.CmpId + "'";
                _cmd += ",@UpdUser ='" + mt.UpdUser + "'";
                _cmd += ",@Position ='" + mt.Position + "'";
                _cmd += ",@AccountID =" + mt.AccountID + "";
                _cmd += ",@ParrentID =" + mt.ParrentID + "";
                _cmd += ",@StateApprove =" + mt.StateApprove + "";
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
                    return NotFound(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }

        [HttpDelete("[action]")]
        public IActionResult delOrganizationTeam([FromQuery] string cmpId, [FromQuery] string id)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd =
                    " delete from dbo.Organization_Team where Id ='"
                    + id
                    + "' and CmpId='"
                    + cmpId
                    + "'";

                if (DB.DBConn.ExecuteOnly(_cmd))
                {
                    _cmd =
                        " delete from dbo.Organization where TeamId ='"
                        + id
                        + "' and CmpId='"
                        + cmpId
                        + "'  ";

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
                        return NotFound(msgretrun);
                    }
                }
                else
                {
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return NotFound(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }

        [HttpDelete("[action]")]
        public IActionResult delOrganization(
            [FromQuery] string cmpId,
            [FromQuery] string id,
            [FromQuery] int accountID
        )
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd =
                    " delete from dbo.Organization where TeamId ='"
                    + id
                    + "' and CmpId='"
                    + cmpId
                    + "' and AccountID="
                    + accountID
                    + "";

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
                    return NotFound(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }
    }
}
