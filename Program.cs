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

// 1️⃣ DbContext - environment variable'dan al
var connectionString = builder.Configuration["CONNECTIONSTRINGS_DEFAULTCONNECTION"];
builder.Services.AddDbContext<CRMDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2️⃣ Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<CRMDbContext>()
    .AddDefaultTokenProviders();

// 3️⃣ JWT Authentication
var jwtKey = builder.Configuration["JWT_KEY"];
var jwtIssuer = builder.Configuration["JWT_ISSUER"] ?? "CRMSystem";
var jwtAudience = builder.Configuration["JWT_AUDIENCE"] ?? "CRMSystemClient";

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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// 4️⃣ Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CRMSystem API", Version = "v1" });
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// 9️⃣ CORS
var frontendUrl = builder.Configuration["FRONTEND_URL"] ?? "http://localhost:5173";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins(frontendUrl)
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

    var adminEmail = builder.Configuration["ADMIN_EMAIL"];
    var adminPassword = builder.Configuration["ADMIN_PASSWORD"];
    var adminFullName = builder.Configuration["ADMIN_FULLNAME"] ?? "Admin";

    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
    {
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = adminFullName };
            var createResult = await userManager.CreateAsync(admin, adminPassword);
            if (createResult.Succeeded && !await userManager.IsInRoleAsync(admin, "Admin"))
                await userManager.AddToRoleAsync(admin, "Admin");
        }
        else if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}

app.Run();
