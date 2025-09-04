using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.DataAccess
{
    public class DataProtectionKeyContext : DbContext, IDataProtectionKeyContext
    {
        public DataProtectionKeyContext(DbContextOptions<DataProtectionKeyContext> options)
            : base(options) { }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = default!;
    }
}