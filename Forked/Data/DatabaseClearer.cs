// ============================================================
//  DatabaseClearer.cs — Forked App
//
//  Usage in Program.cs (behind a dev environment guard):
//    await DatabaseClearer.ClearAsync(services);
//
//  Uses raw SQL TRUNCATE / DELETE so there is zero dependency
//  on EF change tracking or UserManager state.
// ============================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Forked.Data;

public static class DatabaseClearer
{
    public static async Task ClearAsync(IServiceProvider serviceProvider)
    {
        var db = serviceProvider.GetRequiredService<ForkedDbContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<ClearerMarker>>();

        logger.LogWarning("🗑️  Clearing entire database with raw SQL…");

        // Execute every statement individually so a failure is easy to diagnose.
        // Order is strictly child-before-parent to avoid FK violations.
        var statements = new[]
        {
            // ── App tables (children first) ───────────────────
            "DELETE FROM [dbo].[UserFavoriteRecipes]",
            "DELETE FROM [dbo].[UserFollows]",
            "DELETE FROM [dbo].[Reviews]",
            "DELETE FROM [dbo].[RecipeIngredients]",
            "DELETE FROM [dbo].[RecipeSteps]",

            // Null out self-referencing FK before deleting recipes
            "UPDATE [dbo].[Recipes] SET [ParentRecipeId] = NULL WHERE [ParentRecipeId] IS NOT NULL",
            "DELETE FROM [dbo].[Recipes]",
            "DELETE FROM [dbo].[Ingredients]",

            // ── ASP.NET Identity tables (children first) ──────
            "DELETE FROM [dbo].[AspNetUserTokens]",
            "DELETE FROM [dbo].[AspNetUserLogins]",
            "DELETE FROM [dbo].[AspNetUserClaims]",
            "DELETE FROM [dbo].[AspNetUserRoles]",
            "DELETE FROM [dbo].[AspNetRoleClaims]",
            "DELETE FROM [dbo].[AspNetUsers]",
            "DELETE FROM [dbo].[AspNetRoles]",

            // ── Reseed integer identity counters ──────────────
            "DBCC CHECKIDENT ('Ingredients',      RESEED, 0)",
            "DBCC CHECKIDENT ('Recipes',           RESEED, 0)",
            "DBCC CHECKIDENT ('RecipeSteps',       RESEED, 0)",
            "DBCC CHECKIDENT ('RecipeIngredients', RESEED, 0)",
            "DBCC CHECKIDENT ('Reviews',           RESEED, 0)",
        };

        foreach (var sql in statements)
        {
            logger.LogDebug("   SQL: {Sql}", sql);
            await db.Database.ExecuteSqlRawAsync(sql);
        }

        logger.LogWarning("✅  Database cleared — ready to reseed.");
    }

    private class ClearerMarker { }
}