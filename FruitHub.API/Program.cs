using System.Text;
using FruitHub.API.DTOs;
using FruitHub.API.Middlewares;
using FruitHub.ApplicationCore.Errors;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Interfaces.Services;
using FruitHub.ApplicationCore.Options;
using FruitHub.ApplicationCore.Services;
using FruitHub.Infrastructure.Identity;
using FruitHub.Infrastructure.Identity.Models;
using FruitHub.Infrastructure.Identity.Repositories;
using FruitHub.Infrastructure.Identity.Seeders;
using FruitHub.Infrastructure.Interfaces;
using FruitHub.Infrastructure.Interfaces.Repositories;
using FruitHub.Infrastructure.Interfaces.Services;
using FruitHub.Infrastructure.Persistence;
using FruitHub.Infrastructure.Persistence.UnitOfWork;
using FruitHub.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FruitHub.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        // DB
        var connectionString = builder.Configuration.GetConnectionString("SqlServer");
        builder.Services.AddDbContext<ApplicationDbContext>(
            options => options.UseSqlServer(connectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        
        builder.Services.AddDbContext<AppIdentityDbContext>(
            options => options.UseSqlServer(connectionString,
                b => b.MigrationsAssembly(typeof(AppIdentityDbContext).Assembly.FullName)));
        
        // Options

        var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
                         ?? throw new InvalidOperationException("JWT configuration is missing");
        builder.Services.Configure<JwtOptions>(
            builder.Configuration.GetSection("Jwt"));    
        
        builder.Services.Configure<RefreshTokenOptions>(
            builder.Configuration.GetSection("RefreshToken"));
        
        builder.Services.Configure<EmailOptions>(
            builder.Configuration.GetSection("EmailSettings"));

        // Identity
        builder.Services.AddIdentityCore<ApplicationUser>(options =>
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = true;

                    options.User.RequireUniqueEmail = true;
                    
                    options.User.AllowedUserNameCharacters =
                        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789._";
                    
                    options.SignIn.RequireConfirmedEmail = true; 
                }
            )
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();

        // Auth
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.RequireHttpsMetadata = false; // to dev env
                    options.SaveToken = true; // to can access token as a plain text
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtOptions.SigningKey)
                        ),
                    };
                }
            );
        
        builder.Services.AddAuthorization();

        // API Behavior
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .SelectMany(x => x.Value!.Errors.Select(e => new
                    {
                        Field = x.Key,
                        Error = e.ErrorMessage
                    })).ToList();

                return new BadRequestObjectResult(new ErrorResponse
                {
                    Code = ErrorsCode.ValidationError,
                    Message = "Validation failed",
                    Errors = errors
                });
            };
        });

        // DI
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        builder.Services.AddScoped<ICartService, CartService>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IUserFavoritesService, UserFavoritesService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IAuthService, JwtAuthService>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<IImageService, ImageService>();
        builder.Services.AddScoped<IImageValidator, ImageValidator>();
        
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IEmailConfirmationService, EmailConfirmationService>();
        builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IOtpService, OtpService>();
        builder.Services.AddScoped<IIdentityUserRepository, IdentityUserRepository>();
        
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<IAppCache, MemoryAppCache>();
        
        var app = builder.Build();
        
        // Seed Data
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            RoleSeeder.SeedRolesAsync(services).GetAwaiter().GetResult();
            IdentitySeeder.SeedAdminAsync(services).GetAwaiter().GetResult();
        }
        
        // Middleware pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        /*
         * I create this middleware to map the exceptions to status code and msg
         */
        app.UseMiddleware<ExceptionMiddleware>();
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapControllers();
        app.Run();
    }
}