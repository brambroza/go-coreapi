using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models
{

    public class Employee
    {
        public string? UpdUser { get; set; }
        public string CmpId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string? EmployeeFirstName { get; set; }
        public string? EmployeeLastName { get; set; }
        public string? EmployeeNickName { get; set; }
        public string? ImgPath { get; set; }
        public string? Gender { get; set; }
        public string? DateOfBirth { get; set; }
        public string? HireDate { get; set; }
        public string? TerminationDate { get; set; }
        public string StateActive { get; set; }
        public string Prefix { get; set; }
        public string Email { get; set; }
        public EmployeePersonal? personal { get; set; }
        public EmployeeContact? contact { get; set; }
        public List<EmployeeSalary>? salary { get; set; }
        public List<EmployeePosition>? positions { get; set; }

        public string? EmployeeFirstNameEN { get; set; }
        public string? EmployeeLastNameEN { get; set; }
        public string? EmployeeNickNameEN { get; set; }
        public int DepartmentId { get; set; }

    }

    public class EmployeePersonal
    {
        public string? UpdUser { get; set; }
        public string CmpId { get; set; }
        public int EmployeeId { get; set; }
        public string IdentificationNo { get; set; }
        public string? PassportNo { get; set; }
        public string? MaritalStatus { get; set; }
        public string? Nationality { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }

    }


    public class EmployeePosition
    {
        public string? UpdUser { get; set; }
        public string CmpId { get; set; }
        public int EmployeeId { get; set; }
        public string DepartmentNo { get; set; }
        public string PositionNo { get; set; }
        public string ManagerId { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? IsCurrent { get; set; }
        public int Seq { get; set; }
        public string? DepartmentName { get; set; }
        public string? PositionName { get; set; }
    }

    public class EmployeeContact
    {
        public string? UpdUser { get; set; }
        public string CmpId { get; set; }
        public int EmployeeId { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? AddrSubDistrict { get; set; }
        public string? AddrDistrict { get; set; }
        public string? AddrProvince { get; set; }
        public string? AddrPostCode { get; set; }
        public string? Phone { get; set; }
    }


    public class EmployeeSalary
    {
        public string? UpdUser { get; set; }
        public string CmpId { get; set; }    // varchar(50) NOT NULL
        public int EmployeeId { get; set; }                 // int NOT NULL
        public string? EffectiveDate { get; set; }        // date NULL
        public decimal? SalaryAmount { get; set; }
        public string? EndDate { get; set; }              // date NULL
        public string? IsCurrent { get; set; }              // varchar(1) NULL ("0"/"1" or "Y"/"N")
        public string? Reason { get; set; }                 // nvarchar(200) NULL
        public int Seq { get; set; }
    }





}