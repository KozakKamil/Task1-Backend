using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Task1___Backend;
using Task1___Backend.Models.Validators;
using Task1Backend;
using Task1Backend.Entities;
using Task1Backend.Models;
using Task1Backend.Services;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var authenticationSettings = new AuthenticationSettings();

        // Add services to the container.
        builder.Services.AddControllers().AddFluentValidation();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("connectionString")));
        builder.Services.AddScoped<UserSeeder>();
        builder.Services.AddScoped<IAccountService, AccountService>();
        builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        builder.Services.AddScoped<IValidator<RegisterUserDto>, RegisterUserDtoValidator>();

        builder.Services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme = "Bearer";
            option.DefaultScheme = "Bearer";
            option.DefaultChallengeScheme = "Bearer";
        }).AddJwtBearer(cfg =>
        {
            cfg.RequireHttpsMetadata = false;
            cfg.SaveToken = true;
            cfg.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = authenticationSettings.JwtIssuer,
                ValidAudience = authenticationSettings.JwtIssuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.JwtKey))
            };
        });

        builder.Services.AddSingleton(authenticationSettings);
        var app = builder.Build();

        //Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseAuthentication();
        app.UseHttpsRedirection();

        app.UseCors(builder =>
        {
            builder.WithOrigins("http://localhost:3000")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });

        app.UseAuthorization();
        app.MapControllers();
        //Database connection
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<DataContext>();
            var seeder = services.GetRequiredService<UserSeeder>();
            var configuration = services.GetRequiredService<IConfiguration>();
            configuration.GetSection("Authentication").Bind(authenticationSettings);

            if (context.Database.CanConnect())
            {
                Console.WriteLine("Database connected.");

                if (!context.Users.Any())
                {
                    seeder.Seed();
                    Console.WriteLine("Seed data added.");
                }
                else
                {
                    Console.WriteLine("Database already contains data. Skipping seed data.");
                }
            }
            else
            {
                Console.WriteLine("Database connection failed.");
            }
        }
        app.Run();
    }
}