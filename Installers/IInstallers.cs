namespace goalongapi.Installers
{
    public interface IInstallers
    {
         void InstallServices(IServiceCollection services, IConfiguration configuration);
    }
}