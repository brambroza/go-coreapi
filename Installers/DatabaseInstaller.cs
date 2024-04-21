 
using goalongapi.Data;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Installers
{
    public class DatabaseInstaller : IInstallers
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DatabaseContext>(options =>
                  options.UseSqlServer(configuration.GetConnectionString("ConnectionSQLServer"))
            );

          SystemConfig._ConnectionString  = configuration.GetConnectionString("ConnectionSQLServer");
        }
    }
}
