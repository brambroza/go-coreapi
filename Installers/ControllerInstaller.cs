namespace goalongapi.Installers
{
    public class ControllerInstaller : IInstallers
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();           
        }
    }
}