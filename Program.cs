using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using goalongapi.Data;
using goalongapi.Installers;
using goalongapi.Interfaces;
using goalongapi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.InstallServiceInAssembly(builder.Configuration);

// Call UseServiceProviderFactory on the Host sub property 
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Option 1# to Add Service
// builder.Services.AddTransient<IProductService, ProductService>();

// Option 2# to Auto Add Services
builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
{   
    builder.RegisterAssemblyTypes(Assembly.GetEntryAssembly())
    .Where(t=> t.Name.EndsWith("Service"))
    .AsImplementedInterfaces();
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c=> c.SwaggerEndpoint("/swagger/v1/swagger.json", "goalong api"));
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigins");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();