using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ChatApp.Data;
using ChatApp.Areas.Identity.Data;
using ChatApp.Models;


var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ChatAppContextConnection") ?? throw new InvalidOperationException("Connection string 'ChatAppIdentityContextConnection' not found.");

builder.Services.AddDbContext<ChatAppIdentityContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDbContext<ChatContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ChatAppUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ChatAppIdentityContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
//builder.Services.AddProgressiveWebApp();
builder.Services.AddRazorPages();
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
app.UseAuthentication();;

app.UseAuthorization();


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Messages}/{action=Index}/{id?}");
    endpoints.MapRazorPages();
   // endpoints.MapBlazorHub();
   // endpoints.MapHub<ChatHub>("/chatHub");
});
app.Run();
