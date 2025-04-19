using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using CatalogAPI.Data;
using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace CatalogAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var assembly = typeof(Program).Assembly;
            // add services


            _ = builder.Services.AddValidatorsFromAssembly(assembly);
            _ = builder.Services.AddMediatR(config =>
            {
                _ = config.RegisterServicesFromAssembly(assembly);
                _ = config.AddOpenBehavior(typeof(ValidationBehavior<,>));// adding pipelines validator with mediatr instead of inject Ivalidator in handler class
                _ = config.AddOpenBehavior(typeof(LoggingBehavior<,>));// adding pipelines Logging with mediatr instead of inject Ilogger in handler class
            });
            _ = builder.Services.AddMarten(opts =>
            {
                opts.Connection(builder.Configuration.GetConnectionString("Database")!);
            }).UseLightweightSessions();

            if (builder.Environment.IsDevelopment())
            {
                _ = builder.Services.InitializeMartenWith<CatalogInitialData>();
            }

            _ = builder.Services.AddCarter();

            _ = builder.Services.AddExceptionHandler<CustomExceptionHandler>();

            _ = builder.Services.AddHealthChecks().AddNpgSql(builder.Configuration.GetConnectionString("Database")!);
            var app = builder.Build();
            // http request

            _ = app.MapCarter();
            _ = app.UseExceptionHandler(options => { });
            _ = app.UseHealthChecks("/health",
                    new HealthCheckOptions
                    {
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });
            app.Run();
        }
    }
}
