using Forked.Models.Domains;
using Forked.Models.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

public class ForkedDbContext(DbContextOptions<ForkedDbContext> options) : IdentityDbContext<User>(options)
{
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasQueryFilter(u => u.DeletedAt == null);

        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasQueryFilter(r => r.DeletedAt == null);

            entity.OwnsOne(r => r.ImagePaths, builder => { builder.ToJson(); });

            entity.HasOne(r => r.ParentRecipe)
                  .WithMany(r => r.Forks)
                  .HasForeignKey(r => r.ParentRecipeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Author)
                  .WithMany(u => u.Recipes)
                  .HasForeignKey(r => r.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(r => r.RecipeIngredients)
                  .WithOne(ri => ri.Recipe)
                  .HasForeignKey(ri => ri.RecipeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Review>(entity => {
            entity.HasKey(r => r.Id);
            entity.HasQueryFilter(r => r.DeletedAt == null);

            entity.HasIndex(r => new { r.UserId, r.RecipeId })
                  .IsUnique()
                  .HasFilter("[DeletedAt] IS NULL");

            entity.OwnsOne(p => p.ImagePaths, builder => { builder.ToJson(); });

            entity.HasOne(r => r.User)
                  .WithMany(u => u.Reviews)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Recipe)
                  .WithMany(rec => rec.Reviews)
                  .HasForeignKey(r => r.RecipeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<IAuditable>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}