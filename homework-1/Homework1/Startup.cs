using Domain.Repository;
using Domain.Services.IServices;
using Infrastructure;
using Microsoft.OpenApi.Models;
using Sales.Services;

namespace Homework1
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                //Для схемы
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Product API", Version = "v1" });
                c.UseInlineDefinitionsForEnums();
                c.CustomSchemaIds(type => type.ToString());
            });

            services.AddSingleton<ISalesRepository, SalesRepository>();
            services.AddSingleton<ISaleService, SalesService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}