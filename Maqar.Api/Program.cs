using Microsoft.EntityFrameworkCore;
using Serilog;
using Maqar.Domain;
using Maqar.Domain.Repositories;
using Maqar.Repository;
using Maqar.Repository.Data;

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

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // The same MaqarContext the admin dashboard migrates — the API never runs migrations itself.
            builder.Services.AddDbContext<MaqarContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // The public site (maqar-properties-hub frontend) calls this API from the browser.
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            builder.Services.AddCors(options =>
                options.AddPolicy(FrontendCorsPolicy, policy =>
                    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

            #endregion

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseCors(FrontendCorsPolicy);

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
