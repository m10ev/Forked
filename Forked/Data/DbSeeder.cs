// ============================================================
//  DatabaseSeeder.cs — Forked App
//  EF Core + MSSQL | Bogus (Faker) for realistic data
//
//  NuGet dependency:
//    <PackageReference Include="Bogus" Version="35.*" />
//
//  Register in Program.cs:
//    await DatabaseSeeder.SeedAsync(app.Services);
//
//  What gets seeded:
//    • 1 admin + 4 named test users  (fixed credentials below)
//    • 200 community users
//    • 40 canonical ingredients
//    • 800 recipes  (25% are forks of earlier recipes)
//    • 2–7 steps per recipe, 2–8 ingredients per recipe
//    • 2 000 reviews  (with weighted ratings + image URLs)
//    • 1 000 follow relationships
//    • 3 000 favourites
//    • Realistic Unsplash image URLs on recipes, steps & reviews
// ============================================================

using Bogus;
using Forked.Models.Domains;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Forked.Data;

public static class DatabaseSeeder
{
    // ── Volume knobs ─────────────────────────────────────────
    private const int RngSeed = 42;
    private const int CommunityUsers = 200;
    private const int RecipeCount = 800;
    private const int ReviewCount = 2_000;
    private const int FavoriteCount = 3_000;
    private const int FollowCount = 1_000;
    // ─────────────────────────────────────────────────────────

    // ── Fixed test accounts ───────────────────────────────────
    //  Email                  | UserName  | DisplayName      | Password      | Role
    private static readonly (string Email, string UserName, string Display, string Password, string Role)[] TestUsers =
    {
        ("admin@forked.dev",   "admin",   "Admin",          "Admin@123!",  "Admin"),
        ("alice@forked.dev",   "alice",   "Alice Foodie",   "Test@123!",   "User"),
        ("bob@forked.dev",     "bob",     "Bob The Chef",   "Test@123!",   "User"),
        ("charlie@forked.dev", "charlie", "Charlie Eats",   "Test@123!",   "User"),
        ("diana@forked.dev",   "diana",   "Diana Bakes",    "Test@123!",   "User"),
    };

    // ── Unsplash photo IDs for food imagery ───────────────────
    private static readonly string[] FoodPhotoIds =
    {
        "1495521939206-a4ecd0274307","1546069901-5ec6a79120b0","1565299624946-b28f40a0ae38",
        "1567620905732-2d1ec7ab7445","1574484284002-952d92456975","1476718406336-bb5a9690ee2a",
        "1512621776951-a57141f2eefd","1504674900247-0877df9cc836","1494390248081-4e521a5940db",
        "1540189549336-e6e99eb4b9d5","1565958011703-44f9829ba187","1484723091739-30e46f26d3cc",
        "1432139555190-58524dae6a55","1414235077428-338989a2e8c0","1482049016688-2d3e1b311543",
        "1490645935967-10de6ba17061","1473093226795-af9932fe5856","1498837167922-ddd27525d352",
        "1555939594-58d7cb561ad1","1567364711916-b7c08e5e0b87","1571091718767-18b5b1457add",
        "1551218808-bf25b68ad7e2","1583608354155-5c7b0e06f0f8","1519984388953-d2406bc725e1",
        "1607532941433-304659e8198a","1569050467447-ce54b3bbc37d","1559181567-c3190d722f1c",
        "1568901346375-23c9450c58cd","1525059696034-4fedee65f911","1510693206972-6f81e0cb126f",
    };

    private static readonly string[] ProfilePhotoIds =
    {
        "1535713875002-d1d0cf377fde","1494790108377-be9c29b29330","1527980965255-d3b416303d12",
        "1438761681033-6461ffad8d80","1507003211169-0a1dd7228f2d","1580489944761-15a19d654956",
        "1633332755192-727a05c4013d","1570295999919-56ceb5ecca61","1472099645785-5658abf4ff4e",
        "1544005313-94ddf0286df2","1552058544-f2b08422138a","1547425260-76bcadfb4f2c",
    };

