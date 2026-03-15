using System.Diagnostics.CodeAnalysis;
using Amazon.SimpleNotificationService;
using LambdaApiReference.Models;
using LambdaApiReference.Repositories;
using LambdaApiReference.Services;
using Microsoft.OpenApi.Models;

namespace LambdaApiReference;

[ExcludeFromCodeCoverage]
public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title       = "Lambda API Reference",
                Version     = "v1",
                Description = "Reference ASP.NET Core API running on AWS Lambda + API Gateway REST (v1)"
            });
        });

        services.Configure<ReferenceOptions>(Configuration.GetSection("ReferenceOptions"));
        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        services.AddScoped<IItemRepository, PostgresItemRepository>();
        services.AddScoped<IItemService, ItemService>();
        services.AddAWSService<IAmazonSimpleNotificationService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
            app.UseDeveloperExceptionPage();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lambda API Reference v1");
            c.RoutePrefix = "swagger";
        });

        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
