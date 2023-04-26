using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace coreapi.Models
{
    public class Quatation
    {
        public string QuatationNo { get; set; }
        public string QuatationDate { get; set; }
        public string QuatationBy { get; set; }
        public int QuatationState { get; set; }
        public string CustomerCode { get; set; }
        public int CreditType { get; set; }
        public int CreditDate { get; set; }
        public string ProjectName { get; set; }
        public string ReferCode { get; set; }
        public int VatType { get; set; }
        public string Remark { get; set; }
        public string Note { get; set; }
        public decimal QuatationAmt { get; set; }
        public decimal QuatationDisPer { get; set; }
        public decimal QuatationDisAmt { get; set; }
        public decimal QuatationNetAmt { get; set; }
        public decimal QuatationVatAmt { get; set; }
        public decimal QuatationGrandAmt { get; set; }
        public string QuatationGrandAmtTHB { get; set; }
        public string QuatationGrandAmtENB { get; set; }
        public int WithholdingTaxState { get; set; }
        public int ShowSignatureState { get; set; }
        public int CmpId { get; set; }
        public string PriceStand { get; set; }
        public string PaymentDue { get; set; }
        public string Shipping { get; set; }
        public int RevNo { get; set; }
        public string CustomerContactName { get; set; }

        public int Jobtype { get; set; }


    }

    public class QuatationCopy
    {
        public string QuatationNo { get; set; }
        public string QuatationNoNew { get; set; }
        public int RevNo { get; set; } 


    }


    public class SalesBomApprove
    {
        public string UpdUser { get; set; }
        public string UpdDate { get; set; }
        public string UpdTime { get; set; }
        public string BomNo { get; set; }
        public int Rev { get; set; }
        public int StateApp { get; set; }
    }
    public class SalesBom
    {
        public string UpdUser { get; set; }
        public string UpdDate { get; set; }
        public string UpdTime { get; set; }
        public string BomNo { get; set; }
        public int Rev { get; set; }
        public string BomBy { get; set; }
        public string SaleName { get; set; }
        public string CustomerName { get; set; }
        public string CustomerContactName { get; set; }
        public string CustomerContactPhone { get; set; }
        public string CustomerContactEmail { get; set; }
        public string ProjectName { get; set; }
        public int ProjectStatus { get; set; }
        public string Remark { get; set; }
        public int CmpId { get; set; }
    }

    public class SalesBom_Detail
    {
        public string UpdUser { get; set; }
        public string UpdDate { get; set; }
        public string UpdTime { get; set; }
        public string BomNo { get; set; }
        public int Rev { get; set; }
        public int Seq { get; set; }
        public string PartNo { get; set; }
        public string Descriptions { get; set; }
        public decimal Qty { get; set; }
        public decimal QtyBal { get; set; }
        public string DeliveryDate { get; set; }
        public string BalCheckDate { get; set; }
        public string Remark { get; set; }
        public string UnitCode { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public int ReplaceStatus { get; set; }
    }


    public class SalesBom_Action
    {
        public string UpdUser { get; set; }
        public string UpdDate { get; set; }
        public string UpdTime { get; set; }
        public string BomNo { get; set; }
        public int Rev { get; set; }
        public int Seq { get; set; }
        public string DescActions { get; set; }
        public string DateActions { get; set; }
    }

    public class SalesBom_File
    {
        public string UpdUser { get; set; }
        public string UpdDate { get; set; }
        public string UpdTime { get; set; }
        public string BomNo { get; set; }
        public int Rev { get; set; }
        public int Seq { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FlieSize { get; set; }
        public string Remark { get; set; }
    }

}