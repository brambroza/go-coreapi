using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models
{
    public class MAFortigate
    {
        public string cmpName { set; get; }
        public string contactName { set; get; }
        public string contactPhone { set; get; }
        public string contactEmail { set; get; }
        public string address { set; get; }
        public string contactPosition { set; get; }
        public string serviceType { set; get; }
        public string model { set; get; }
        public string serial { set; get; }
        public string forticloud { set; get; }
        public string maDuration { set; get; }
        public string advanceReplacement { set; get; }
        public string sla { set; get; }
        public string additionalDetail { set; get; }
        public string fromApp { get; set; }
        public string docno { get; set; }
    }

    public class MACiscoServer
    {
        public string cmpName { set; get; }
        public string contactName { set; get; }
        public string contactPhone { set; get; }
        public string contactEmail { set; get; }
        public string address { set; get; }
        public string contactPosition { set; get; }
        public string serviceType { set; get; }
        public string model { set; get; }
        public string serial { set; get; }
        public string partNumber { set; get; }
        public string maBy { set; get; }
        public string maDuration { set; get; }
        public string advanceReplacement { set; get; }
        public string sla { set; get; }
        public string additionalDetail { set; get; }
        public string fromApp { get; set; }
        public string docno { get; set; }
    }

    public class MAOther
    {
        public string cmpName { set; get; }
        public string contactName { set; get; }
        public string contactPhone { set; get; }
        public string contactEmail { set; get; }
        public string address { set; get; }
        public string contactPosition { set; get; }
        public string serviceType { set; get; }
        public string additionalDetail { set; get; }
        public string desiredService { set; get; }

        public string fromApp { get; set; }
        public string docno { get; set; }
    }

    public class ReqFromCustList
    {
        public string UpdUser { get; set; }
        public string CmpId { get; set; }
        public string TicketId { get; set; }
        public string ServiceType { get; set; }
        public string CustomerName { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string Address { get; set; }
        public string AdditionalDetail { get; set; }
        public string AdditionalDetail2 { get; set; }
        public DateTime CreateAt { get; set; }
        public string Status { get; set; }
        public string FromApp { get; set; }

        public List<ReqFromCustItem> ReqItem { get; set; }
        public List<ReqFromCustAssign> ReqAssign { get; set; }
        public List<ReqComment> ReqComments { get; set; }

        public List<ReqFromCustOwner> ReqOwner { get; set; }
        public List<CustomerReqTicketRoute>? ReqRoute { get; set; }

        public string todo { get; set; }
        public decimal completepercent { get; set; }
        public string ticketIdRef { get; set; }
        public string DueDate { get; set; }
        public string Priority { get; set; }
        public string? CustomerImgPath { get; set; }
        public int? TaskUnRead { get; set; } = 0;
        public int? TodoStatus { get; set; } = 0;
        public string? Labels { get; set; } = "";
        public decimal? GrandAmt { get; set; } = 0;
    }

    public class ReqFromCustRemoveItem
    {
        public string UpdUser { get; set; }
        public string CmpId { get; set; }
        public string TicketId { get; set; }
        public string ServiceType { get; set; }
        public int Seq { get; set; }

    }


    public class ReqFromCustItem
    {
        public string CmpId { get; set; }
        public string TicketId { get; set; }
        public string ServiceType { get; set; }
        public string ModelName { get; set; }
        public string SerialNo { get; set; }
        public string PartNo { get; set; }
        public string Forticloud { get; set; }
        public string MABy { get; set; }
        public string MADuration { get; set; }
        public string AdvanceReplacement { get; set; }
        public string SLA { get; set; }
        public string AdditionalDetail { get; set; }
        public string AdditionalDetail2 { get; set; }
        public string DesiredService { get; set; }
        public string FileUrl { get; set; }

        public string FileUrl1 { get; set; }
        public int Seq { get; set; }
    }

    public class ReqFromCustAssign
    {
        public string CmpId { get; set; }
        public string TicketId { get; set; }
        public string UserId { get; set; }
        public string UserFullName { get; set; }
        public string ImgPath { get; set; }

        public string RouteId { get; set; }
        public string RemindId { get; set; }

        public string Permission { get; set; }
    }

    public class ReqFromCustOwner
    {
        public string CmpId { get; set; }
        public string TicketId { get; set; }
        public string UserId { get; set; }
        public string UserFullName { get; set; }
        public string ImgPath { get; set; }
    }

    public class ReqComment
    {
        public string CmpId { get; set; }
        public string CommentId { get; set; }
        public string TicketId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public string Message { get; set; }
        public DateTime PostedAt { get; set; }

        public List<ReplyComment> replyComment { get; set; }
    }

    public class ReplyComment
    {
        public string CmpId { get; set; }
        public string CommentId { get; set; }
        public string TicketId { get; set; }
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
        public string TagUser { get; set; }
        public DateTime PostedAt { get; set; }
    }

    public class ReqUpdateStatus
    {
        public string cmpid { get; set; }
        public string status { get; set; }
        public string ticketId { get; set; }
    }

    public class ReqPriority
    {
        public string cmpid { get; set; }
        public string priority { get; set; }
        public string ticketId { get; set; }
    }

    public class CustomerReqTicketRoute
    {
        public string UpdUser { get; set; }
        public string TicketId { get; set; }
        public string CmpId { get; set; }
        public string RouteId { get; set; }
        public string RemindId { get; set; }
        public string RouteIdBefore { get; set; }
        public DateTime? DueDate { get; set; }
        public int StatusFinish { get; set; }
        public string RouteName { get; set; } = "";
        public string Department { get; set; } = "";
        public int Seq { get; set; }
        public string RemideDescription { get; set; }
        public string DateFinish { get; set; } = "";
        public string UserFinish { get; set; } = "";

        public List<ReqFromCustAssign>? reqAssign { get; set; }
        public List<CustomerReqTicketRouteReply>? reqReply { get; set; }
    }

    public class CustomerReqTicketRouteReply
    {
        public string TicketId { get; set; }
        public string CmpId { get; set; }
        public string RouteId { get; set; }
        public string RemindId { get; set; }
        public int Seq { get; set; }
        public string Comment { get; set; }
        public string FileUrl { get; set; }
        public DateTime createAt { get; set; }
        public string UpdUser { get; set; }
        public string ImgPath { get; set; } = "";
    }

    public class CustomerReqTicketRouteReplyRead
    {
        public string TicketId { get; set; }
        public string CmpId { get; set; }
        public string RouteId { get; set; }
        public string RemindId { get; set; }
        public int Seq { get; set; }
        public string UpdUser { get; set; }

    }


    public class TaskUpdate
    {
        public string updUser { get; set; }
        public string TicketId { get; set; }
        public string RouteId { get; set; }
        public string CmpId { get; set; }
        public string? TaskId  { get; set; } = null;
    }
}
