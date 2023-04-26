using coreapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
 

namespace coreapi.Controllers
{
    
    public class MADetailController : ApiController
    {
        // GET: api/MADetail
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/MADetail/5
        public IHttpActionResult Get(string id)
        {
            //int[] values = new[] { 1, 2, 3, 4, 5, 4, 4, 3 };

            //var groups = values.GroupBy(v => v);
            //foreach (var group in groups )
            //    Console.WriteLine("Value {0} has {1} items", group.Key, group.Count());
            //int s = int.Parse(id);

            string _QuatationNo = id;
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getMADetail] @MANo='" + _QuatationNo + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            return Ok(dt);
        }
        public IHttpActionResult Get(string DocNo , int CmpId)
        {
            string _QuatationNo = DocNo;
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getMADetail_PODetail] @MANo='" + _QuatationNo + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            return Ok(dt);
        }
        // POST: api/MADetail
        public void Post(List<MaDetail> maDetail)
        {

            

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {

                string _cmd;
                if (maDetail.Count > 0)
                {
                    _cmd = "Delete From dbo.MAService_Detail where MANo='" + maDetail[0].MANo + "'";
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < maDetail.Count; i++)
                {

                  

                    _cmd = "exec  dbo.MAService_DetailTrans";
                    _cmd += " @UpdUser  ='" + maDetail[i].UpdUser + "'";
                    _cmd += ",@MANo  ='" + maDetail[i].MANo + "'";
                    _cmd += ",@Description  ='" + Tool.Tool.validateStr(maDetail[i].Description ) + "'";
                    _cmd += ",@ServiceType =1";// + maDetail[i].ServiceType;
                    _cmd += ",@ProdductCode  ='" + maDetail[i].ProdductCode + "'";
                    _cmd += ",@SerialNumber  ='" + maDetail[i].SerialNumber + "'";
                    _cmd += ",@Model  ='" + maDetail[i].Model + "'";
                    _cmd += ",@Seq =" + maDetail[i].Seq;
                    _cmd += ",@StartDate ='" + maDetail[i].StartDate + "'";
                    _cmd += ",@ExpireDate ='" + maDetail[i].ExpireDate + "'";
                    _cmd += ",@WarningTime ='" + maDetail[i].WarningTime + "'";
                    _cmd += ",@WarningBeforExpireDay =" + maDetail[i].WarningBeforExpireDay;
                    _cmd += ",@NotificationQtySet =" + maDetail[i].NotificationQtySet;
                    _cmd += ",@NotificationPeriodDay =" + maDetail[i].NotificationPeriodDay;
                    _cmd += ",@NotificationQty =" + maDetail[i].NotificationQty;
                    _cmd += ",@ServiceGrp =" + maDetail[i].ServiceGrp;
                    _cmd += ",@ProjectName  ='" + maDetail[i].ProjectName + "'";
                    _cmd += ",@QuatationNo  ='" + maDetail[i].QuatationNo + "'";
                    _cmd += ",@PurchaseNo  ='" + maDetail[i].PurchaseNo + "'";
                    _cmd += ",@ReferNo  ='" + maDetail[i].ReferNo + "'";
                    _cmd += ",@ProductType =" + maDetail[i].ProductType;
                    _cmd += ",@SerialNo ='" + maDetail[i].SerialNo + "'";
                    _cmd += ",@LicensNo ='" + maDetail[i].LicensNo + "'";
                    _cmd += ",@SuplName  ='" + maDetail[i].SuplName + "'";
                    _cmd += ",@InvoiceNo ='" + maDetail[i].InvoiceNo + "'";
                    _cmd += ",@InvoiceDate ='" + maDetail[i].InvoiceDate + "'";
                    _cmd += ",@BrandName ='" + maDetail[i].BrandName + "'";
                    _cmd += ",@PriceSale =" + maDetail[i].PriceSale;
                    _cmd += ",@PricePur =" + maDetail[i].PricePur;

                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        return ;

                    };
                }

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

            }
            catch (Exception ex)
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

            }



        }

        // PUT: api/MADetail/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/MADetail/5
        public void Delete(string id  , int seq)
        {
            string _cmd = "";
            _cmd = "delete from dbo.MAService_Detail where  MANo='" + id + "' and Seq=" + seq;
            DB.DBConn.ExecuteOnly(_cmd);
        }
    }
}
