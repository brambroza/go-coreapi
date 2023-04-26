using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Timers;
using System.Threading;
using System.Text;
using System.IO;
using System.Data; 

namespace coreapi.Controllers
{
    
    public class LineNotiController : ApiController
    {
         
        
      
        // GET: api/LineNoti
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/LineNoti/5
        public string Get(string id)
        {
            string _cmd = "";
            _cmd = "exec  dbo.sp_getNotima ";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            StringBuilder _msg ;

            foreach (DataRow r in dt.Rows)
            {
                _msg = new StringBuilder();
                _msg.Append("");
                _msg.AppendLine();
                _msg.Append("ลูกค้า : " + r["CustCode"].ToString());
                _msg.AppendLine();
                _msg.Append("ชื่อโปรเจค : " + r["ProjectName"].ToString());
                _msg.AppendLine();
                _msg.Append("เลขที่สัญญา : " + r["ReferNo"].ToString());
                _msg.AppendLine();
                _msg.Append("กลุ่ม : " + r["GrpService"].ToString());
                _msg.AppendLine();
                _msg.Append("รายละเอียด : " + r["Description"].ToString());
                _msg.AppendLine();
                _msg.Append("วันหมดอายุ :" + r["ExpireDate"].ToString());


                lineNotify(_msg.ToString());
            }

            rcvnotify();



            return "value";
        }

        private void rcvnotify ()
        {
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.sp_getNotimaRMRcvInstall ";
                DataTable dt = DB.DBConn.GetDataTable(_cmd);
                StringBuilder _msg;

                foreach (DataRow r in dt.Rows)
                {
                    _msg = new StringBuilder();
                    _msg.Append("");
                    _msg.AppendLine();
                    _msg.Append("เลขที่โปรเจค : " + r["ProjectNo"].ToString());
                    _msg.AppendLine();
                    _msg.Append("ชื่อลูกค้า : " + r["CustomerName"].ToString());                 
                    _msg.AppendLine();
                    _msg.Append("เลขที่ใบรับ : " + r["ReceiveNo"].ToString());
                    _msg.AppendLine();
                    _msg.Append("ชื่อผู้รับ : " + r["ReceiveBy"].ToString());
                    _msg.AppendLine();
                    _msg.Append("วันที่รับ : " + r["ReceiveDate"].ToString());
                    _msg.AppendLine();
                    _msg.Append("รหัสสินค้า : " + r["ProductCode"].ToString());
                    _msg.AppendLine();
                    _msg.Append("รายละเอียด : " + r["ProdDescription"].ToString());
                    _msg.AppendLine();
                    _msg.Append("จำนวน : " + r["Qty"].ToString() + " " + r["UnitCode"].ToString());

                  


                    lineNotifyRcv(_msg.ToString());
                }

            }
            catch
            {

            }

        }



        private void lineNotify(string msg)
        {
            string token = "8LtACGcDqZS6ZouELpfLZPc8Trl6LWgbEErI0pgjSeg";
            token = "0mNuIHnaDbNaSeY5Wih1uYAvtOdouKZnIg9uYMyKKMc";
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://notify-api.line.me/api/notify");
                var postData = string.Format("message={0}", msg);
                var data = Encoding.UTF8.GetBytes(postData);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                request.Headers.Add("Authorization", "Bearer " + token);

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void lineNotifyRcv(string msg)
        {
            string token = "8LtACGcDqZS6ZouELpfLZPc8Trl6LWgbEErI0pgjSeg";
            token = "W8n3v5Aj4wvXM7MYJgDCApPpITKt0dM6SBTGxhxRhAA";
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://notify-api.line.me/api/notify");
                var postData = string.Format("message={0}", msg);
                var data = Encoding.UTF8.GetBytes(postData);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                request.Headers.Add("Authorization", "Bearer " + token);

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }




        // POST: api/LineNoti
        public void Post(DateTime Sdate , DateTime EDate)
        {
         
           

        }

        // PUT: api/LineNoti/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/LineNoti/5
        public void Delete(int id)
        {
        }
        bool ranDailyToday = false;
        // Run every hour i.e.
    




    }
}
