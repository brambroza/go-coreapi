using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using goalongapi.Data;
using goalongapi.Installers;
// using goalongapi.Interfaces;
// using goalongapi.Services;
// using Microsoft.EntityFrameworkCore;



System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(p => p.AddPolicy("_MyAllowSpecificOrigins", builder =>
{
  builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));

// Add services to the container.
builder.Services.InstallServiceInAssembly(builder.Configuration);


/// google auth
/// 

/*   var services = builder.Services;
var configuration = builder.Configuration; 
services.AddAuthentication().AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = "1096508373254-n72sf3a8ems41i43psu14dcsl9lifioa.apps.googleusercontent.com" ; // configuration["Authentication:Google:ClientId"];
        googleOptions.ClientSecret = "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";//configuration["Authentication:Google:ClientSecret"];
    }); */


///end google auth 




// Call UseServiceProviderFactory on the Host sub property 
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Option 1# to Add Service
// builder.Services.AddTransient<IProductService, ProductService>();

// Option 2# to Auto Add Services
builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
{
  builder.RegisterAssemblyTypes(Assembly.GetEntryAssembly())
  .Where(t => t.Name.EndsWith("Service"))
  .AsImplementedInterfaces();
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "goalong api"));
}


// else
// {
//     app.UseSwagger();
//     app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "goalong api"));
// }



app.UseCors("_MyAllowSpecificOrigins");
app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();