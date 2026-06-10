using CarRentalApp.Data;
using Microsoft.EntityFrameworkCore;

namespace CarRentalTests.Helper
{
    public static class TestDbContextFactory
    {

        public static CarRentalDbContext Create()
        {
            DbContextOptions<CarRentalDbContext> options;

            options = new DbContextOptionsBuilder<CarRentalDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new CarRentalDbContext(options);
        }
    }
}
