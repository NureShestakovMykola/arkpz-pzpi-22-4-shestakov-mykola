
using Core.Helpers;
using DAL;
using DAL.Repositories;
using GrowMate.Services;
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

            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(nameof(EmailSettings)));

            builder.Services.AddScoped(typeof(EmailService));
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped(typeof(UnitOfWork));

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseSession();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
