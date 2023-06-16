using CustomerApp.Data;
using CustomerApp.Data.Stripe;
using CustomerApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stripe;

var builder = WebApplication.CreateBuilder(args);


// Load User Secrets
builder.Configuration.AddUserSecrets<Program>();

//DbContext configuration
builder.Services.AddDbContext<AppDBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString")));
// Add services to the container.

builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AppDBContext>();

// Stripe configuration
var stripeSettings = builder.Configuration.GetSection("Stripe").Get<StripeSettings>();
StripeConfiguration.ApiKey = stripeSettings.SecretKey;
builder.Services.AddSingleton(stripeSettings);


builder.Services.AddControllersWithViews();

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

app.Run();