    private static string FoodUrl(string id, int w = 800) => $"https://images.unsplash.com/photo-{id}?w={w}&q=80";
    private static string AvatarUrl(string id) => $"https://images.unsplash.com/photo-{id}?w=200&q=80&fit=crop&crop=face";

    // ── Ingredient master list ────────────────────────────────
    private static readonly (string Name, string[] Units)[] IngredientData =
    {
        ("All-Purpose Flour",    new[]{"g","cup","tbsp"}),
        ("Butter",               new[]{"g","tbsp","cup"}),
        ("Eggs",                 new[]{"whole","large","medium"}),
        ("Whole Milk",           new[]{"ml","cup","tbsp"}),
        ("Granulated Sugar",     new[]{"g","cup","tbsp","tsp"}),
        ("Brown Sugar",          new[]{"g","cup","tbsp"}),
        ("Salt",                 new[]{"tsp","tbsp","pinch"}),
        ("Black Pepper",         new[]{"tsp","tbsp","pinch"}),
        ("Olive Oil",            new[]{"ml","tbsp","cup"}),
        ("Garlic",               new[]{"cloves","tsp","tbsp"}),
        ("Onion",                new[]{"medium","large","small","g"}),
        ("Tomatoes",             new[]{"g","cup","whole"}),
        ("Chicken Breast",       new[]{"g","lb","piece"}),
        ("Ground Beef",          new[]{"g","lb"}),
        ("Pasta",                new[]{"g","cup","oz"}),
        ("Rice",                 new[]{"g","cup"}),
        ("Potatoes",             new[]{"g","medium","large"}),
        ("Carrots",              new[]{"g","medium","cup"}),
        ("Celery",               new[]{"g","stalks","cup"}),
        ("Bell Pepper",          new[]{"whole","g","cup"}),
        ("Spinach",              new[]{"g","cup","handful"}),
        ("Lemon",                new[]{"whole","juice of","tbsp"}),
        ("Parmesan Cheese",      new[]{"g","cup","tbsp"}),
        ("Heavy Cream",          new[]{"ml","cup","tbsp"}),
        ("Chicken Broth",        new[]{"ml","cup"}),
        ("Soy Sauce",            new[]{"tbsp","tsp","ml"}),
        ("Honey",                new[]{"tbsp","tsp","ml"}),
        ("Baking Powder",        new[]{"tsp","tbsp"}),
        ("Baking Soda",          new[]{"tsp","tbsp"}),
        ("Vanilla Extract",      new[]{"tsp","tbsp"}),
        ("Cinnamon",             new[]{"tsp","tbsp","pinch"}),
        ("Paprika",              new[]{"tsp","tbsp"}),
        ("Cumin",                new[]{"tsp","tbsp"}),
        ("Chili Flakes",         new[]{"tsp","pinch"}),
        ("Fresh Basil",          new[]{"g","cup","leaves"}),
        ("Fresh Parsley",        new[]{"g","cup","tbsp"}),
        ("Mushrooms",            new[]{"g","cup","whole"}),
        ("Zucchini",             new[]{"g","medium","cup"}),
        ("Cheddar Cheese",       new[]{"g","cup","oz"}),
        ("Coconut Milk",         new[]{"ml","cup","can"}),
    };

    // ── Recipe title pools ────────────────────────────────────
    private static readonly string[] Cuisines =
        { "Italian","Mexican","Thai","Japanese","Indian","French","Greek","American","Chinese","Mediterranean" };

    private static readonly string[] DishTypes =
        { "Pasta","Stew","Soup","Salad","Stir-fry","Curry","Casserole","Tacos","Pizza",
          "Risotto","Roast","Cake","Cookies","Bread","Muffins","Pancakes","Burger","Noodles" };

    private static readonly string[] Adjectives =
        { "Creamy","Spicy","Crispy","Smoky","Tangy","Classic","One-Pot","Quick",
          "Easy","Hearty","Light","Rustic","Zesty","Garlic-Butter","Honey-Glazed" };

    // ─────────────────────────────────────────────────────────
    //  Entry point
    // ─────────────────────────────────────────────────────────
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        // Do NOT create a new scope here — caller (Program.cs) already provides a scoped provider.
        var db = serviceProvider.GetRequiredService<ForkedDbContext>();
        var userMgr = serviceProvider.GetRequiredService<UserManager<User>>();
        var roleMgr = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = serviceProvider.GetRequiredService<ILogger<SeederMarker>>();

