using LaptopStore.Repositories.Context;
using LaptopStore.Repositories.Implements;
using LaptopStore.Repositories.Interfaces;
using LaptopStore.Services.Configurations;
using LaptopStore.Services.Implements;
using LaptopStore.Services.Interfaces;
using LaptopStore.Services.Mappings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

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
            // [Program] : Cấu hình Swagger và thêm định nghĩa Bearer để test API có Authorize dễ hơn ngay trong Swagger UI.
            builder.Services.AddSwaggerGen(option => 
            {
                option.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
                {
                    Title = "LaptopStore API",
                    Version = "v1"
                });

                option.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme 
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Nhập token theo format: Bearer {your access token}"
                });
                option.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }

                });
            });

            builder.Services.AddDbContext<LaptopStoreDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));



            // [Program] : Bind section JwtSettings từ appsettings sang class typed options để inject bằng IOptions<JwtSettings>.
            //gom các thông số (Key, Issuer, hạn sử dụng...) viết trong file appsettings.json và "nhồi" vào class C# JwtSettings
            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection(JwtSettings.SectionName));
            //var jwtKey = builder.Configuration["JwtSettings:Key"];
            //var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
            //var jwtAudience = builder.Configuration["JwtSettings:Audience"];
            var jwtSettings = builder.Configuration
                                .GetSection(JwtSettings.SectionName)
                                .Get<JwtSettings>() ?? throw new Exception("JwtSettings chưa được cấu hình trong appsettings.json.");
            // [Program] : Cấu hình Authentication dùng JWT Bearer để các endpoint có thể đọc token từ header Authorization.
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt => 
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key!)),
                        ClockSkew = TimeSpan.Zero
                    };
                });


            // [Program] : Đăng ký UnitOfWork để quản lý Transaction
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            // [Program] : Đăng ký các Services của nghiệp vụ (Business Logic)
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IBrandService, BrandService>();
            builder.Services.AddScoped<ICartService,CartService>();
            // [Program] : Đăng ký AutoMapper, tự động quét các Profile trong assembly của tầng Services
            builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
