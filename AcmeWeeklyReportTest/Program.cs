using AcmeWeeklyReportTest.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ✅ EF Core DbContext（從 appsettings.json 讀連線字串）
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Cookie Authentication（登入後才可用 [Authorize]）
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";         // 未登入導到登入頁
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";
        //options.ExpireTimeSpan = TimeSpan.FromMinutes(1); //cookie存在時間
        options.ExpireTimeSpan=TimeSpan.FromHours(1);
        options.SlidingExpiration = true;//有操作自動繼續此cookie
    });
Console.WriteLine(">>> ENV DefaultConnection=" + Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection"));
Console.WriteLine(">>> CFG DefaultConnection=" + builder.Configuration.GetConnectionString("DefaultConnection"));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// ✅ 靜態檔案
app.UseStaticFiles();

app.UseRouting();

// ✅ 一定要先 Authentication 再 Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
