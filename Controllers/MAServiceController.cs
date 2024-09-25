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
   
    public class MAServiceController : ApiController
    {
        // GET: api/MAService
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/MAService/5
        public IHttpActionResult Get(string id)
        {
            string _QuotationNo = id;
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getMAService] @MANo='" + _QuotationNo + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            return Ok(dt);
        }


        // POST: api/MAService
        public void Post(List<MaService> maServices)
        {


            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {

                string _cmd;
                if (maServices.Count > 0)
                {
                    _cmd = "Delete From dbo.MAService_Service where MANo='" + maServices[0].MANo + "'";
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < maServices.Count; i++)
                {                    
                    _cmd = "exec  dbo.MAService_ServiceTrans";
                    _cmd += "  @UpdUser  ='" + maServices[i].UpdUser + "'";
                    _cmd += ",@MANo  ='" + maServices[i].MANo + "'";
                    _cmd += ",@ServiceType =" + maServices[i].ServiceType;
                    _cmd += ",@Description  ='" + maServices[i].Description + "'";
                    _cmd += ",@Model  ='" + maServices[i].Model + "'";
                    _cmd += ",@Seq =" + maServices[i].Seq;
                    _cmd += ",@StartDate  ='" + maServices[i].StartDate + "'";
                    _cmd += ",@ExpireDate  ='" + maServices[i].ExpireDate + "'";
                    _cmd += ",@WarningTime  ='" + maServices[i].WarningTime + "'";
                    _cmd += ",@WarningBeforExpireDay =" + maServices[i].WarningBeforExpireDay;
                    _cmd += ",@NotificationQtySet =" + maServices[i].NotificationQtySet;
                    _cmd += ",@NotificationPeriodDay =" + maServices[i].NotificationPeriodDay;
                    _cmd += ",@NotificationQty =" + maServices[i].NotificationQty;
                    _cmd += ",@ServiceGrp =" + maServices[i].ServiceGrp;
                    _cmd += ",@PurchaseNo  ='" + maServices[i].PurchaseNo + "'";
                    _cmd += ",@ReferNo  ='" + maServices[i].ReferNo + "'";
                    _cmd += ",@ProjectName  ='" + maServices[i].ProjectName + "'";
                    _cmd += ",@QuotationNo  ='" + maServices[i].QuotationNo + "'";
                    _cmd += ",@PricePur  ='" + maServices[i].PricePur + "'";
                    _cmd += ",@PriceSale  ='" + maServices[i].PriceSale + "'";

                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        return;

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


        // PUT: api/MAService/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/MAService/5
        public void Delete(string id, int seq)
        {
            string _cmd = "";
            _cmd = "delete from dbo.MAService_Service where  MANo='" + id + "' and Seq=" + seq;
            DB.DBConn.ExecuteOnly(_cmd);
        }
    }
}
