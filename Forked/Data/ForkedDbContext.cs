using Forked.Models.Domains;
using Forked.Models.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Forked.Data;

public class ForkedDbContext : IdentityDbContext<User>
{
    public ForkedDbContext(DbContextOptions<ForkedDbContext> options) : base(options)
    {
    }

    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<RecipeStep> RecipeSteps => Set<RecipeStep>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<UserFavoriteRecipe> UserFavoriteRecipes => Set<UserFavoriteRecipe>();
    public DbSet<UserFollow> UserFollows => Set<UserFollow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureRecipe(modelBuilder);
        ConfigureRecipeIngredient(modelBuilder);
        ConfigureIngredient(modelBuilder);
        ConfigureRecipeStep(modelBuilder);
        ConfigureReview(modelBuilder);
        ConfigureUserFavoriteRecipe(modelBuilder);
        ConfigureUserFollow(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasQueryFilter(u => u.DeletedAt == null);

            entity.HasIndex(u => u.DisplayName)
                .IsUnique();

            entity.HasIndex(u => u.Email)
                .IsUnique();
        });
    }

    private static void ConfigureRecipe(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasQueryFilter(r => r.DeletedAt == null);

            entity.Property(r => r.ImagePaths)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                )
                .Metadata.SetValueComparer(new ListStringValueComparer());

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
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(r => r.RecipeSteps)
                .WithOne(rs => rs.Recipe)
                .HasForeignKey(rs => rs.RecipeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureRecipeIngredient(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RecipeIngredient>(entity =>
        {
            entity.HasKey(ri => ri.Id);
            entity.HasQueryFilter(ri => ri.Recipe.DeletedAt == null);

            entity.HasOne(ri => ri.Recipe)
                .WithMany(r => r.RecipeIngredients)
                .HasForeignKey(ri => ri.RecipeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ri => ri.Ingredient)
                .WithMany(i => i.RecipeIngredients)
                .HasForeignKey(ri => ri.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureIngredient(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(i => i.Id);

            entity.HasIndex(i => i.Name)
                .IsUnique();

            entity.HasMany(i => i.RecipeIngredients)
                .WithOne(ri => ri.Ingredient)
                .HasForeignKey(ri => ri.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureRecipeStep(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RecipeStep>(entity =>
        {
            entity.HasKey(rs => rs.Id);
            entity.HasQueryFilter(rs => rs.Recipe.DeletedAt == null);

            entity.Property(rs => rs.ImagePaths)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                )
                .Metadata.SetValueComparer(new ListStringValueComparer());

            entity.HasOne(rs => rs.Recipe)
                .WithMany(r => r.RecipeSteps)
                .HasForeignKey(rs => rs.RecipeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureReview(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasQueryFilter(r => r.DeletedAt == null);

            entity.HasIndex(r => new { r.UserId, r.RecipeId })
                .IsUnique()
                .HasFilter("[DeletedAt] IS NULL");

            entity.Property(r => r.ImagePaths)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                )
                .Metadata.SetValueComparer(new ListStringValueComparer());

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

    private static void ConfigureUserFavoriteRecipe(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserFavoriteRecipe>(entity =>
        {
            entity.HasKey(ufr => new { ufr.UserId, ufr.RecipeId });
            entity.HasQueryFilter(ufr => ufr.DeletedAt == null);

            entity.HasOne(ufr => ufr.User)
                .WithMany(u => u.FavoriteRecipes)
                .HasForeignKey(ufr => ufr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ufr => ufr.Recipe)
                .WithMany(r => r.FavoritedByUsers)
                .HasForeignKey(ufr => ufr.RecipeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureUserFollow(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserFollow>(entity =>
        {
            entity.HasKey(uf => new { uf.FollowerId, uf.FollowingId });
            entity.HasQueryFilter(uf => uf.DeletedAt == null);

            entity.HasOne(uf => uf.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(uf => uf.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(uf => uf.Following)
                .WithMany(u => u.Followers)
                .HasForeignKey(uf => uf.FollowingId)
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

// Value comparer for List<string> to properly track changes
public class ListStringValueComparer : ValueComparer<List<string>>
{
    public ListStringValueComparer()
        : base(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList())
    {
    }
}