using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Maqar.Api.Helpers;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Maqar.Api.Errors;
using Maqar.Api.Middlewares;
using Maqar.Api.Options;
using Maqar.Domain.Models;
using Maqar.Domain.Services.ContactMessages;
using Maqar.Domain.Services.Lookups;
using Maqar.Domain.Services.Public;
using Maqar.Domain.Services.Settings;
using Maqar.Domain;
using Maqar.Domain.Repositories;
using Maqar.Repository;
using Maqar.Repository.Data;
using Maqar.Repository.Repositories;
using Maqar.Services.ContactMessages;
using Maqar.Services.Lookups;
using Maqar.Services.Public;
using Maqar.Services.Settings;

namespace Maqar.Api
{
    public class Program
    {
        private const string FrontendCorsPolicy = "Frontend";

        public static void Main(string[] args)
        {
            #region Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(
                    path: "Logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    fileSizeLimitBytes: 10 * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    shared: true,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
            #endregion

            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog();

            #region Configure Services

            builder.Services.AddControllers(o => o.Conventions.Add(new RouteTokenTransformerConvention(new LowercaseRouteTransformer())))
                // Enums travel as the frontend's camelCase keys ("apartment", "rent", "superLux"), both ways.
                .AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                    // A malformed body reports "The input was not valid." instead of parser internals (CLR type names).
                    o.AllowInputFormatterExceptionMessages = false;
                });
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            #region Validation error response
            // [ApiController] short-circuits an invalid model before the action runs; this replaces the
            // default ProblemDetails body with the ApiResponse shape every other error uses.
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                // Bare 4xx results (e.g. 415 Unsupported Media Type) leave without a ProblemDetails body, so
                // UseStatusCodePagesWithReExecute turns them into the same ApiResponse as every other error.
                options.SuppressMapClientErrors = true;

                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var modelState = actionContext.ModelState;

                    // A body that isn't valid JSON: System.Text.Json reports it under "$…" keys, and the action's
                    // [ApiController] body parameter then also fails as "required" — one clear message instead.
                    if (modelState.Keys.Any(k => k.StartsWith('$')))
                        return new BadRequestObjectResult(new ApiValidationErrorResponse { Errors = ["The request body is not valid JSON."] });

                    var errors = modelState
                        .Where(p => p.Value?.Errors.Count > 0)
                        .SelectMany(p => p.Value!.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return new BadRequestObjectResult(new ApiValidationErrorResponse { Errors = errors });
                };
            });
            #endregion

            // The same MaqarContext the admin dashboard migrates — the API never runs migrations itself.
            builder.Services.AddDbContext<MaqarContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IPropertyStatsRepository, PropertyStatsRepository>();

            builder.Services.AddScoped<IPublicPropertyService, PublicPropertyService>();
            builder.Services.AddScoped<IPropertyEventService, PropertyEventService>();
            builder.Services.AddScoped<IContactMessageService, ContactMessageService>();
            builder.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();
            builder.Services.AddScoped<ILookupService<Area>, AreaService>();
            builder.Services.AddScoped<ILookupService<Feature>, FeatureService>();

            builder.Services.Configure<MediaOptions>(builder.Configuration.GetSection(MediaOptions.Section));
            builder.Services.Configure<TrackingOptions>(builder.Configuration.GetSection(TrackingOptions.Section));

            builder.Services.AddMaqarRateLimits();

            // The public site (maqar-properties-hub frontend) calls this API from the browser.
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            builder.Services.AddCors(options =>
                options.AddPolicy(FrontendCorsPolicy, policy =>
                    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

            #endregion

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            // First, and in every environment — it must wrap everything below to catch it.
            app.UseMiddleware<ExceptionMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference(); // interactive explorer at /scalar
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "api");
                });
            }

            // Body-less error responses (unknown route, wrong verb, bare 401/403) → ErrorsController.
            app.UseStatusCodePagesWithReExecute("/errors/{0}");

            if (!app.Environment.IsDevelopment())
                app.UseHttpsRedirection();

            app.UseCors(FrontendCorsPolicy);

            app.UseRateLimiter();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
