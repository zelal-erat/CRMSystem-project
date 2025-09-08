using CRMSystem.Infrastructure;
using CRMSystem.Domain.Interfaces;
using CRMSystem.Infrastructure.Repositories;
using CRMSystem.Application.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MediatR;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using CRMSystem.Application.Common.Interfaces;
using CRMSystem.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ DbContext (Env variable kullan)
var connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<CRMDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2️⃣ Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<CRMDbContext>()
    .AddDefaultTokenProviders();

// 3️⃣ JWT Authentication (Env variable kullan)
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
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "CRMSystem API", Version = "v1" });
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };
    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new List<string>() }
    });
});

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
builder.Services.AddScoped<CRMSystem.Domain.Services.IInvoiceDomainService, CRMSystem.Domain.Services.InvoiceDomainService>();
builder.Services.AddScoped<CRMSystem.Domain.Services.ICustomerDomainService, CRMSystem.Domain.Services.CustomerDomainService>();
builder.Services.AddScoped<CRMSystem.Domain.Services.IServiceDomainService, CRMSystem.Domain.Services.ServiceDomainService>();

// 🔟 Global Exception Handler
builder.Services.AddTransient<GlobalExceptionHandler>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// 9️⃣ CORS (Prod için frontend URL kullan)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins(Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});

var app = builder.Build();

// Middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.UseCors("AllowFrontend");

// Global Exception Handler
app.UseMiddleware<GlobalExceptionHandler>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed default roles and admin user
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "Admin", "Staff" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
    var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
    var adminFullName = Environment.GetEnvironmentVariable("ADMIN_FULLNAME") ?? "Admin";

    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
    {
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = adminFullName
            };
            var createResult = await userManager.CreateAsync(admin, adminPassword);
            if (createResult.Succeeded)
            {
                if (!await userManager.IsInRoleAsync(admin, "Admin"))
                    await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
        else if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}

app.Run();
