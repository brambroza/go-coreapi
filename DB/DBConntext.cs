using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace coreapi.DB
{
    public class DBConntext
    {
        public string getConnectionString()
        {
            string strcon = "Server=localhost,1433;user id=sa; password=dr0wss@p; Database=goalongdatabase; TrustServerCertificate=true;";
            //create new sqlconnection and connection to database by using connection string from web.config file  
            return strcon;
        }
    }

    public class DBConntextSystem
    {
        public string getConnectionString()
        {
            string strcon = "Server=localhost,1433;user id=sa; password=dr0wss@p; Database=goalongdatabase; TrustServerCertificate=true;";
            //create new sqlconnection and connection to database by using connection string from web.config file  
            return strcon;
        }
    }


}