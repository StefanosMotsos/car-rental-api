using CarRentalApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRentalApp.Data
{
    public class CarRentalDbContext : DbContext
    {

        public CarRentalDbContext(DbContextOptions<CarRentalDbContext> options)
            : base(options)
        {
        }

        public DbSet<Capability> Capabilities { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehiclePhoto> VehiclePhotos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Capability>(en =>
            {
                en.Property(p => p.Name).HasMaxLength(100);
                en.Property(p => p.Description).HasMaxLength(255);
                en.HasIndex(i => i.Name, "UQ_Capabilities_Name").IsUnique();
            });

            modelBuilder.Entity<Role>(en =>
            {
                en.Property(p => p.Name).HasMaxLength(100);
                en.HasMany(p => p.Capabilities)
                .WithMany(p => p.Roles)
                .UsingEntity("RolesCapabilities", j =>
                {
                    j.HasIndex("CapabilitiesId")
                    .HasDatabaseName("IX_RolesCapabilities_CapabilityId");
                });
                en.HasIndex(i => i.Name, "UQ_Roles_Name").IsUnique();
            });

            modelBuilder.Entity<User>(en =>
            {
                en.Property(e => e.Uuid).HasDefaultValueSql("NEWID()");
                en.Property(p => p.Username).HasMaxLength(50);
                en.Property(p => p.Password).HasMaxLength(100);
                en.Property(p => p.Email).HasMaxLength(50);
                en.Property(e => e.Firstname).HasMaxLength(50);
                en.Property(e => e.Lastname).HasMaxLength(50);

                en.HasOne(p => p.Role).WithMany(p => p.Users)
                    .HasForeignKey(p => p.RoleId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Users_RoleId");

                en.HasIndex(i => i.Username, "IX_Users_Username").IsUnique();
                en.HasIndex(i => i.Email, "IX_Users_Email").IsUnique();
                en.HasIndex(i => i.RoleId, "IX_Users_RoleId");
                en.HasIndex(e => e.Uuid).IsUnique();
            });

            modelBuilder.Entity<Customer>(en =>
            {
                en.Property(e => e.Uuid).HasDefaultValueSql("NEWID()");
                en.Property(p => p.DriverLicense).HasMaxLength(20);
                en.Property(p => p.DateOfBirth).HasColumnType("date");
                en.HasOne(p => p.User).WithOne(p => p.Customer)
                    .HasForeignKey<Customer>(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Customer_UserId");

                en.HasIndex(i => i.UserId, "IX_Customers_UserId");
                en.HasIndex(i => i.DriverLicense, "IX_Customers_DriverLicense").IsUnique();
            });

            modelBuilder.Entity<Employee>(en =>
            {
                en.Property(e => e.Uuid).HasDefaultValueSql("NEWID()");
                en.Property(p => p.PhoneNumber).HasMaxLength(20);
                en.HasOne(p => p.User).WithOne(p => p.Employee)
                    .HasForeignKey<Employee>(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Employee_UserId");

                en.HasIndex(i => i.UserId, "IX_Employee_UserId");
                en.HasIndex(e => e.Uuid).IsUnique();
            });

            modelBuilder.Entity<Category>(en =>
            {
                en.Property(p => p.Name).HasMaxLength(50);
                en.Property(p => p.Description).HasMaxLength(255);

                en.HasIndex(i => i.Name, "UQ_Categories_Name").IsUnique();
            });

            modelBuilder.Entity<Vehicle>(en =>
            {
                en.Property(e => e.Uuid).HasDefaultValueSql("NEWID()");
                en.Property(p => p.Make).HasMaxLength(50);
                en.Property(p => p.Model).HasMaxLength(50);
                en.Property(p => p.Year).HasColumnType("smallint");
                en.Property(p => p.LicensePlate).HasMaxLength(20);
                en.Property(p => p.DailyRate).HasColumnType("decimal(10,2)");
                en.Property(p => p.TierType).HasConversion<string>();
                en.Property(p => p.Status).HasConversion<string>();

                en.HasOne(p => p.Category).WithMany(p => p.Vehicles)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Vehicles_CategoryId");

                en.HasIndex(i => i.LicensePlate, "UQ_Vehicles_LicensePlate").IsUnique();
                en.HasIndex(i => i.CategoryId, "IX_Vehicles_CategoryId");
                en.HasIndex(i => i.Status, "IX_Vehicles_Status");
                en.HasIndex(e => e.Uuid).IsUnique();
            });

            modelBuilder.Entity<VehiclePhoto>(en =>
            {
                en.Property(e => e.Uuid).HasDefaultValueSql("NEWID()");
                en.Property(p => p.OriginalName).HasMaxLength(255);
                en.Property(p => p.SavedName).HasMaxLength(255);
                en.Property(p => p.FilePath).HasMaxLength(1024);
                en.Property(p => p.ContentType).HasMaxLength(100);
                en.Property(p => p.Extension).HasMaxLength(10);

                en.HasOne(p => p.Vehicle).WithOne(p => p.Photo)
                    .HasForeignKey<VehiclePhoto>(p => p.VehicleId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_VehiclePhotos_VehicleId");

                en.HasIndex(i => i.SavedName, "UQ_VehiclePhotos_SavedName").IsUnique();
                en.HasIndex(i => i.VehicleId, "IX_VehiclePhotos_VehicleId").IsUnique();
                en.HasIndex(e => e.Uuid).IsUnique();
            });

            modelBuilder.Entity<Location>(en =>
            {
                en.Property(p => p.Name).HasMaxLength(100);
                en.Property(p => p.Address).HasMaxLength(255);
                en.Property(p => p.City).HasMaxLength(100);
                en.Property(p => p.Phone).HasMaxLength(20);

                en.HasIndex(i => i.Name, "IX_Locations_Name");
            });

            modelBuilder.Entity<Rental>(en =>
            {
                en.Property(e => e.Uuid).HasDefaultValueSql("NEWID()");
                en.Property(p => p.StartDate).HasColumnType("date");
                en.Property(p => p.EndDate).HasColumnType("date");
                en.Property(p => p.TotalCost).HasColumnType("decimal(10,2)");
                en.Property(p => p.Status).HasConversion<string>();

                en.HasOne(p => p.Customer).WithMany(p => p.Rentals)
                    .HasForeignKey(p => p.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Rentals_CustomerId");

                en.HasOne(p => p.Employee).WithMany(p => p.Rentals)
                    .HasForeignKey(p => p.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Rentals_EmployeeId");

                en.HasOne(p => p.Vehicle).WithMany(p => p.Rentals)
                    .HasForeignKey(p => p.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Rentals_VehicleId");

                en.HasOne(p => p.PickupLocation).WithMany(p => p.PickupRentals)
                    .HasForeignKey(p => p.PickupLocationId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Rentals_PickupLocationId");

                en.HasOne(p => p.DropoffLocation).WithMany(p => p.DropoffRentals)
                    .HasForeignKey(p => p.DropoffLocationId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Rentals_DropoffLocationId");

                en.HasIndex(i => i.CustomerId, "IX_Rentals_CustomerId");
                en.HasIndex(i => i.EmployeeId, "IX_Rentals_EmployeeId");
                en.HasIndex(i => i.VehicleId, "IX_Rentals_VehicleId");
                en.HasIndex(i => i.Status, "IX_Rentals_Status");
                en.HasIndex(e => e.Uuid).IsUnique();
            });


        }
    }
}
