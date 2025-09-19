using FanucRelease.Data;
using FanucRelease.Services;
using FanucRelease.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddHostedService<RobotTcpListenerService>();

// 🔐 AUTH
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
    // Burayı /Home/Error yapıyoruz, çünkü Error action genelde HomeController’da.
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// SignalR Hub endpoint
app.MapHub<RobotStatusHub>("/robotStatusHub");

// Routes (önce Areas, sonra Default)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}"); // ⬅️ Root = Login

app.Run();
