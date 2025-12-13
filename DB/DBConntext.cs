using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Data.SqlClient;

namespace goalongapi.DB
{
    public class DBConntext
    {
        public string getConnectionString()
        {
            string appset = "appsettings.json";
            //   string appset = "appsettings.Development.json";
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(appset);

            IConfigurationRoot configuration = builder.Build();
            string connectionString = configuration.GetConnectionString("ConnectionSQLServer");

            // string strcon = "Server=localhost,1433;user id=sa; password=dr0wss@p; Database=goalongdatabase; TrustServerCertificate=true;";
            //   strcon = "Server=192.168.1.105,1433;user id=sa; password=1234; Database=goalongdatabase; TrustServerCertificate=true;";
            //create new sqlconnection and connection to database by using connection string from web.config file
            return connectionString;
        }
    }

    public class DBConntextSystem
    {
        public string getConnectionString()
        {
            string appset = "appsettings.json";
            //string appset = "appsettings.Development.json";
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(appset);

            IConfigurationRoot configuration = builder.Build();
            string connectionString = configuration.GetConnectionString("ConnectionSQLServer");

            // string strcon = "Server=localhost,1433;user id=sa; password=dr0wss@p; Database=goalongdatabase; TrustServerCertificate=true;";
            //   strcon = "Server=192.168.1.105,1433;user id=sa; password=1234; Database=goalongdatabase; TrustServerCertificate=true;";
            //create new sqlconnection and connection to database by using connection string from web.config file
            return connectionString;
        }
    }

    public class DbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(IConfiguration configuration)
        {
            // ใช้ชื่อเดียวกับใน appsettings.json
            _connectionString = configuration.GetConnectionString("ConnectionSQLServer")
                                ?? throw new InvalidOperationException(
                                    "Connection string 'ConnectionSQLServer' not found.");
        }

        public SqlConnection CreateConnection()
        {
            // ที่เหลือให้คนเรียกเป็นคน open/close เอง
            return new SqlConnection(_connectionString);
        }
    }
}
