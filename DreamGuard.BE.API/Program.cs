using DreamGuard.BE.BLL;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Implements;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Options;
using DreamGuard.BE.DAL.Repositories.Implements;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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

            //Add authentication with JWT
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };

                options.Events = new JwtBearerEvents
                {
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
                    }
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
                .AddEntityFrameworkStores<DreamGuardContext>();
            //Add BrevoKey
            builder.Services.Configure<BrevoOptions>(builder.Configuration.GetSection(BrevoOptions.BrevoOptionsKey));
            //Add BLL services
            builder.AddBLLServices();

            builder.Services.AddHttpContextAccessor();

            //Add DI for BLL and DAL
            builder.Services.AddScoped<IIdentityService, IdentityService>();
            builder.Services.AddScoped<IAuthRepository, AuthRepository>();
            builder.Services.AddScoped<IOtpRepository, OtpRepository>();
            builder.Services.AddScoped<IOtpService, OtpService>();
            builder.Services.AddScoped<IBrevoEmailService, BrevoEmailService>();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IBabyProfileRepository, BabyProfileRepository>();
            builder.Services.AddScoped<IBabyProfileService, BabyProfileService>();
            builder.Services.AddScoped<IAddressRepository, AddressRepository>();
            builder.Services.AddScoped<IAddressService, AddressService>();
            builder.Services.AddScoped<IUserVoucherRepository, UserVoucherRepository>();
            builder.Services.AddScoped<IVoucherRepository, VoucherRepository>();
            builder.Services.AddScoped<IVoucherService, VoucherService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            builder.Services.AddScoped<DreamGuardDbContextInitialiser>();

            var app = builder.Build();
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
            }

            app.UseHttpsRedirection();
            // Use authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
