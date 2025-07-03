using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

public static class ServicesRegistration
{

    public static IServiceCollection RgisterServices(this IServiceCollection Services, IConfiguration Configuration)
    {
        Services.AddSwaggerGen(options =>
        {

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Paste Token"
            });


            options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

        Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(Configuration.GetConnectionString("DefaultConnectionstring")));


        Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();


        Services.Configure<JwtSettings>(Configuration.GetSection("Jwt"));

        var jwtConfig = Configuration.GetSection("Jwt").Get<JwtSettings>();

        Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(jwt =>
        {
            var Key = Encoding.ASCII.GetBytes(jwtConfig!.Key);
            jwt.SaveToken = true;
            jwt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                IssuerSigningKey = new SymmetricSecurityKey(Key),
                ValidIssuer = jwtConfig.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtConfig.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

        });

        Services.AddScoped<AuthTokenManager>();
        return Services;
    }


}