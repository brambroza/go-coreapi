using System.Diagnostics.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace coreapi.Models
{
    public class AccountCode
    {
        public int AccId { get; set; }
        public string AccCode { get; set; }
        public string AccName { get; set; }
        public string AccMainId { get; set; }

        public string AccTypeId { get; set; }
        public string AccLevelId { get; set; }
        public string StateActive { get; set; }

        public List<AccountCode> children { get; set; }
    }

    public class setAccountCode
    {

        public string UpdUser { get; set; }
        public string AccCode { get; set; }
        public string AccName { get; set; }
        public string StateActive { get; set; }
        public int AccId { get; set; }
        public int AccTypeId { get; set; }
        public int AccLevelId { get; set; }
        public string CmpId { get; set; }
        public string AccMainId { get; set; }

    }

    public class setAccountType
    {
        public string UpdUser { get; set; }
        public string AccTypeCode { get; set; }
        public string AccTypeName { get; set; }
        public string StateActive { get; set; }
        public int AccTypeId { get; set; }

        public string CmpId { get; set; }

    }

    public class setAccountLevel
    {
        public string UpdUser { get; set; }
        public string AccLevelCode { get; set; }
        public string AccLevelName { get; set; }
        public string StateActive { get; set; }
        public int AccLevelId { get; set; }

        public string CmpId { get; set; }
    }

    public class AccountRcvBook
    {
        public string UpdUser { get; set; }
        public string CmpId { get; set; }
        public int BookRcvId { get; set; }
        public string BookRcvCode { get; set; }
        public string BookRcvName { get; set; }
        public int AccRcvType { get; set; }
        public string AccCode { get; set; }
        public string BankCode { get; set; }
        public string BankBranchCode { get; set; }
        public string Remark { get; set; }
        public string BankAccCode { get; set; }
    }


    public class TARTReciveInv_H
    {
        public string UpdUser { get; set; }
        public string RchDocNo { get; set; }
        public string RchDocDate { get; set; }
        public string RchDocType { get; set; }
        public string RchDeptCode { get; set; }
        public string RchUsrCode { get; set; }
        public string RchType { get; set; }
        public string RchCustCode { get; set; }
        public string RchCurCode { get; set; }
        public decimal RchCurExcRate { get; set; }
        public decimal RchAmtTotal { get; set; }
        public decimal RchAmtDis { get; set; }
        public decimal RchAmtChg { get; set; }
        public decimal RchAmtGross { get; set; }
        public decimal RchAmtVatEx { get; set; }
        public decimal RchAmtVat { get; set; }
        public decimal RchAmtNet { get; set; }
        public decimal RchAmtDiffExcRate { get; set; }
        public string RchGndTextEN { get; set; }
        public string RchGndTextTH { get; set; }
        public string RchDocNote { get; set; }

        public string RchRefGLDocNo { get; set; }
        public string CmpId { get; set; }
    }


    public class TARTReciveInv_D
    {
        public string UpdUser { get; set; }
        public string RchDocNo { get; set; }
        public int RcdSeqNo { get; set; }
        public string RcdType { get; set; }
        public string RcdStaVat { get; set; }
        public string RcdAccCode { get; set; }
        public string RcdDeptActivity { get; set; }
        public string RcdDesc { get; set; }
        public string RcdCurCode { get; set; }
        public decimal RcdCurExcRate { get; set; }
        public decimal RcdCurAmt { get; set; }
        public decimal RcdAmt { get; set; }
        public decimal RcdNetAmt { get; set; }
        public string RcdStaAuto { get; set; }
        public string CmpId { get; set; }
    }

    public class TARTReciveInv_I
    {
        public string UpdUser { get; set; }
        public string RchDocNo { get; set; }
        public int RciSeqNo { get; set; }
        public string RciDocNo { get; set; }
        public string RciDocType { get; set; }
        public string RciTypeDoc { get; set; }
        public decimal RciRcvAmtVat { get; set; }
        public decimal RciRcvAmtVatEx { get; set; }
        public decimal RciRcvAmtNet { get; set; }
        public string RciWhhDocNo { get; set; }
        public decimal RciWhhAmtNet { get; set; }
        public decimal RciDphAmtNet { get; set; }
        public string RciStaTaxInv { get; set; }
        public string RciTaxInvNo { get; set; }
        public string RciTaxInvDate { get; set; }
        public string RciARDocNo { get; set; }
        public string RciBLDocNo { get; set; }
        public string CmpId { get; set; }
    }

    public class TARTReciveInv_R
    {
        public string UpdUser { get; set; }
        public string RchDocNo { get; set; }
        public int RcrSeqNo { get; set; }
        public string RcrRcvTypeCode { get; set; }
        public string RcrBookCode { get; set; }
        public string RcrBnkCode { get; set; }
        public string RcrBnkBchCode { get; set; }
        public string RcrChequeNo { get; set; }
        public string RcrChequeDate { get; set; }
        public decimal RcrAmtFee { get; set; }
        public decimal RcrAmt { get; set; }
        public string RcrBnkAccCode { get; set; }
        public string RcrAccCode { get; set; }
        public string RcrNote { get; set; }
        public string CmpId { get; set; }
    }

    public class TARTReciveInvApprove
    {
        public string UpdUser { get; set; }
        public string RchDocNo { get; set; }
        public string CmpId { get; set; }
    }



    public class TARTCreditNote_H
    {
        public string UpdUser { get; set; }
        public string CnhDocNo { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerContact { get; set; }
        public string CmpId { get; set; }
        public string CnhCurCode { get; set; }
        public decimal CnhCurExcRate { get; set; }
        public int CnhVatType { get; set; }
        public string CnhVatRate { get; set; }
        public string CnhDocNote { get; set; }
        public decimal CnhAmtTotal { get; set; }
        public decimal CnhAmtDis { get; set; }
        public decimal CnhAmtchg { get; set; }
        public decimal CnhAmtGross { get; set; }
        public string CnhGndTextEN { get; set; }
        public string CnhGndTextTH { get; set; }
        public decimal CnhAmtVat { get; set; }
        public decimal CnhAmtVatEx { get; set; }
        public decimal CnhAmtNet { get; set; }
        public string CnhRefARDocNo { get; set; }
    }


    public class TARTCreditNote_D
    {
        public string UpdUser { get; set; }
        public string CnhDocNo { get; set; }
        public int CndSeqNo { get; set; }
        public string CndCode { get; set; }
        public string CndDesc { get; set; }
        public string InvoiceNo { get; set; }
        public decimal CndQty { get; set; }
        public string CndUnit { get; set; }
        public decimal CndAmtTotal { get; set; }
        public decimal CndAmtGross { get; set; }
        public string CmpId { get; set; }
        public decimal CndUnitPrice {get;set; }
    }


    public class TARTCreditNote_H_Approve
    {
        public string UpdUser { get; set; }
        public string CnhDocNo { get; set; }
        public string CmpId { get; set; }
        public string CnhStaApprove { get; set; }

    }


    public class TARTBillingSlips_H
    {
        public string UpdUser { get; set; }
        public string BlhDocNo { get; set; }
        public string BlhDocDate {get;set;}
        public string BlhDocType { get; set; }
        public string BlhDeptCode { get; set; }
        public string CmpId { get; set; }
        public string CustomerCode { get; set; }
        public int BlhCustCrTerm { get; set; }
        public string BlhDueDate { get; set; }
        public string BlhDateOfBill { get; set; }
        public string BlhDocNote { get; set; }
        public string BlhGndTextEN { get; set; }
        public string BlhGndTextTH { get; set; }
        public string BlhCmpCode { get; set; }
        public string BlhCustCode { get; set; }
        public string BlhReceiptDocDate { get; set; }
        public decimal BlhGndAmt {get;set; }
    }


    public class TARTBillingSlips_D
    {
        public string UpdUser { get; set; } 
        public string BlhDocNo { get; set; }
        public int BldSeqNo { get; set; }
        public string InvoiceNo { get; set; }
        public string  InvoiceDate { get; set; }
        public string  InvoiceDueDate { get; set; }
        public string InvDocType { get; set; }
        public string BldCurCode { get; set; }
        public decimal BldCurExcRate { get; set; }
        public decimal BldAmtBill { get; set; }
        public decimal BldRcvAmtBill { get; set; }
        public decimal BldNetAmt { get; set; }
        public string CmpId { get; set; }
    }

      public class TARTBillingSlips_H_Approve
    {
        public string UpdUser { get; set; }
        public string BlhDocNo { get; set; }
        public string CmpId { get; set; }
        public string BlhStaApprove { get; set; }

    }










}