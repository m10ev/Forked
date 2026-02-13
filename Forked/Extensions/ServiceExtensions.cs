using Forked.Data;
using Forked.Models.Domains;
using Forked.Services;
using Forked.Services.Ingredients;
using Forked.Services.Interfaces;
using Forked.Services.Recipes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace Forked.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddDatabase(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<ForkedDbContext>(options =>
                options.UseSqlServer(
                    config.GetConnectionString("DefaultConnection")));
        }

        public static void AddIdentityServices(this IServiceCollection services)
        {
            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
                .AddEntityFrameworkStores<ForkedDbContext>()
                .AddDefaultTokenProviders();
        }

        public static void AddEmailServices(this IServiceCollection services)
        {
            services.AddTransient<IEmailSender, EmailSender>();
        }

        public static void AddImageServices(this IServiceCollection services)
        {
            services.AddTransient<IImageService, ImageService>();
        }

        public static void AddRecipeServices(this IServiceCollection services)
        {
            services.AddTransient<IRecipeService, RecipeService>();
        }

        public static void AddIngredientServices(this IServiceCollection services)
        {
            services.AddTransient<IIngredientParser, IngredientParser>();
        }
    }
}
