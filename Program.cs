using Microsoft.EntityFrameworkCore;
using PaperCutDash.Data;

var b = WebApplication.CreateBuilder(args);

b.Services.AddDbContext<AppDb>(o => o.UseSqlite("Data Source=runs.db"));
b.Services.AddControllersWithViews();

var app = b.Build();
using (var s = app.Services.CreateScope())
    s.ServiceProvider.GetRequiredService<AppDb>().Database.EnsureCreated();

app.UseStaticFiles();
app.MapControllerRoute("default", "{controller=Runs}/{action=Index}/{id?}");
app.Run();