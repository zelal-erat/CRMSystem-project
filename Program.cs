

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

// 1️⃣ DbContext
builder.Services.AddDbContext<CRMDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
        var jwtSection = builder.Configuration.GetSection("Jwt");
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var key = jwtSection["Key"]!;

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
        {
            securityScheme,
            new List<string>()
        }
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

// 9️⃣ Background Services & Email
// Not: Bu servisler henüz implement edilmedi
// builder.Services.AddScoped<CRMSystem.Domain.Services.IAdvancedRecurringService, CRMSystem.Domain.Services.AdvancedRecurringService>();
// builder.Services.AddScoped<IEmailService, EmailService>();
// builder.Services.AddHostedService<RecurringInvoiceBackgroundService>();

// 🔟 Global Exception Handler
builder.Services.AddTransient<GlobalExceptionHandler>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// 8️⃣ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:5173") // frontend portu
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

app.UseAuthentication();   // ❗ olmalı
app.UseAuthorization();

app.MapControllers();

// Seed default roles and admin user
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    string[] roles = ["Admin", "Staff"];
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
    var adminSection = builder.Configuration.GetSection("AdminUser");
    var adminEmail = adminSection["Email"];
    var adminPassword = adminSection["Password"];
    var adminFullName = adminSection["FullName"];    
    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
    {
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = adminFullName ?? "Admin" };
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
