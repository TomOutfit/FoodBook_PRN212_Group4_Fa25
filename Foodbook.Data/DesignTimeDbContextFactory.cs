using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Foodbook.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FoodbookDbContext>
    {
        public FoodbookDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FoodbookDbContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=FoodbookDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true");

            return new FoodbookDbContext(optionsBuilder.Options);
        }
    }
}
