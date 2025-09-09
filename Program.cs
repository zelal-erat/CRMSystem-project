using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using CRMSystem.Infrastructure;
using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Interfaces;
using CRMSystem.Infrastructure.Repositories;
using CRMSystem.Application.Validators;
using CRMSystem.Domain.Services;
using CRMSystem.Application.Common.Interfaces;
using CRMSystem.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ DB Context (Render env variable kullan)
var connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<CRMDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2️⃣ Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<CRMDbContext>()
    .AddDefaultTokenProviders();

// 3️⃣ JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"];
    var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"];
    var key = Environment.GetEnvironmentVariable("JWT_KEY") ?? builder.Configuration["Jwt:Key"];

    if (string.IsNullOrWhiteSpace(key))
        throw new Exception("JWT_KEY environment variable is missing!");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

// 4️⃣ Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 5️⃣ Repository Pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();

// 6️⃣ MediatR & FluentValidation
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// 7️⃣ AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// 8️⃣ Domain Services
builder.Services.AddScoped<IInvoiceDomainService, InvoiceDomainService>();
builder.Services.AddScoped<ICustomerDomainService, CustomerDomainService>();
builder.Services.AddScoped<IServiceDomainService, ServiceDomainService>();

// 9️⃣ JWT Service
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// 9️⃣ CORS (Render frontend URL)
builder.Services.AddCors(options =>
{
    var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:5173";
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(frontendUrl)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Database Migration (Render için)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<CRMDbContext>();
        await context.Database.MigrateAsync();
        Console.WriteLine("Database migration completed successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration failed: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
}

// Middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// 10️⃣ Admin Seed (try-catch ile güvenli)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

       
        string[] roles = { "Admin", "Staff" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (result.Succeeded)
                    Console.WriteLine($"Role '{role}' created successfully.");
                else
                    Console.WriteLine($"Failed to create role '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            else
            {
                Console.WriteLine($"Role '{role}' already exists.");
            }
        }

        // Admin kullanıcısını oluştur
        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
        var adminFullName = Environment.GetEnvironmentVariable("ADMIN_FULLNAME") ?? "Admin";

        if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
        {
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = adminFullName };
                var createResult = await userManager.CreateAsync(admin, adminPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                    Console.WriteLine($"Admin user '{adminEmail}' created successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to create admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine($"Admin user '{adminEmail}' already exists.");
            }
        }
        else
        {
            Console.WriteLine("Admin credentials not provided in environment variables.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Admin seed failed: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
}

// 11️⃣ Port ayarı (Render kullanımı)
var port = Environment.GetEnvironmentVariable("PORT") ?? "80";
app.Urls.Add($"http://0.0.0.0:{port}");

app.MapControllers();
await app.RunAsync();
