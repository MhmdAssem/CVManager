using CV_Manager.Infrastructure.Data;
using CV_Manager.Domain.Interfaces;
using CV_Manager.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Hosting;
using CV_Manager.Mappings;
using CV_Manager.Infrastructure.Middleware;
using Serilog;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace CV_Manager
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Configure Logging first
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(dispose: true);
            });

            // Database
            services.AddDbContext<CVManagerContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(Startup).Assembly.GetName().Name))
                    .EnableSensitiveDataLogging(Configuration.GetValue<bool>("Logging:EnableSqlParameterLogging"))  // Add this for better debugging
            );

            // Core services
            services.AddAutoMapper(typeof(MappingProfile));
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            services.AddTransient<ICVRepository, CVRepository>();

            // API-related services
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling =
                        Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });

            // CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", builder =>
                {
                    builder.WithOrigins("http://localhost:4200")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // Swagger/OpenAPI
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CV Manager API",
                    Version = "v1",
                    Description = "API for managing CVs and related information"
                });
            });

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Exception handling should be first
            app.UseMiddleware<ErrorHandlingMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CV Manager API v1"));
            }

            // Add Serilog request logging after error handling but before other middleware
            app.UseSerilogRequestLogging(options =>
            {
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());
                };
            });

            // Security headers should come before response-generating middleware
            app.UseHttpsRedirection();

            // CORS should be early in the pipeline
            app.UseCors("AllowAngular");

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}