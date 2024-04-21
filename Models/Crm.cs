using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace coreapi.Models
{

    public class CrmGrpModel
    {
        public string CreateUser { get; set; }
        public string CmpId { get; set; }
        public string GrpId { get; set; }
        public string GrpName { get; set; }
        public string GrpDescription { get; set; }
    }

    public class CrmTaskMoveModel
    {
        public string CreateUser { get; set; }
        public string CmpId { get; set; }
        public string GrpId { get; set; }
        public string TaskId { get; set; }
    }

    public class CrmTaskModel
    {
        public string CreateUser { get; set; }
        public string CmpId { get; set; }
        public string GrpId { get; set; }
        public string TaskId { get; set; }
        public string TaskName { get; set; }
        public string SalesName { get; set; }
        public int Taskrating { get; set; }
        public decimal ExpRevenue { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string ImgPath { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerProvince { get; set; }
        public string CustomerDistrict { get; set; }
        public string CustomerSubDistrict { get; set; }
        public string CustomerPostCode { get; set; }
        public string CustomerWebsite { get; set; }
        public string CustomerContactName { get; set; }
        public string CustomerContactTile { get; set; }
        public string CustomerContactJobPosition { get; set; }
        public string CustomerContactMobile { get; set; }
        public string Note { get; set; }
    }


    public class CRMFilesModel
    {
        public string CreateUser { get; set; }
        public string CmpId { get; set; }
        public string TaskId { get; set; }
        public int Seq { get; set; }
        public string FilePath { get; set; }
        public string Description { get; set; }
    }

        public class CRMAppointmentModel
    {
        public string CreateUser { get; set; }
        public string CmpId { get; set; }
        public string TaskId { get; set; }
        public int Seq { get; set; }
        public string AppointmentDescription { get; set; }
        public string AppointmentDate { get; set; }
        public string AppointmentTime {get;set;}
        public string AppointmentType {get;set;}
    }





    public class CRMCommentModel
    {
        public string CreateUser { get; set; }
        public string CmpId { get; set; }
        public string TaskId { get; set; }
        public string CommentId { get; set; }
        public string Author { get; set; }
        public string Avatar { get; set; }
        public string Content { get; set; }
        public string CommentDateTime { get; set; }
        public int likes { get; set; }
        public int dislikes { get; set; }
    }

    public class CRMComment_likesModel
    {
        public string CreateUser { get; set; }
        public string CmpId { get; set; }
        public string TaskId { get; set; }
        public string CommentId { get; set; }
        public int Seq { get; set; }
        public string Userlikes { get; set; }
    }


    public class CRMComment_dislikesModel
    {
        public string CreateUser { get; set; }
        public string CmpId { get; set; }
        public string TaskId { get; set; }
        public string CommentId { get; set; }
        public int Seq { get; set; }
        public string Userdislikes { get; set; }
    }



    public class CRMComment_filesModel
    {
        public string CreateUser { get; set; }
        public string CmpId { get; set; }
        public string TaskId { get; set; }
        public string CommentId { get; set; }
        public int Seq { get; set; }
        public string FilePath { get; set; }
    }



    public class getCrm
    {
        public string grpid { get; set; }
        public string grpname { get; set; }
        public string grpdesciption { get; set; }
        public List<getCrmTask> items { get; set; }
        public decimal expRevenueTotal { get;set;}

    }



    public class getCrmTask
    {

        public string taskId { get; set; }
        public string taskname { get; set; }
        public string salesname { get; set; }
        public int taskrating { get; set; }
        public decimal expRevenue { get; set; }
        public string customername { get; set; }
        public string customerEmail { get; set; }
        public string customerPhone { get; set; }
        public string imgPath { get; set; }
        public string customeraddress { get; set; }
        public string customerProvince { get; set; }
        public string customerDistrict { get; set; }
        public string customerSubDistrict { get; set; }
        public string customerPostCode { get; set; }
        public string customerWebsite { get; set; }
        public string customerContactName { get; set; }
        public string customerContactTile { get; set; }
        public string customerContactJobPosition { get; set; }
        public string customerContactMobile { get; set; }
        public string note { get; set; }
        public List<getCrmFile> files { get; set; }
        public List<getCRMComment> comments { get; set; }
        public List<getCrmAppointment> appointment {get;set;}

        public int Progress {get;set;}
    }





    public class getCrmFile
    {

        public int Seq { get; set; }
        public string filePath { get; set; }
        public string description { get; set; }
    }


    public class getCRMComment
    {

        public string commentId { get; set; }
        public string author { get; set; }
        public string avatar { get; set; }
        public string content { get; set; }
        public string datetime { get; set; }
        public int likes { get; set; }
        public int dislikes { get; set; }
    }


      public class getCrmAppointment
    {

        public int Seq { get; set; }
        public string appointmentdescription { get; set; }
        public string appointmentdate { get; set; }
        public string appointmenttime { get; set; }
        public string appointmenttype { get; set; }


    }
















}





