using CloudinaryDotNet;
using DreamGuard.BE.API.Hubs;
using DreamGuard.BE.API.Implements;
using DreamGuard.BE.BLL;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Implements;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Options;
using DreamGuard.BE.DAL.Repositories.Implements;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Hangfire.PostgreSql.Properties;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DreamGuard.BE.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Add Controllers with options
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
                    //chuyển đổi Enum thành String khi nhận/xuất JSON
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var errors = context.ModelState.Values
                                                .SelectMany(v => v.Errors)
                                                .Select(e => e.ErrorMessage)
                                                .ToList();
                        var errorResponse = new ErrorResponse
                        {
                            ErrorCode = 400,
                            Message = errors ?? new List<string> { "Bad request" },

                        };
                        return new BadRequestObjectResult(errorResponse);
                    };
                });

            //Register options
            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
            builder.Services.Configure<BrevoOptions>(builder.Configuration.GetSection(BrevoOptions.BrevoOptionsKey));
            builder.Services.Configure<CloudinaryOptions>(builder.Configuration.GetSection("Cloudinary"));
            builder.Services.Configure<OtpOptions>(builder.Configuration.GetSection("OtpOptions"));
            builder.Services.Configure<VnPayOptions>(builder.Configuration.GetSection("VnpayOptions"));

            //Add authentication with JWT

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtOptions>();

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Cookies["AccessToken"];
                        // 2. Nếu Cookie trống, thử tìm trong Query String với tên "access_token" 
                        // (Dành cho lúc test bằng HTML hoặc mốt Mobile App xài)
                        if (string.IsNullOrEmpty(accessToken))
                        {
                            accessToken = context.Request.Query["access_token"];
                        }

                        // 3. Nếu tìm thấy token ở 1 trong 2 nơi, gán vào hệ thống
                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    },

                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        var errorResponse = new ErrorResponse
                        {
                            ErrorCode = 401,
                            Message = new List<string> { "Token missing/invalid" }
                        };

                        var jsonResponse = JsonSerializer.Serialize(errorResponse);
                        return context.Response.WriteAsync(jsonResponse);
                    },

                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";

                        var errorResponse = new ErrorResponse
                        {
                            ErrorCode = 403,
                            Message = new List<string> { "Permission denied" }
                        };

                        var jsonResponse = JsonSerializer.Serialize(errorResponse);
                        return context.Response.WriteAsync(jsonResponse);
                    },
                };
            });
            builder.Services.AddEndpointsApiExplorer();

            //Add swagger with JWT auth
            builder.Services.AddSwaggerGen(option =>
            {
                option.DescribeAllParametersInCamelCase();
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
            });

            // Brevo HttpClient
            builder.Services.AddHttpClient("brevo", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration.GetSection("BrevoOptions").Get<BrevoOptions>().BaseUrl ?? "https://api.brevo.com/v3/");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            //CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    policy =>
                    {
                        policy.WithOrigins(
                            "http://localhost:5173",
                            "https://localhost:5173",
                            "https://dream-guard.vercel.app",
                            "http://127.0.0.1:5500" // Thêm localhost với cổng 5500 để test static files
                        )
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials() // QUAN TRỌNG: Cho phép cookies
                            .SetIsOriginAllowedToAllowWildcardSubdomains(); // Hỗ trợ SignalR
                    });
            });

            //Add DbContext
            var connectionString = builder.Configuration.GetConnectionString("DreamGuardConnection");
            builder.Services.AddDbContext<DreamGuardContext>(options =>
            {
                options.UseNpgsql(connectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            //Add IdentityUser and IdentityRole
            builder.Services.AddIdentityCore<User>()
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<DreamGuardContext>()
                .AddDefaultTokenProviders();

            // Cloudinary configuration
            builder.Services.AddSingleton(sp =>
            {
                // Lấy config đã được bind chuẩn ra khỏi DI Container
                var config = sp.GetRequiredService<IOptions<CloudinaryOptions>>().Value;

                // Khởi tạo Account bằng các thuộc tính (an toàn, có gợi ý code)
                var account = new Account(
                    config.CloudName,
                    config.ApiKey,
                    config.ApiSecret
                );

                return new Cloudinary(account);
            });

            // CẤU HÌNH GIỚI HẠN DUNG LƯỢNG REQUEST
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 52428800; // 50MB
                options.ValueLengthLimit = 52428800;
                options.MultipartHeadersLengthLimit = 52428800;
            });

            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.Limits.MaxRequestBodySize = 52428800; // 50MB
            });
            // Thêm SignalR service
            // Bật chế độ báo lỗi chi tiết của SignalR lên
            builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true; // <--- THÊM DÒNG NÀY
            });
            // Them HangFire
            builder.Services.AddHangfire(config =>
            config.UsePostgreSqlStorage(options => options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DreamGuardConnection"))));
            builder.Services.AddHangfireServer();
            //thêm DI cho hubservice và hangfire service
            builder.Services.AddScoped<IHubService, HubService>();
            builder.Services.AddScoped<IHangFireService, HangFireService>();

            builder.AddBLLServices();
            builder.AddDALServices();

            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();


            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            //Use exception handler
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    //lấy feature chứa exception
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature == null)
                    {
                        return;
                    }
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var errorResponse = new ErrorResponse
                    {
                        ErrorCode = 500,
                        Message = new List<string> { "Internal server error" }
                    };

                    var jsonResponse = JsonSerializer.Serialize(errorResponse);
                    await context.Response.WriteAsync(jsonResponse);
                });
            });

            // Use swagger, initialize database abd seed data in development environment
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                await app.Initialise();
                app.UseHttpsRedirection();
            }
            //test production environment with swagger and database initialization
            //app.UseSwagger();
            //app.UseSwaggerUI();
            //await app.Initialise();


            // CORS phải được đặt TRƯỚC Authentication/Authorization
            app.UseCors("AllowFrontend");
            // Use authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();
            // UI: /hangfire
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() }
            }); 
            


            app.MapControllers();
            // Map endpoint cho Hub
            app.MapHub<ChatHub>("/chathub");
            app.MapHub<SystemHub>("/systemhub");
            app.MapHub<NotiAndLogHub>("/notiandloghub");

            using (var scope = app.Services.CreateScope())
            {
                var hangfireService = scope.ServiceProvider.GetRequiredService<IHangFireService>();
                // Run daily at 01:00 UTC (08:00 AM VN time)
                hangfireService.AddOrUpdateRecurringJob<ICustomerCareService>(
                    "SendProductCareEmailsJob",
                    service => service.SendProductCareEmailsAsync(),
                    "0 1 * * *");
            }

            app.Run();
        }
    }
    public class HangfireAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
    {
        public bool Authorize(Hangfire.Dashboard.DashboardContext context)
        {
            // Chỉ cho phép truy cập dashboard nếu người dùng đã đăng nhập và có role "Admin"
            var httpContext = context.GetHttpContext();
            return httpContext.User.Identity != null && httpContext.User.Identity.IsAuthenticated && httpContext.User.IsInRole(Role.Admin);
        }
    }
}