        logger.LogInformation("🌱  Starting Forked database seed…");

        await db.Database.MigrateAsync();

        // Idempotency guard — skip if community users are already present
        // (Checks for community user pattern to avoid conflict with external role seeders)
        if (await db.Users.AnyAsync(u => u.UserName != null && u.UserName.StartsWith("user_")))
        {
            logger.LogInformation("✅  Database already seeded — skipping.");
            return;
        }

        var rng = new Faker { Random = new Randomizer(RngSeed) };

        // ─────────────────────────────────────────────────────
        // 1. Roles
        // ─────────────────────────────────────────────────────
        logger.LogInformation("   [1/7] Roles…");
        foreach (var role in new[] { "Admin", "User" })
            if (!await roleMgr.RoleExistsAsync(role))
                await roleMgr.CreateAsync(new IdentityRole(role));

        // ─────────────────────────────────────────────────────
        // 2. Fixed test / admin accounts
        // ─────────────────────────────────────────────────────
        logger.LogInformation("   [2/7] Test accounts…");
        var allUsers = new List<User>();

        foreach (var (email, userName, display, password, role) in TestUsers)
        {
            // Check by username, email, AND display name — any of these being taken means the user exists
            var existing = await userMgr.FindByNameAsync(userName)
                        ?? await userMgr.FindByEmailAsync(email)
                        ?? await db.Users.FirstOrDefaultAsync(u => u.DisplayName == display);

            if (existing != null)
            {
                // Ensure role is assigned even if user was created by an external seeder
                if (!await userMgr.IsInRoleAsync(existing, role))
                    await userMgr.AddToRoleAsync(existing, role);

                allUsers.Add(existing);
                logger.LogInformation("      ⏭  Already exists: {Email}", email);
                continue;
            }

            var user = new User
            {
                UserName = userName,
                NormalizedUserName = userName.ToUpperInvariant(),
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                EmailConfirmed = true,
                DisplayName = display,
                Bio = $"Hi, I'm {display} — one of the Forked test accounts.",
                ProfilePicturePath = AvatarUrl(rng.PickRandom(ProfilePhotoIds)),
                CreatedAt = DateTime.UtcNow.AddMonths(-rng.Random.Int(6, 24)),
            };

            var result = await userMgr.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userMgr.AddToRoleAsync(user, role);
                allUsers.Add(user);
                logger.LogInformation("      ✔  {Role}: {Email}", role, email);
            }
            else
            {
                logger.LogWarning("      ⚠  {Email}: {Errors}",
                    email, string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }

        // ─────────────────────────────────────────────────────
        // 3. Community users (Faker)
        // ─────────────────────────────────────────────────────
        logger.LogInformation("   [3/7] {Count} community users…", CommunityUsers);

        var userFaker = new Faker<User>()
            .UseSeed(RngSeed)
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.EmailConfirmed, _ => true)
            .RuleFor(u => u.Bio, f => f.PickRandom<string?>(new string?[]
            {
                f.Lorem.Sentence(8),
                $"Home cook from {f.Address.City()} who loves experimenting.",
                $"Food blogger since {f.Date.Past(10).Year}. Come for the recipes!",
                $"Obsessed with {f.PickRandom(Cuisines)} cuisine.",
                null,
            }))
            .RuleFor(u => u.ProfilePicturePath, f => f.Random.Bool(0.75f)
                ? AvatarUrl(f.PickRandom(ProfilePhotoIds))
                : null)
            .RuleFor(u => u.CreatedAt, f => f.Date.Past(3).ToUniversalTime());

        var communityUserDtos = userFaker.Generate(CommunityUsers);
        int created = 0;

        for (int i = 0; i < communityUserDtos.Count; i++)
        {
            var dto = communityUserDtos[i];
            // Guarantee unique UserName and DisplayName with index suffix to satisfy the unique index
            dto.UserName = $"user_{i:D4}_{rng.Random.AlphaNumeric(4)}";
            dto.DisplayName = $"{rng.Name.FullName()} #{i:D4}";

            var result = await userMgr.CreateAsync(dto, "Community@123!");
            if (result.Succeeded)
            {
                await userMgr.AddToRoleAsync(dto, "User");
                allUsers.Add(dto);
                created++;
            }
        }

