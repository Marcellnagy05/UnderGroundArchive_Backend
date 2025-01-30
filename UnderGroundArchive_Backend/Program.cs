using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using UnderGroundArchive_Backend.Dbcontext;
using UnderGroundArchive_Backend.Models;
using UnderGroundArchive_Backend.Services;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddDbContext<UGA_DBContext>(options =>
            options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
                new MySqlServerVersion(new Version(8, 0, 33)))); // Replace with your MySQL version

        builder.Services.AddScoped<IBookService, BookService>();

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<UGA_DBContext>()
        .AddDefaultTokenProviders();

        // AddJwtBearer beállítás
        var secretKey = Encoding.UTF8.GetBytes("SA5Tq6PMb/6UKyx7IPCe7c1kISP3wnSoyH/mFeZzxoM=");
        builder.Services.AddAuthentication(options =>
        {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
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
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey)
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        var result = JsonSerializer.Serialize(new { message = "Token érvénytelen vagy hiányzik." });
                        return context.Response.WriteAsync(result);
                    }
                };
            }).AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
                googleOptions.Scope.Add("https://www.googleapis.com/auth/user.phonenumbers.read");
            });


        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Events.OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = 401; // Ne irányítsa át, csak adjon vissza 401-et
                    return Task.CompletedTask;
                }
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };
        });

        // CORS beállítása
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowLocalhost", builder =>
            {
                builder.WithOrigins("http://localhost:5173")
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials();
            });
        });

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        // CORS beállítás alkalmazása a UseRouting előtt
        app.UseCors("AllowLocalhost");  // CORS szabályok alkalmazása
        app.UseRouting();  // Routing beállítása

        app.UseAuthentication(); // Bejelentkezés (JWT alapú)
        app.UseAuthorization();  // Jogosultságok ellenőrzése
        app.UseStaticFiles();  // Statikus fájlok kezelése (ha van)

        app.MapControllers();

        app.Run();
    }
}
