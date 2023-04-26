using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Microsoft.VisualBasic;

namespace coreapi.Tool
{
    public class Tool
    {

        public static string validateStr(string tmpStr)
        {

            if (!string.IsNullOrEmpty(tmpStr))
            {

                return tmpStr.Replace("'", "''");
            }
            else
            {
                return tmpStr;
            }

        }


        public static string ConvertDate(object DataDate)
        {
            string strDate = "";

            try
            {
                strDate = DataDate.ToString(); // validate(DataDate);

                if ((Convert.ToInt32(strDate.Substring(7, 4)) > 0) & (Convert.ToInt32(strDate.Substring(7, 4)) > (2300)))
                {
                    strDate = (strDate .Substring  ( 1, 2)) + "/" + ((strDate.Substring ( 4, 2)) + "/" + (Convert.ToInt32(strDate.Substring(7, 4)) - 543).ToString("0000"));
                }
                else
                {
                    strDate = (strDate.Substring(1, 2)) + "/" + (strDate.Substring(4, 2)) + "/" + (strDate.Substring(7, 4));
                }

                strDate = strDate.Substring(7, 4) + "/" + strDate.Substring(4, 2) + "/" + strDate.Substring(1, 2);
            }
            catch //(Exception ex)
            {
                strDate = "";
            }


            return strDate;

        }

        public static string validate(DateTime Obj)
        {
            try
            {
                CultureInfo _Culture = new CultureInfo("en-US", true);
                _Culture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
                _Culture.DateTimeFormat.ShortTimePattern = "HH:mm:ss";
              
                System.Threading.Thread.CurrentThread.CurrentCulture = _Culture;
                System.Threading.Thread.CurrentThread.CurrentUICulture = _Culture;
                 

                string _Date = "";
               
                _Date = Obj.ToString("yyyy-MM-dd");
                return _Date;
            }
            catch //(Exception ex)
            {
                return "";
            }
        }

        public static string validatestring(string Obj)
        {
            try
            {
                CultureInfo _Culture = new CultureInfo("en-US", true);
                _Culture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
                _Culture.DateTimeFormat.ShortTimePattern = "HH:mm:ss";

                System.Threading.Thread.CurrentThread.CurrentCulture = _Culture;
                System.Threading.Thread.CurrentThread.CurrentUICulture = _Culture;


                string _Date = "";

                _Date = Obj.Substring(6, 4) + "/" + Obj.Substring(3, 2) + "/" + Obj.Substring(0, 2);
                return _Date;
            }
            catch //(Exception ex)
            {
                return "";
            }
        }



    }
}