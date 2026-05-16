namespace CarRentalApp.Services
{
    public static class ServicesDIExtension
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IApplicationService, ApplicationService>();
            return services;
        }
    }
}
