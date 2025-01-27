using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models
{
    
    public class ReceiveModel
    {
        public string UpdUser { get; set; }
        public string ReceiveNo { get; set; }
        public string ReceiveDate { get; set; }
        public string ReceiveBy { get; set; }
        public string PurChaseNo { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceDate { get; set; }
        public int ReceiveType { get; set; }
        public int CmpId { get; set; }
        public string Remark { get; set; }
        public int StateApp { get; set; }
        public string AppBy { get; set; }

        public string SupplierCode { get; set; }
        public int SysWHId { get; set; }
        public int SysWHLocId { get; set; }
    }


    public class InvenTransModel
    {
        public string UpdUser { get; set; }
        public int Seq { get; set; }
        public string DocNo { get; set; }
        public string TransDate { get; set; }
        public int SysWHId { get; set; }
        public int SysWHLocId { get; set; }
        public string BarcodeNo { get; set; }
        public string ProductCode { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Qty { get; set; }
        public string UnitCode { get; set; }
        public string PurChaseNo { get; set; }
        public int StateReserve { get; set; }
        public string BatchNo { get; set; }
        public string Grade { get; set; }
        public string DateExpire { get; set; }
        public int StateQC { get; set; }
        public string QCBy { get; set; }
        public string TransType { get; set; }
        public string CmpId { get; set; }
    }



    public class AdjustModel
    {
        public string UpdUser { get; set; }
        public string AdjustNo { get; set; }
        public string AdjustDate { get; set; }
        public string AdjustBy { get; set; }
        public string PurChaseNo { get; set; }
        public string CmpId { get; set; }
        public string Remark { get; set; }
        public int StateApp { get; set; }
        public int WHId { get; set; }
        public int WHLocId { get; set; }
        public int AdjustType { get; set; }

    }


    public class ReserveModel
    {
        public string UpdUser { get; set; }
        public string ReserveNo { get; set; }
        public string ReserveDate { get; set; }
        public string ReserveBy { get; set; }
        public string ProjectNo { get; set; }
        public int CmpId { get; set; }
        public string Remark { get; set; }
        public int StateApp { get; set; }
        public int WHId { get; set; }
        public int WHLocId { get; set; }
        public int ReserveType { get; set; }

    }


    public class IssueModel
    {
        public string UpdUser { get; set; }
        public string IssueNo { get; set; }
        public string IssueDate { get; set; }
        public string IssueBy { get; set; }
        public string  CmpId { get; set; }
        public string Remark { get; set; }
        public string DocRef { get; set; }
        public int WHId { get; set; }
        public int WHLocId { get; set; }
        public string ProjectNo { get; set; }

    }

    public class QualityModel
    {
        public string UpdUser { get; set; }
        public string QualityNo { get; set; }
        public string QualityDate { get; set; }
        public string QualityBy { get; set; }
        public string PurChaseNo { get; set; }
        public int CmpId { get; set; }
        public string Remark { get; set; }

    }

    public class ReturnToSuplModel
    {
        public string UpdUser { get; set; }

        public string ReturnToSuplNo { get; set; }
        public string ReturnToSuplDate { get; set; }
        public string ReturnToSuplBy { get; set; }
        public string PurChaseNo { get; set; }
        public string CmpId { get; set; }
        public string Remark { get; set; }
        public int ReturnType { get; set; }
        public int WHId { get; set; }
        public int WHLocId { get; set; }
        public string SupplierCode { get; set; }


    }

    public class ReturnToStock
    {
        public string UpdUser { get; set; }
        public string ReturnToStockNo { get; set; }
        public string ReturnToStockDate { get; set; }
        public string ReturnToStockBy { get; set; }
        public string IssueNo { get; set; }
        public int CmpId { get; set; }
        public string Remark { get; set; }
        public int SysWHId { get; set; }
        public int SysWHLocId { get; set; }
    } 

    public class invenAppModel
    {
        public string DocNo { get; set; }
        public int StateApp { get; set; }
        public string AppBy { get; set; }
        public string Type { get; set; }
    }

    public class TransferWHModel
    {
        public string UpdUser { get; set; }
        public string TransferWHNo { get; set; }
        public string TransferWHDate { get; set; }
        public string TransferWHBy { get; set; }
        public int CmpId { get; set; }
        public string Remark { get; set; }
        public int DocRef { get; set; }
        public int WHId { get; set; }
        public int WHLocId { get; set; }
        public int WHToId { get; set; }
        public int WHLocToId { get; set; }
    } 
    public class TransferWHRcvModel
    {
        public string UpdUser { get; set; }
        public string TransferWHNo { get; set; }
        public string TransferWHDate { get; set; }
        public string TransferWHBy { get; set; }
        public int CmpId { get; set; }
        public string Remark { get; set; }
        public int DocRef { get; set; }
        public int WHId { get; set; }
        public int WHLocId { get; set; }
    }


}