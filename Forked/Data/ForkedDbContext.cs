using Microsoft.EntityFrameworkCore;

namespace Forked.Data
{
    public class ForkedDbContext(DbContextOptions<ForkedDbContext> options) : IdentityDbContext<User>(options)
    {
    }
}
