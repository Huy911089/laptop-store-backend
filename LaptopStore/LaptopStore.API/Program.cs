using LaptopStore.Repositories.Context;
using Microsoft.EntityFrameworkCore;

namespace LaptopStore.API
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

            // [Program.cs] : Cấu hình DbContext để EF Core biết rằng các file Migrations sẽ nằm ở project Repositories.
            builder.Services.AddDbContext<LaptopStoreDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    // [Program.cs] : Chỉ định rõ Assembly chứa Migrations là LaptopStore.Repositories
                    b => b.MigrationsAssembly("LaptopStore.Repositories")
                ));

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