        logger.LogInformation("      ✔  {Total} total users.", allUsers.Count);

        var allUserIds = allUsers.Select(u => u.Id).ToList();

        // ─────────────────────────────────────────────────────
        // 4. Ingredients
        // ─────────────────────────────────────────────────────
        logger.LogInformation("   [4/7] Ingredients…");

        var ingredients = IngredientData
            .Select(d => new Ingredient { Name = d.Name })
            .ToList();

        await db.Ingredients.AddRangeAsync(ingredients);
        await db.SaveChangesAsync();

        // ─────────────────────────────────────────────────────
        // 5. Recipes + Steps + RecipeIngredients
        // ─────────────────────────────────────────────────────
        logger.LogInformation("   [5/7] {Count} recipes…", RecipeCount);

        var recipeFaker = new Faker { Random = new Randomizer(RngSeed) };
        var recipes = new List<Recipe>();
        var allSteps = new List<RecipeStep>();
        var allRIs = new List<RecipeIngredient>();

        var prepStyles = new[]
        {
            "", "finely chopped", "diced", "minced", "sliced", "grated",
            "roughly chopped", "halved", "peeled", "toasted", "softened", "crushed",
        };

        // Save recipes in batches so we can reference earlier IDs for forks
        const int batchSize = 100;

        for (int batch = 0; batch < RecipeCount; batch += batchSize)
        {
            var batchRecipes = new List<Recipe>();
            int end = Math.Min(batch + batchSize, RecipeCount);

            for (int i = batch; i < end; i++)
            {
                int imgCount = recipeFaker.Random.Int(1, 4);
                var imagePaths = recipeFaker.PickRandom(FoodPhotoIds, imgCount)
                    .Select(id => FoodUrl(id))
                    .ToList();

                // Recipes after the first 80 may fork from an already-saved recipe
                int? parentId = null;
                if (i >= 80 && recipeFaker.Random.Bool(0.25f) && recipes.Count > 0)
                    parentId = recipeFaker.PickRandom(recipes).Id;

                var recipe = new Recipe
                {
                    Title = $"{recipeFaker.PickRandom(Adjectives)} {recipeFaker.PickRandom(Cuisines)} {recipeFaker.PickRandom(DishTypes)}",
                    Description = recipeFaker.Lorem.Paragraphs(recipeFaker.Random.Int(1, 3)),
                    ImagePaths = imagePaths,
                    PreparationTimeInMinutes = recipeFaker.Random.Bool(0.9f) ? recipeFaker.Random.Int(5, 60) : null,
                    CookingTimeInMinutes = recipeFaker.Random.Bool(0.9f) ? recipeFaker.Random.Int(10, 180) : null,
                    Servings = recipeFaker.Random.Int(1, 12),
                    AuthorId = recipeFaker.PickRandom(allUserIds),
                    ParentRecipeId = parentId,
                    CreatedAt = recipeFaker.Date.Past(2).ToUniversalTime(),
                };

                batchRecipes.Add(recipe);
            }

            await db.Recipes.AddRangeAsync(batchRecipes);
            await db.SaveChangesAsync();
            recipes.AddRange(batchRecipes);

            // Build steps & ingredients for this batch immediately (so RecipeId is populated)
            foreach (var recipe in batchRecipes)
            {
                // Steps
                int stepCount = recipeFaker.Random.Int(2, 7);
                for (int s = 1; s <= stepCount; s++)
                {
                    allSteps.Add(new RecipeStep
                    {
                        RecipeId = recipe.Id,
                        StepNumber = s,
                        StepName = s == 1 ? "Prepare ingredients"
                                    : s == stepCount ? "Serve and enjoy"
                                    : recipeFaker.Random.Bool(0.5f)
                                        ? recipeFaker.Lorem.Sentence(3)
                                        : null,
                        Instruction = recipeFaker.Lorem.Sentences(recipeFaker.Random.Int(1, 4)),
                        ImagePaths = recipeFaker.Random.Bool(0.25f)
                            ? new List<string> { FoodUrl(recipeFaker.PickRandom(FoodPhotoIds), 600) }
                            : new List<string>(),
                        CreatedAt = recipe.CreatedAt,
                    });
                }

                // Ingredients
                int riCount = recipeFaker.Random.Int(2, 8);
                var picked = recipeFaker.PickRandom(ingredients, riCount).DistinctBy(x => x.Id).ToList();

                foreach (var ing in picked)
                {
                    var units = IngredientData.First(d => d.Name == ing.Name).Units;
                    allRIs.Add(new RecipeIngredient
                    {
                        RecipeId = recipe.Id,
                        IngredientId = ing.Id,
                        Quantity = Math.Round(recipeFaker.Random.Double(0.25, 500), 2),
                        Unit = recipeFaker.PickRandom(units),
                        Preparation = recipeFaker.PickRandom(prepStyles),
                        CreatedAt = recipe.CreatedAt,
                    });
                }
            }
        }

