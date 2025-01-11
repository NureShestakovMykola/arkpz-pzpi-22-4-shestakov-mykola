
using DAL;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GrowMate
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<ApplicationContext>(options =>
            {
                options.UseLazyLoadingProxies()
                    .UseSqlServer(builder.Configuration.GetConnectionString("DBConnectionString"));
            });

            string repositoryPath = Path.Combine(Directory.GetCurrentDirectory(), "SavedFiles");
            builder.Services.AddScoped<IFileRepository>(provider =>
                new FileRepository(provider.GetRequiredService<ILogger<FileRepository>>(), repositoryPath));

            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped(typeof(UnitOfWork));

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
