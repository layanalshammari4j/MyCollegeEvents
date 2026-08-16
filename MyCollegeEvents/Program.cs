using Microsoft.EntityFrameworkCore;
using MyCollegeEvents.Data;
using MyCollegeEvents.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// Add Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

// Add Export Service
builder.Services.AddScoped<IExportService, ExportService>();

// Add Backup Service
builder.Services.AddScoped<IBackupService, BackupService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// كود إنشاء الجداول تلقائياً في قاعدة البيانات عند تشغيل التطبيق
//using (var scope = app.Services.CreateScope())
//{
    //var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    //try
    //{
     //   dbContext.Database.EnsureCreated();
   // }
   // catch (Exception ex)
   // {
    //    Console.WriteLine($"Database creation warning: {ex.Message}");
   // }
//}


app.Run();