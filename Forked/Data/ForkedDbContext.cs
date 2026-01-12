using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Forked.Models.Domains;

namespace Forked.Data
{
    public class ForkedDbContext(DbContextOptions<ForkedDbContext> options) : IdentityDbContext<User>(options)
    {
    }
}