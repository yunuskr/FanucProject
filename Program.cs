using FanucRelease.Data;
using FanucRelease.Services;
using FanucRelease.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Uygulamayı aynı ağdan erişilebilir hale getir
builder.WebHost.UseUrls(
    "http://localhost:5000",
    "http://0.0.0.0:5000"
);

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVC
builder.Services.AddControllersWithViews();

// DI
builder.Services.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));
builder.Services.AddScoped<IProgramVerisiService, ProgramVerisiService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IKaynakService, KaynakService>();
builder.Services.AddScoped<IHataService, HataService>();
builder.Services.AddSingleton<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IOperatorService, OperatorService>();
builder.Services.AddScoped<IAnlikKaynakService, AnlikKaynakService>();
builder.Services.AddHostedService<RobotTcpListenerService>();

// AUTH
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.LoginPath = "/Login/Index";
        opt.AccessDeniedPath = "/Login/Index";
        opt.SlidingExpiration = true;
        opt.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR();

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Aynı ağda test için önce HTTP kullan
// app.UseHttpsRedirection();

app.UseStaticFiles();

// Serve icons folder from project root at /icons
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "icons")),
    RequestPath = "/icons"
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// SignalR Hub endpoint
app.MapHub<RobotStatusHub>("/robotStatusHub");

// Routes
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();