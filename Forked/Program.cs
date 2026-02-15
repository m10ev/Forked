using Forked.Data;
using Forked.Extensions;
using Forked.Models.Domains;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddIdentityServices();
builder.Services.AddImageServices();
builder.Services.AddRecipeServices();
builder.Services.AddReviewServices();
builder.Services.AddUserServices();
builder.Services.AddIngredientServices();
builder.Services.AddFavoriteServices();
builder.Services.AddRazorPages();
builder.Services.AddEmailServices();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    if (app.Environment.IsDevelopment())
    {
        // In dev: wipe and reseed everything (roles + admin included)
        await DatabaseClearer.ClearAsync(services);
        await DatabaseSeeder.SeedAsync(services);
    }
    else
    {
        // In production: only ensure roles + admin exist, never wipe data
        await RoleSeeder.SeedRolesAndAdminAsync(services);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();