using Domain.Repository;
using Domain.Repository.Interfaces;
using Domain.Services.Interfaces;
using FluentValidation;
using global::Homework2.GRPCServices;
using global::Homework2.Interceptors;
using global::Homework2.Validators;
using Homework2.Middleware;
using Homework2.Validators.GRPCValidators;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace Homework2
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
            services.AddValidatorsFromAssemblyContaining<UpdateProductPriceRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<GetProductsWithFiltersRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<AddProductRequestValidator>();

            services.AddValidatorsFromAssemblyContaining<UpdateProductPriceRequestGRPCValidator>();
            services.AddValidatorsFromAssemblyContaining<GetProductsWithFiltersRequestGRPCValidator>();
            services.AddValidatorsFromAssemblyContaining<AddProductRequestGRPCValidator>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();

                //Для схемы
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Product API", Version = "v1" });
                c.UseInlineDefinitionsForEnums();
                c.CustomSchemaIds(type => type.ToString());
            });

            services.AddGrpc(op =>
            {
                op.EnableDetailedErrors = true;
                op.Interceptors.Add<LoggerInterceptor>();
                op.Interceptors.Add<ExceptionInterceptor>();
                op.Interceptors.Add<ValidationInterceptor>();
            });

            services.AddGrpc();
            services.AddGrpcReflection();

            services.AddSingleton<IProductRepository, ProductRepository>();
            services.AddSingleton<IProductService, Domain.Services.ProductService>();          
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseMiddleware<ValidationMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGrpcService<ProductService>();
                endpoints.MapGrpcReflectionService();
            });
        }
    }
}

