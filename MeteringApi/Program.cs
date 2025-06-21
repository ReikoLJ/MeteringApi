using System.Reflection;
using MeteringApi.Database;
using MeteringApi.Database.Interfaces;
using MeteringApi.Services;
using MeteringApi.Services.Interfaces;

namespace MeteringApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Configuration.AddJsonFile("appsettings.json", false);

            var dbConnectionString = builder.Configuration.GetConnectionString("PostgresDatabase");

            // Database
            builder.Services.AddScoped<IMeteringDbContext, MeteringDbContext>(c =>
                new MeteringDbContext(dbConnectionString));
            // Services
            builder.Services.AddTransient<IMeteringService, MeteringService>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                // Other config...

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
