using System.Text;
using FruitHub.API.DTOs;
using FruitHub.API.Middlewares;
using FruitHub.ApplicationCore.Errors;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Interfaces.Services;
using FruitHub.ApplicationCore.Options;
using FruitHub.ApplicationCore.Services;
using FruitHub.Infrastructure.Identity;
using FruitHub.Infrastructure.Identity.Repositories;
using FruitHub.Infrastructure.Interfaces;
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

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddDbContext<ApplicationDbContext>(
            options => options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        
        builder.Services.AddDbContext<AppIdentityDbContext>(
            options => options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"),
                b => b.MigrationsAssembly(typeof(AppIdentityDbContext).Assembly.FullName)));
        
        // Bind Jwt section to JwtOptions
        var jwtOptions = builder.Configuration
            .GetSection("Jwt")
            .Get<JwtOptions>();

        builder.Services.Configure<JwtOptions>(
            builder.Configuration.GetSection("Jwt"));    
        
        builder.Services.Configure<RefreshTokenOptions>(
            builder.Configuration.GetSection("RefreshToken"));
        
        builder.Services.Configure<EmailOptions>(
            builder.Configuration.GetSection("EmailSettings"));

        builder.Services.AddIdentityCore<ApplicationUser>(options =>
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    // options.Password.RequiredUniqueChars = ;
                    options.User.RequireUniqueEmail = true;
                    options.User.AllowedUserNameCharacters =
                        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789._";
                    // options.Lockout.MaxFailedAccessAttempts = 5;
                    // options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    // options.Lockout.AllowedForNewUsers = true;
                    options.SignIn.RequireConfirmedEmail = true; // Email confirmation required
                    // options.SignIn.RequireConfirmedPhoneNumber = false;// Phone confirmation required
                    // options.SignIn.RequireConfirmedAccount = false;    // Account confirmation required
                    // options.Tokens.AuthenticatorTokenProvider = 
                    //     TokenOptions.DefaultAuthenticatorProvider;
                    //
                    // options.Tokens.EmailConfirmationTokenProvider =
                    //     TokenOptions.DefaultEmailProvider;
                    //
                    // options.Tokens.PasswordResetTokenProvider =
                    //     TokenOptions.DefaultEmailProvider;
                    //
                    // options.Tokens.ChangeEmailTokenProvider =
                    //     TokenOptions.DefaultEmailProvider;
                    //
                    // options.Tokens.ChangePhoneNumberTokenProvider =
                    //     TokenOptions.DefaultPhoneProvider;
                }
            )
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();

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
                        // LifetimeValidator = 
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtOptions.SigningKey)
                        ),
                    };
                }
            );
        
        builder.Services.AddAuthorization();

        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .SelectMany(x => x.Value.Errors.Select(e => new
                    {
                        Field = x.Key,
                        Error = e.ErrorMessage
                    })).ToList();

                var response = new ErrorResponse
                {
                    Code = ErrorsCode.ValidationError,
                    Message = "Validation failed",
                    Errors = errors
                };
                return new BadRequestObjectResult(response);
            };
        });

        
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
        
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            RoleSeeder.SeedRolesAsync(services)
                .GetAwaiter()
                .GetResult();
            IdentitySeeder.SeedAdminAsync(services)
                .GetAwaiter()
                .GetResult();
        }
        
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        /*
         * I create this middleware to map the exceptions to right status code and msg
         */
        app.UseMiddleware<ExceptionMiddleware>();
        
        app.MapControllers();

        app.Run();
    }
}