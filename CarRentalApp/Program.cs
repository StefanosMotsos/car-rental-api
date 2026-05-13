
using CarRentalApp.Configuration;
using CarRentalApp.Data;
using CarRentalApp.Repositories;
using CarRentalApp.Security;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace CarRentalApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((hostingContext, configuration) =>
            {
                configuration.ReadFrom.Configuration(hostingContext.Configuration);
            });

            var connString = builder.Configuration.GetConnectionString("DevConnection");

            builder.Services.AddDbContext<CarRentalDbContext>(options =>
                    options.UseSqlServer(connString));

            builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MapperConfig>());

            builder.Services.AddSingleton<IEncryptionUtil, EncryptionUtil>();

            builder.Services.AddRepositories();

            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            builder.Services.AddControllers();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                
            }

            var supportedCultures = new[] { "en", "el" };
            app.UseRequestLocalization(options =>
            {
                options.SetDefaultCulture("en")
                       .AddSupportedCultures(supportedCultures)
                       .AddSupportedUICultures(supportedCultures);
            });

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
