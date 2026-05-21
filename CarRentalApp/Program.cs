using CarRentalApp.Configuration;
using CarRentalApp.Data;
using CarRentalApp.Helpers;
using CarRentalApp.Repositories;
using CarRentalApp.Security;
using CarRentalApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

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

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowClient", policy =>
                policy.WithOrigins(builder.Configuration["Cors:Origin"]!)
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });

            builder.Services.AddSingleton<IEncryptionUtil, EncryptionUtil>();

            builder.Services.AddRepositories();

            builder.Services.AddServices();

            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MapperConfig>());

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

            var jwtSettings = builder.Configuration.GetSection("Jwt");
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.IncludeErrorDetails = builder.Environment.IsDevelopment();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],

                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],

                    ValidateLifetime = true,

                    ValidateIssuerSigningKey = true,

                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!))
                };
            });

            builder.Services.AddAuthorization();

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Car Rental App", Version = "v1" });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);

                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme,
                    new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme.",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = JwtBearerDefaults.AuthenticationScheme,
                        BearerFormat = "JWT"
                    });
                options.OperationFilter<AuthorizeOperationFilter>();
            });

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            builder.Services.AddScoped<MDCLoggingMiddleware>();

            var app = builder.Build();

            app.UseExceptionHandler();
            app.UseSerilogRequestLogging();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            var supportedCultures = new[] { "en", "el" };
            app.UseRequestLocalization(options =>
            {
                options.SetDefaultCulture("en")
                       .AddSupportedCultures(supportedCultures)
                       .AddSupportedUICultures(supportedCultures);
            });

            app.UseHttpsRedirection();

            app.UseCors("AllowClient");

            app.UseAuthentication();
            app.UseMiddleware<MDCLoggingMiddleware>();
            app.UseAuthorization();


            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CarRentalDbContext>();
                db.Database.Migrate();
            }

            app.Run();
        }
    }
}
