using LaptopStore.Repositories.Context;
using LaptopStore.Repositories.Implements;
using LaptopStore.Repositories.Interfaces;
using LaptopStore.Services.Configurations;
using LaptopStore.Services.Helpers;
using LaptopStore.Services.Implements;
using LaptopStore.Services.Interfaces;
using LaptopStore.Services.Mappings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Serilog;
using Serilog.Sinks.Graylog;
using Serilog.Sinks.Graylog.Core.Transport;
using Serilog.Events;

namespace LaptopStore.API
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            // [Program] : Khởi tạo hệ thống Log tập trung Graylog , Cấu hình Serilog ngay dòng đầu tiên
            Log.Logger = new LoggerConfiguration()
                /*
                 * Sẽ không sử dụng 3 dòng này lý do nó sẽ gây hard code thay vào đó mình có thể sử dụng appsettings.json để chưa và sau này mình sửa sẽ dễ hơn
                 * Trường hợp để như vậy sẽ gây ra mốt mình ko muốn nữa thì phải mở code lên comment lại và build deploy lại các thứ rất phiên và ko nên
                //.MinimumLevel.Information()
                // 1. Ép tất cả log từ namespace "Microsoft" chỉ hiện từ mức Warning trở lên
                //.MinimumLevel.Override("Microsoft",LogEventLevel.Warning)
                // 2. (Tùy chọn) Nếu vẫn muốn xem Microsoft log nhẹ, nhưng ghét riêng EF Core SQL:
                //.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
                */
                .ReadFrom.Configuration(builder.Configuration) // Yêu cầu Serilog lấy các luật Override từ appsettings.json
                .WriteTo.Console()
                .WriteTo.Graylog(new GraylogSinkOptions
                {
                    HostnameOrAddress = "localhost",
                    Port = 12201,
                    TransportType = TransportType.Udp
                })
                .CreateLogger();

            try
            {
                
                // Chặn ILogger mặc định của Microsoft và thay bằng Serilog
                builder.Host.UseSerilog();

                // [Program] : Hỗ trợ hiển thị tiếng Việt có dấu trên Console
                Console.OutputEncoding = Encoding.UTF8;

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
                builder.Services.AddStackExchangeRedisCache(options =>
            {
                // [Program] : Khai báo Redis connection string để IDistributedCache dùng Redis thay vì memory cache.
                options.Configuration = builder.Configuration.GetConnectionString("Redis");
                options.InstanceName = "LaptopStore:"; // Thêm tiền tố để khỏi nhầm với project khác nếu dùng chung 1 Redis
            });


                //[Program] : Đăng ký Options Pattern cho kafka
                builder.Services.Configure<KafkaOptions>(
                        builder.Configuration.GetSection(KafkaOptions.SectionName)
                    );


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
                builder.Services.AddScoped<ICartService, CartService>();
                builder.Services.AddScoped<IOrderService, OrderService>();
                builder.Services.AddScoped<IProductQueryBuilder, ProductQueryBuilder>();
                builder.Services.AddScoped<ICacheService, CacheService>();
                // [Program] : Đăng ký AutoMapper, tự động quét các Profile trong assembly của tầng Services
                builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
                builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();


                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowFrontend", policy =>
                    {
                        policy
                            .WithOrigins("http://localhost:5173")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
                });

                var app = builder.Build();



                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.UseCors("AllowFrontend");
                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllers();

                app.Run();
            }
            catch (Exception ex) when (ex is not HostAbortedException) // Dặn catch bỏ qua lỗi ngắt của EF Core
            {
                Log.Fatal(ex, "[System] : Ứng dụng dừng đột ngột do lỗi chí mạng!");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
