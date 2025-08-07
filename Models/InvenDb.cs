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
        public string PurchaseNo { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceDate { get; set; }
        public string ReceiveType { get; set; }
        public string CmpId { get; set; }
        public string Remark { get; set; }
        public string StateApp { get; set; }
        public string AppBy { get; set; }

        public string SupplierCode { get; set; }
        public int SysWHId { get; set; }
        public int SysWHLocId { get; set; }

        public string? ImgPath { get; set; }
        public string? WareHouseName { get; set; }
        public string? WareHouseLocName { get; set; }
        public string? SupplierName { get; set; }
        public string? PurchaseDate { get; set; }
        public decimal VatType { get; set; }
        public decimal ReceiveAmt { get; set; }
        public decimal ReceiveDisPer { get; set; }
        public decimal ReceiveDisAmt { get; set; }
        public decimal ReceiveNetAmt { get; set; }
        public decimal ReceiveVatAmt { get; set; }
        public decimal ReceiveGrandAmt { get; set; }
        public string ReceiveGrandAmtTHB { get; set; }
        public string ReceiveGrandAmtENB { get; set; }
        public List<InvenTransModel> items { get; set; }

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
        public string PurchaseNo { get; set; }
        public int StateReserve { get; set; }
        public string BatchNo { get; set; }
        public string Grade { get; set; }
        public string DateExpire { get; set; }
        public int StateQC { get; set; }
        public string QCBy { get; set; }
        public string TransType { get; set; }
        public string CmpId { get; set; }
        public string? ProdDescription { get; set; }
        public List<InvenTransModelSerial>? serials { get; set; }
    }



    public class InvenTransModelSerial
    {
        public string UpdUser { get; set; }
        public int Seq { get; set; }
        public string DocNo { get; set; }
        public string TransDate { get; set; }
        public int SysWHId { get; set; }
        public int SysWHLocId { get; set; }
        public string BarcodeNo { get; set; }
        public string ProductCode { get; set; }
        public decimal Qty { get; set; }
        public string UnitCode { get; set; }
        public string WarrantyStartDate { get; set; }
        public string WarrantyEndDate { get; set; }
        public string WarrantyPeriod { get; set; }
        public string SerialNumber { get; set; }
        public string MACAddress { get; set; }
        public string TransType { get; set; }
        public string CmpId { get; set; }
        public string? StatusInStock { get; set; }
        public string? ProdDescription { get; set; }
        public int MainSeq { get; set; }
    }




    public class AdjustModel
    {
        public string UpdUser { get; set; }
        public string AdjustNo { get; set; }
        public string AdjustDate { get; set; }
        public string AdjustBy { get; set; }

        public string CmpId { get; set; }
        public string Remark { get; set; }

        public int SysWHId { get; set; }
        public int SysWHLocId { get; set; }
        public int AdjustType { get; set; }

        public string StateApp { get; set; }
        public string AppBy { get; set; }

        public string StateSend { get; set; }
        public string SendAppBy { get; set; }

        public string? WareHouseName { get; set; }
        public string? WareHouseLocName { get; set; }
        public string? Status { get; set; }
        public string? SendTo { get; set; }

        public string Type { get; set; }
        public string Reason { get; set; }
        public string? RefDocNo { get; set; }

        public List<AdjustItem> items { get; set; }

    }

        public class AdjustItem
        {
            public string UpdUser { get; set; }
            public int Seq { get; set; }
            public string DocNo { get; set; }
            public int SysWHId { get; set; }
            public int SysWHLocId { get; set; }
            public string BarcodeNo { get; set; }
            public string ProductCode { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal QtySystem { get; set; }
            public decimal QtyCounted { get; set; }
            public decimal AdjustQty { get; set; }
            public decimal QtyAfter { get; set; }
            public string UnitCode { get; set; }
            public string? BatchNo { get; set; }
            public string? Grade { get; set; }
            public string? DateExpire { get; set; }
            public int StateQC { get; set; }
            public string TransType { get; set; }
            public string CmpId { get; set; }
            public string ProdDescription { get; set; }
            public decimal Amt { get; set; }
            public string? Imgpath { get; set; }
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
        public string CmpId { get; set; }
        public string Remark { get; set; }
        public string DocRef { get; set; }
        public int SysWHId { get; set; }
        public int SysWHLocId { get; set; }
        public string ProjectNo { get; set; }
        public string StateApp { get; set; }
        public string? WareHouseName { get; set; }
        public string? WareHouseLocName { get; set; }
        public string? AppBy { get; set; }
        public List<InvenTransModel> items { get; set; }

        public string? CustomerName { get; set; }
        public string? ReferCode { get; set; }

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
        public string PurchaseNo { get; set; }
        public string CmpId { get; set; }
        public string Remark { get; set; }
        public string ReturnType { get; set; }
        public int SysWHId { get; set; }
        public int SysWHLocId { get; set; }
        public string SupplierCode { get; set; }
        public string? WareHouseName { get; set; }
        public string? WareHouseLocName { get; set; }
        public string? SupplierName { get; set; }
        public string? PurchaseDate { get; set; }
         public string StateApp { get; set; }
      
        public string? AppBy { get; set; }

        public List<InvenTransModel> items { get; set; }


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
        public string  CmpId { get; set; }
        public string Remark { get; set; }
        public string DocRef { get; set; }
        public int SysWHId { get; set; }
        public int SysWHLocId { get; set; }
        public int? SysWHToId { get; set; }
        public int? SysWHLocToId { get; set; }
        public string? TransferWHApp { get; set; }
        public string? TransferWHAppBy { get; set; }

        public string? WareHouseName { get; set; }
        public string? ToWareHouseName { get; set; }
        public string? WareHouseLocName { get; set; }
        public string? ToWareHouseLocName { get; set; }
        public string? Status { get; set;   }

         public List<InvenTransItemModel> items { get; set; }
    }


    public class InvenTransItemModel
    {
        public string UpdUser { get; set; }
        public int Seq { get; set; }
        public string DocNo { get; set; }
        public string TransDate { get; set; }
        public int SysWHId { get; set; }
        public int SysWHLocId { get; set; }
        public int SysWHToId { get; set; }
        public int SysWHLocToId { get; set; }

        public string BarcodeNo { get; set; }
        public string ProductCode { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Qty { get; set; }
        public string UnitCode { get; set; }
        public string? PurchaseNo { get; set; }
        public int StateReserve { get; set; }
        public string BatchNo { get; set; }
        public string? Grade { get; set; }
        public string? DateExpire { get; set; }
        public int? StateQC { get; set; }
        public string? QCBy { get; set; }
        public string TransType { get; set; }
        public string CmpId { get; set; }
        public string? ProdDescription { get; set; }

        public string? WareHouseName { get; set; }
        public string? ToWareHouseName { get; set; }
        public string? WareHouseLocName { get; set; }
        public string? ToWareHouseLocName { get; set; }
        
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