        await db.RecipeSteps.AddRangeAsync(allSteps);
        await db.RecipeIngredients.AddRangeAsync(allRIs);
        await db.SaveChangesAsync();

        logger.LogInformation("      ✔  {R} recipes | {S} steps | {I} recipe-ingredients",
            recipes.Count, allSteps.Count, allRIs.Count);

        // ─────────────────────────────────────────────────────
        // 6. Reviews (weighted ratings, optional images)
        // ─────────────────────────────────────────────────────
        logger.LogInformation("   [6/7] {Count} reviews…", ReviewCount);

        var reviewFaker = new Faker { Random = new Randomizer(RngSeed) };
        var reviews = new List<Review>();
        var reviewedPairs = new HashSet<(string UserId, int RecipeId)>();

        var reviewTemplates = new Dictionary<int, string[]>
        {
            [5] = new[]
            {
                "Absolutely delicious! Made this for dinner and the whole family loved it.",
                "This is a keeper — easy to follow and the result is incredible.",
                "I've tried many recipes but this one is the best by far!",
                "Made it twice already. Will 100% make again.",
                "Perfect flavours, beautifully balanced. A new household favourite.",
            },
            [4] = new[]
            {
                "Really good recipe. Came out great with minimal effort.",
                "Tasty and straightforward. I added a pinch more salt and it was perfect.",
                "Great weeknight dinner. Everyone asked for seconds.",
                "Solid recipe — I'll be adding this to my regular rotation.",
            },
            [3] = new[]
            {
                "Decent recipe. Followed it closely and it turned out okay.",
                "Good base — I tweaked the seasoning a bit to suit my taste.",
                "Reliable and tasty, though not particularly exciting.",
                "Came out alright. Would adjust the cooking time slightly next time.",
            },
            [2] = new[]
            {
                "A bit bland for my taste — needed much more seasoning.",
                "The cooking time felt off; mine needed an extra 15 minutes.",
                "Acceptable result but the instructions could be clearer.",
            },
            [1] = new[]
            {
                "Really didn't work for me — the texture was completely off.",
                "Followed exactly and it was quite disappointing.",
                "The flavours just didn't come together. Not making this again.",
            },
        };

        int attempts = 0;
        while (reviews.Count < ReviewCount && attempts < ReviewCount * 4)
        {
            attempts++;
            var userId = reviewFaker.PickRandom(allUserIds);
            var recipe = reviewFaker.PickRandom(recipes);

            if (recipe.AuthorId == userId) continue; // no self-reviews
            if (!reviewedPairs.Add((userId, recipe.Id))) continue; // one review per (user, recipe)

            int rating = reviewFaker.Random.WeightedRandom(
                new[] { 1, 2, 3, 4, 5 },
                new[] { 0.03f, 0.07f, 0.15f, 0.35f, 0.40f });

            var message = reviewFaker.PickRandom(reviewTemplates[rating]);
            if (reviewFaker.Random.Bool(0.3f))
                message += " " + reviewFaker.Lorem.Sentence(6);

            reviews.Add(new Review
            {
                UserId = userId,
                RecipeId = recipe.Id,
                Rating = rating,
                Message = message,
                // 40 % of reviews include 1–2 food photos
                ImagePaths = reviewFaker.Random.Bool(0.4f)
                    ? reviewFaker.PickRandom(FoodPhotoIds, reviewFaker.Random.Int(1, 2))
                               .Select(id => FoodUrl(id, 600))
                               .ToList()
                    : new List<string>(),
                CreatedAt = reviewFaker.Date.Between(recipe.CreatedAt, DateTime.UtcNow).ToUniversalTime(),
            });
        }

