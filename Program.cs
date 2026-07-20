using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using goalongapi.Hubs;
using goalongapi.Data;
using goalongapi.Installers;
using goalongapi.DB;
using Microsoft.EntityFrameworkCore;


System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddCors(p =>
    p.AddPolicy(
        "_MyAllowSpecificOrigins",
        builder =>
        {
            /*   builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();   */


            builder
               .WithOrigins(
                  "https://erp.nisolution.co.th", "https://app.nisolution.co.th",
                  "http://nisolution.fortiddns.com:8284",
                  "http://localhost:8080", "http://192.168.1.179:8080",
                   "http://localhost:8081",
                  "http://192.168.55.219:8285", "http://10.0.2.2:8000",
                  "http://127.0.0.1:51052", "https://liff.line.me",
                  "http://127.0.0.1:65060", "http://127.0.0.1:9101", // simulator ios
                  "http://localhost:5173", "http://localhost:5174"   // mockup-nis Vite dev
              )

               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
        }
    )
);

builder.Services.AddScoped<DbConnectionFactory>();

// Add services to the container.
builder.Services.InstallServiceInAssembly(builder.Configuration);

builder.Services.AddSingleton<RabbitMQService>();

builder.Services.AddHostedService<LogProcessorService>();
builder.Services.AddScoped<EmailSettingRepository>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connStr = config.GetConnectionString("ConnectionSQLServer");
    return new EmailSettingRepository(connStr!);
});

builder.Services.AddSingleton<AesCrypto>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new AesCrypto(config["EmailCrypto:KeyBase64"]!);
});

// Pooled HttpClient factory — avoids socket exhaustion from `new HttpClient()`
// per request in the Google OAuth mail/calendar services.
builder.Services.AddHttpClient();

// NIS Onsite push (Track B) — Expo Push sender + overdue watcher (วันละครั้ง/ตั๋ว ทุก 15 นาที)
builder.Services.AddScoped<goalongapi.Services.ExpoPushService>();
builder.Services.AddHostedService<goalongapi.Services.NisOverduePushService>();
builder.Services.AddScoped<goalongapi.Helpers.GoogleCalendarApiKeyClient>();
builder.Services.AddDataProtection();
builder.Services.AddScoped<goalongapi.Helpers.GoogleOAuthMailService>();
builder.Services.AddScoped<goalongapi.Helpers.GoogleCalendarEventMappingRepository>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new goalongapi.Helpers.GoogleCalendarEventMappingRepository(config.GetConnectionString("ConnectionSQLServer")!);
});
builder.Services.AddScoped<goalongapi.Helpers.GoogleOAuthCalendarService>();
// NIS Onsite — persists client-generated Service Report PDFs (blob + sha256) for attach/resend/audit.
builder.Services.AddScoped<goalongapi.Helpers.NisReportPdfStorage>();

// Cap request body so a large base64 PDF (+ photos) fails as a controlled 413 instead of an
// obscure connection reset. Kestrel default (~28.6 MB) is otherwise never overridden in this repo.
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(o =>
{
    o.Limits.MaxRequestBodySize = builder.Configuration.GetValue<long?>("NisOnsite:MaxRequestBodyBytes") ?? 32L * 1024 * 1024;
});


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
    builder
        .RegisterAssemblyTypes(Assembly.GetEntryAssembly())
        .Where(t => t.Name.EndsWith("Service"))
        .AsImplementedInterfaces();
});

builder.Services.AddDbContext<HrDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionSQLServer"))
);




var app = builder.Build();
/* 
app.UseMiddleware<DuplicateRouteNameMiddleware>(); */
//if (app.Environment.IsDevelopment())
//{


app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "goalong api"));

// }
app.UseCors("_MyAllowSpecificOrigins");
app.UseStaticFiles();
// Dev: ไม่ redirect HTTP→HTTPS เพื่อให้ iPad/มือถือยิง http://<LAN-IP>:5052 ได้ตรง
// (HTTPS 7046 เป็น self-signed cert ที่ Expo Go ไม่ trust) · prod ยัง redirect ปกติ
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

/* app.MapControllers(); */
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    // SignalR
    endpoints.MapHub<NotificationHub>("/notificationhub");
    endpoints.MapHub<TicketTaskReplyHub>("/tickettaskreplyhub");
    endpoints.MapHub<TicketCommentHub>("/ticketcommenthub");
    endpoints.MapHub<ChatHub>("/chathub");
    endpoints.MapHub<SessionHub>("/sessionhub");
    endpoints.MapHub<DispatchKanbanHub>("/dispatchkanbanhub");

});




app.Run();
