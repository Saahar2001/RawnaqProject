using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RawnaqProject.Data;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<RawnaqProjectContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RawnaqProjectContext") ?? throw new InvalidOperationException("Connection string 'RawnaqProjectContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(20); });

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();


app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=usersaccounts}/{action=Login}/{id?}");

/// add session
app.UseSession();

app.Run();