        await db.Reviews.AddRangeAsync(reviews);
        await db.SaveChangesAsync();

        // ─────────────────────────────────────────────────────
        // 7. Follows + Favourites
        // ─────────────────────────────────────────────────────
        logger.LogInformation("   [7/7] Follows & favourites…");

        // Follows
        var followFaker = new Faker { Random = new Randomizer(RngSeed) };
        var follows = new List<UserFollow>();
        var followPairs = new HashSet<(string, string)>();

        for (int att = 0; follows.Count < FollowCount && att < FollowCount * 5; att++)
        {
            var followerId = followFaker.PickRandom(allUserIds);
            var followingId = followFaker.PickRandom(allUserIds);
            if (followerId == followingId) continue;
            if (!followPairs.Add((followerId, followingId))) continue;

            follows.Add(new UserFollow
            {
                FollowerId = followerId,
                FollowingId = followingId,
                CreatedAt = followFaker.Date.Past(2).ToUniversalTime(),
            });
        }

        await db.UserFollows.AddRangeAsync(follows);
        await db.SaveChangesAsync();

        // Favourites
        var favFaker = new Faker { Random = new Randomizer(RngSeed) };
        var favorites = new List<UserFavoriteRecipe>();
        var favPairs = new HashSet<(string, int)>();

        for (int att = 0; favorites.Count < FavoriteCount && att < FavoriteCount * 3; att++)
        {
            var userId = favFaker.PickRandom(allUserIds);
            var recipeId = favFaker.PickRandom(recipes).Id;
            if (!favPairs.Add((userId, recipeId))) continue;

            favorites.Add(new UserFavoriteRecipe
            {
                UserId = userId,
                RecipeId = recipeId,
                CreatedAt = favFaker.Date.Past(2).ToUniversalTime(),
            });
        }

        await db.UserFavoriteRecipes.AddRangeAsync(favorites);
        await db.SaveChangesAsync();

        // ─────────────────────────────────────────────────────
        // Summary
        // ─────────────────────────────────────────────────────
        logger.LogInformation("🎉  Seed complete!");
        logger.LogInformation("    Users       : {U}", allUsers.Count);
        logger.LogInformation("    Ingredients : {Ing}", ingredients.Count);
        logger.LogInformation("    Recipes     : {R}", recipes.Count);
        logger.LogInformation("    Steps       : {S}", allSteps.Count);
        logger.LogInformation("    RI rows     : {I}", allRIs.Count);
        logger.LogInformation("    Reviews     : {Rev}", reviews.Count);
        logger.LogInformation("    Follows     : {F}", follows.Count);
        logger.LogInformation("    Favourites  : {Fav}", favorites.Count);

        logger.LogInformation("");
        logger.LogInformation("  ┌─────────────────────────────────────────────┐");
        logger.LogInformation("  │  Test account credentials                   │");
        logger.LogInformation("  ├──────────────────────────┬──────────────────┤");
        logger.LogInformation("  │  admin@forked.dev        │  Admin@123!      │");
        logger.LogInformation("  │  alice@forked.dev        │  Test@123!       │");
        logger.LogInformation("  │  bob@forked.dev          │  Test@123!       │");
        logger.LogInformation("  │  charlie@forked.dev      │  Test@123!       │");
        logger.LogInformation("  │  diana@forked.dev        │  Test@123!       │");
        logger.LogInformation("  └──────────────────────────┴──────────────────┘");
    }

    // ILogger requires a non-static type argument — this marker satisfies that constraint.
    private class SeederMarker { }
}