using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using goalongapi.Hubs;
using goalongapi.Data;
using goalongapi.Installers;
using goalongapi.DB;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;


System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
// SignalR — ใช้ Redis backplane เมื่อตั้ง Redis:Configuration (จำเป็นเมื่อรันมากกว่า 1 replica)
// ค่าว่าง = in-process เหมือนเดิม (dev ไม่ต้องรัน Redis)
var signalRBuilder = builder.Services.AddSignalR();
var redisConfiguration = builder.Configuration["Redis:Configuration"];
var redisChannelPrefix = builder.Configuration["Redis:ChannelPrefix"] ?? "goalongapi";
var useRedisBackplane = !string.IsNullOrWhiteSpace(redisConfiguration);
if (useRedisBackplane)
{
    signalRBuilder.AddStackExchangeRedis(redisConfiguration!, options =>
    {
        options.Configuration.ChannelPrefix = RedisChannel.Literal(redisChannelPrefix);
        // Redis ล่มตอน start → app ยังขึ้น แล้ว reconnect เองเมื่อ Redis กลับมา
        options.Configuration.AbortOnConnectFail = false;
    });
}
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
                  "http://localhost:5173", "http://localhost:5174" , "http://192.168.88.11:8284"   // mockup-nis Vite dev
              )

               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
        }
    )
);

builder.Services.AddScoped<DbConnectionFactory>();
// distributed lock (sp_getapplock) ให้ BackgroundService ไม่รันพร้อมกันข้าม replica (job ต้อง idempotent) — singleton ไม่ auto-register
builder.Services.AddSingleton<goalongapi.Services.SqlAppLock>();

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
// NIS Onsite — auto-close ตั๋วเคส Accident หลังโปรเจคหมดสัญญา (เช็ควันละครั้ง, bypass ขั้นตอนอนุมัติ)
builder.Services.AddHostedService<goalongapi.Services.NisAccidentAutoCloseService>();
// NIS Service Report — archive sync: คัดลอกเคสจาก SP getProblem* → NisServiceCase ทุก NisReport:SyncEveryMinutes นาที
// ครอบด้วย SqlAppLock (goalongapi:nis-archive-sync) · singleton ตัวเดียวใช้ทั้ง hosted loop และ endpoint sync/run
// (ถือ fail streak / backoff ในหน่วยความจำ) — register ครั้งเดียวผ่าน factory ไม่ให้ได้ worker ซ้อน
builder.Services.AddSingleton<goalongapi.Services.NisArchiveSyncService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<goalongapi.Services.NisArchiveSyncService>());
// NIS Service Report — service ที่ไม่มี interface ต้อง register เอง (Autofac scan ด้านล่าง register AsImplementedInterfaces เท่านั้น)
builder.Services.AddScoped<goalongapi.Services.NisProblemSourceService>();
builder.Services.AddScoped<goalongapi.Services.NisCustomerGroupResolverService>();
builder.Services.AddScoped<goalongapi.Services.NisHolidayService>();
builder.Services.AddScoped<goalongapi.Services.INisArchiveService, goalongapi.Services.NisArchiveService>();
// NIS Service Report — report builder (api/nis/report): ข้อมูลบริษัท · template · asset · รายงาน + snapshot/gates
// ไม่มี interface จึง register เอง (Autofac scan register AsImplementedInterfaces เท่านั้น)
builder.Services.AddScoped<goalongapi.Helpers.NisReportAssetStorage>();
builder.Services.AddScoped<goalongapi.Services.NisReportCompanyService>();
builder.Services.AddScoped<goalongapi.Services.NisReportTemplateService>();
builder.Services.AddScoped<goalongapi.Services.NisReportSnapshotService>();
builder.Services.AddScoped<goalongapi.Services.NisReportService>();
// ตัวเรนเดอร์ PDF — Playwright/Chromium (NisReportPdfRenderer) · singleton เพราะถือ IPlaywright/IBrowser ตัวเดียวใช้ซ้ำทั้งโปรเซส
// (คิว resolve ผ่าน scope ได้ตามปกติ — singleton ที่ resolve จาก scope คือ instance เดียวกัน) · ต้องมี Chromium ในเครื่อง/ภาพ
// ไม่งั้นงานล้มพร้อมข้อความบอกวิธีติดตั้ง (docs/nsr-pdf-renderer.md) · ตัวสำรองเดิม NisReportPdfRendererUnavailable ยังอยู่ สลับกลับได้บรรทัดนี้
builder.Services.AddSingleton<goalongapi.Services.INisReportPdfRenderer, goalongapi.Services.NisReportPdfRenderer>();
// คิวเรนเดอร์ PDF — singleton ตัวเดียวใช้ทั้ง hosted loop และ endpoint generate/job · ครอบด้วย SqlAppLock (goalongapi:nis-report-pdf)
builder.Services.AddSingleton<goalongapi.Services.NisReportPdfQueueService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<goalongapi.Services.NisReportPdfQueueService>());
// NIS Service Report — analytics (summary/trend/breakdown/heatmap) · กลุ่มลูกค้า (ตั้งค่า+overview) · effort · kb · workload (E11–E19)
builder.Services.AddScoped<goalongapi.Services.INisAnalyticsService, goalongapi.Services.NisAnalyticsService>();
builder.Services.AddScoped<goalongapi.Services.NisCustomerGroupService>();
builder.Services.AddScoped<goalongapi.Services.NisEffortService>();
builder.Services.AddScoped<goalongapi.Services.NisKbService>();
builder.Services.AddScoped<goalongapi.Services.NisWorkloadService>();
// NIS Onsite realtime bridge — best-effort POST → go-chat-api emits socket.io nis:notify
// (foreground refresh, complements ExpoPushService for background/killed app)
builder.Services.AddScoped<goalongapi.Services.NisRealtimeNotifyService>();
// NIS → กระดิ่ง CRM (SystemNotification + SignalR NotificationHub) — ใช้ตอนช่างกดรับงาน
builder.Services.AddScoped<goalongapi.Services.NisCrmNotifyService>();
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
        // Hosted services are already registered explicitly with AddHostedService.
        // Registering them again here creates duplicate background workers.
        .Where(t => t.Name.EndsWith("Service") && !typeof(IHostedService).IsAssignableFrom(t))
        .AsImplementedInterfaces();
});

builder.Services.AddDbContext<HrDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionSQLServer"))
);




var app = builder.Build();

if (useRedisBackplane)
    app.Logger.LogInformation("SignalR backplane: Redis (ChannelPrefix={Prefix})", redisChannelPrefix);
else
    app.Logger.LogWarning("SignalR backplane: in-process only — ห้ามรันเกิน 1 replica (ตั้ง Redis:Configuration เพื่อเปิด Redis backplane)");
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
