using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using goalongapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace goalongapi.Controllers
{
    [ApiController]
    [Authorize]
    public class hrmprofileController : ControllerBase
    {

        [HttpGet("[action]")]
        public IActionResult getEmployee([FromQuery] string cmpid, [FromQuery] string user)
        {
            string _cmd;
            _cmd = "exec dbo.get_hrmEmployee @CmpId='" + cmpid + "' , @User='" + user + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.get_hrmEmployee_Personal @CmpId='" + cmpid + "' , @User='" + user + "'";
            DataTable dtp = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.get_hrmEmployee_Contact @CmpId='" + cmpid + "' , @User='" + user + "'";
            DataTable dtc = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.get_hrmEmployee_Salary @CmpId='" + cmpid + "' , @User='" + user + "'";
            DataTable dts = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.get_hrmEmployee_Position @CmpId='" + cmpid + "' , @User='" + user + "'";
            DataTable dtt = DB.DBConn.GetDataTable(_cmd);

            List<Employee> employees = new List<Employee>();

            foreach (DataRow r in dt.Rows)
            {
                var employee = new Employee()
                {
                    UpdUser = r["UpdUser"].ToString(),
                    CmpId = r["CmpId"].ToString(),
                    EmployeeId = int.Parse(r["EmployeeId"].ToString()),
                    EmployeeCode = r["EmployeeCode"].ToString(),
                    EmployeeFirstName = r["EmployeeFirstName"].ToString(),
                    EmployeeLastName = r["EmployeeLastName"].ToString(),
                    EmployeeNickName = r["EmployeeNickName"].ToString(),
                    ImgPath = r["ImgPath"].ToString(),
                    Gender = r["Gender"].ToString(),
                    DateOfBirth = r["DateOfBirth"].ToString(),
                    HireDate = r["HireDate"].ToString(),
                    TerminationDate = r["TerminationDate"].ToString(),
                    StateActive = r["StateActive"].ToString(),
                    Prefix = r["Prefix"].ToString(),
                    Email = r["Email"].ToString(), 
                    personal = new EmployeePersonal(),
                    contact = new EmployeeContact(),
                    salary = new List<EmployeeSalary>(),
                    positions = new List<EmployeePosition>(),




                };

                foreach (DataRow rp in dtp.Select("EmployeeId=" + int.Parse(r["EmployeeId"].ToString())))
                {

                    var personal = new EmployeePersonal()
                    {
                        UpdUser = rp["UpdUser"].ToString(),
                        CmpId = rp["CmpId"].ToString(),
                        EmployeeId = int.Parse(rp["EmployeeId"].ToString()),
                        IdentificationNo = rp["IdentificationNo"].ToString(),
                        PassportNo = rp["PassportNo"].ToString(),
                        MaritalStatus = rp["MaritalStatus"].ToString(),
                        Nationality = rp["Nationality"].ToString(),
                        EmergencyContactName = rp["EmergencyContactName"].ToString(),
                        EmergencyContactPhone = rp["EmergencyContactPhone"].ToString(),

                    };

                    employee.personal = personal;


                }

                foreach (DataRow rp in dtc.Select("EmployeeId=" + int.Parse(r["EmployeeId"].ToString())))
                {

                    var contact = new EmployeeContact()
                    {
                        UpdUser = rp["UpdUser"].ToString(),
                        CmpId = rp["CmpId"].ToString(),
                        EmployeeId = int.Parse(rp["EmployeeId"].ToString()),
                        Address1 = rp["Address1"].ToString(),
                        Address2 = rp["Address2"].ToString(),
                        AddrSubDistrict = rp["AddrSubDistrict"].ToString(),
                        AddrDistrict = rp["AddrDistrict"].ToString(),
                        AddrProvince = rp["AddrProvince"].ToString(),
                        AddrPostCode = rp["AddrPostCode"].ToString(),
                        Phone = rp["Phone"].ToString(),

                    };

                    employee.contact = contact;


                }


                foreach (DataRow rp in dtt.Select("EmployeeId=" + int.Parse(r["EmployeeId"].ToString())))
                {

                    var position = new EmployeePosition()
                    {
                        UpdUser = rp["UpdUser"].ToString(),
                        CmpId = rp["CmpId"].ToString(),
                        EmployeeId = int.Parse(rp["EmployeeId"].ToString()),
                        Seq = int.Parse(rp["Seq"].ToString()),
                        DepartmentNo = rp["DepartmentNo"].ToString(),
                        PositionNo = rp["PositionNo"].ToString(),
                        IsCurrent = rp["IsCurrent"].ToString(),
                        ManagerId = rp["ManagerId"].ToString(),
                        StartDate = rp["StartDate"].ToString(),
                        EndDate = rp["EndDate"].ToString(),
                         DepartmentName = rp["DepartmentName"].ToString(),
                        PositionName = rp["PositionName"].ToString(),

                    };

                    employee.positions.Add(position);


                }



                foreach (DataRow rp in dts.Select("EmployeeId=" + int.Parse(r["EmployeeId"].ToString())))
                {

                    var salary = new EmployeeSalary()
                    {
                        UpdUser = rp["UpdUser"].ToString(),
                        CmpId = rp["CmpId"].ToString(),
                        EmployeeId = int.Parse(rp["EmployeeId"].ToString()),
                        Seq = int.Parse(rp["Seq"].ToString()),
                        EffectiveDate = rp["EffectiveDate"].ToString(),
                        SalaryAmount = decimal.Parse(rp["SalaryAmount"].ToString()),
                        EndDate = rp["EndDate"].ToString(),
                        IsCurrent = rp["IsCurrent"].ToString(),
                        Reason = rp["Reason"].ToString(),

                    };

                    employee.salary.Add(salary);


                }






                employees.Add(employee);
            }

            return Ok(employees);
        }
        

        [HttpPost("[action]")]
        public IActionResult setEmployee([FromBody] Employee employee)
        {
            MsgReturn msgretrun = new MsgReturn();

            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd = "";

                

                _cmd = "exec  dbo.set_hrmEmployee @UpdUser='" + employee.UpdUser + "' ";
                _cmd += ", @CmpId='" + employee.CmpId + "'";
                _cmd += " ,@EmployeeId=" + employee.EmployeeId;
                _cmd += ", @EmployeeCode='" + employee.EmployeeCode + "'";
                _cmd += ", @EmployeeFirstName='" + employee.EmployeeFirstName + "'";
                _cmd += ", @EmployeeLastName='" + employee.EmployeeLastName + "'";
                _cmd += ", @EmployeeNickName='" + employee.EmployeeNickName + "'";
                _cmd += ", @ImgPath='" + employee.ImgPath + "'";
                _cmd += ", @Gender='" + employee.Gender + "'";

                _cmd += ", @DateOfBirth='" + employee.DateOfBirth + "'";
                _cmd += ", @HireDate='" + employee.HireDate + "'";
                 _cmd += ", @TerminationDate='" + employee.TerminationDate + "'";
                _cmd += ", @StateActive='" + employee.StateActive + "'";
                 _cmd += ", @Prefix='" + employee.Prefix + "'";
                _cmd += ", @Email='" + employee.Email + "'";

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return Ok(msgretrun);
                }


                for (int i = 0; i < employee.salary.Count; i++)
                {
                    _cmd =
                        "Exec set_hrmEmployee_Salary @UpdUser='"
                        + employee.salary[i].UpdUser
                        + "'";
                    _cmd += ",@Seq=" + employee.salary[i].Seq;
                    _cmd += ",@EmployeeId=" + employee.salary[i].EmployeeId;


                    _cmd += ",@EffectiveDate='" + employee.salary[i].EffectiveDate + "'";
                    _cmd += ",@SalaryAmount=" + employee.salary[i].SalaryAmount + "";
                    _cmd += ",@EndDate='" + employee.salary[i].EndDate + "'";
                    _cmd += ",@IsCurrent='" + employee.salary[i].IsCurrent + "'";
                    _cmd += ",@Reason='" + employee.salary[i].Reason + "'"; 
                    _cmd += ",@CmpId='" + employee.salary[i].CmpId + "'"; 
 

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


                for (int i = 0; i < employee.positions.Count; i++)
                {
                    _cmd =
                        "Exec set_hrmEmployee_Position @UpdUser='"
                        + employee.positions[i].UpdUser
                        + "'";
                    _cmd += ",@Seq=" + employee.positions[i].Seq;
                    _cmd += ",@EmployeeId=" + employee.positions[i].EmployeeId;

                    _cmd += ",@DepartmentNo='" + employee.positions[i].DepartmentNo + "'";
                    _cmd += ",@PositionNo='" + employee.positions[i].PositionNo + "'";
                    _cmd += ",@ManagerId='" + employee.positions[i].ManagerId + "'"; 
                    _cmd += ",@StartDate='" + employee.positions[i].StartDate + "'"; 
                    _cmd += ",@EndDate='" + employee.positions[i].EndDate + "'";
                    _cmd += ",@IsCurrent='" + employee.positions[i].IsCurrent + "'";
                     _cmd += ",@CmpId='" + employee.positions[i].CmpId + "'";


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


                 if (employee.personal is EmployeePersonal p)
                {
                    _cmd =
                        "Exec set_hrmEmployee_Personal @UpdUser='"
                        + p.UpdUser
                        + "'"; 
                    _cmd += ",@EmployeeId=" + p.EmployeeId;

                    _cmd += ",@CmpId='" + p.CmpId + "'";
                    _cmd += ",@IdentificationNo='" + p.IdentificationNo + "'";
                    _cmd += ",@PassportNo='" + p.PassportNo + "'";
                    _cmd += ",@MaritalStatus='" + p.MaritalStatus + "'";
                    _cmd += ",@Nationality='" + p.Nationality + "'";
                    _cmd += ",@EmergencyContactName='" + p.EmergencyContactName + "'";
                    _cmd += ",@EmergencyContactPhone='" + p.EmergencyContactPhone + "'";
                     

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


                 if (employee.contact is EmployeeContact c)
                {
                    _cmd =
                        "Exec set_hrmEmployee_Contact @UpdUser='"
                        + c.UpdUser
                        + "'"; 
                    _cmd += ",@EmployeeId=" + c.EmployeeId;

                    _cmd += ",@CmpId='" + c.CmpId + "'";
                    _cmd += ",@Address1='" + c.Address1 + "'";
                    _cmd += ",@Address2='" + c.Address2 + "'";
                    _cmd += ",@AddrSubDistrict='" + c.AddrSubDistrict + "'";
                    _cmd += ",@AddrDistrict='" + c.AddrDistrict + "'";
                    _cmd += ",@AddrProvince='" + c.AddrProvince + "'";
                    _cmd += ",@AddrPostCode='" + c.AddrPostCode + "'";
                    _cmd += ",@Phone='" + c.Phone + "'";
                     

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