using Forked.Data;
using Microsoft.EntityFrameworkCore;

namespace Tests
{
    internal static class DbContextFactory
    {
        public static ForkedDbContext Create(string dbName = null)
        {
            var options = new DbContextOptionsBuilder<ForkedDbContext>()
                .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
                .Options;
            return new ForkedDbContext(options);
        }
    }
}
