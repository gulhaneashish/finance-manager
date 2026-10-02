using FinanceManager.API.Data;
using FinanceManager.API.Exceptions;
using FinanceManager.API.Repositories;
using FinanceManager.API.Repositories.Interfaces;
using FinanceManager.API.Services;
using FinanceManager.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json.Serialization;
var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:4200", "https://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        if (allowedOrigins.Contains("*"))
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});


builder.Services.AddExceptionHandler<
    GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

// ==========================================
// REPOSITORIES
// ==========================================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<ICategoryBudgetRepository, CategoryBudgetRepository>();
builder.Services.AddScoped<ILoanRepository, LoanRepository>();
builder.Services.AddScoped<ILoanPaymentRepository, LoanPaymentRepository>();
builder.Services.AddScoped<IInvestmentRepository, InvestmentRepository>();

// ==========================================
// SERVICES
// ==========================================
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAccountBalanceService, AccountBalanceService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IMonthlyReportService, MonthlyReportService>();
builder.Services.AddScoped<INetWorthService, NetWorthService>();
builder.Services.AddScoped<ICreditCardService, CreditCardService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IInvestmentService, InvestmentService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });
builder.Services.AddDbContext<FinanceDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});

builder.Services.AddAuthentication(
    JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"] ?? "FinanceManager.API",

                ValidAudience =
                    builder.Configuration["Jwt:Audience"] ?? "FinanceManager.Client",

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:Key"]
                            ?? "THIS_IS_A_FALLBACK_DEFAULT_SECRET_KEY_FOR_JWT_SIGNING_123456789"))
            };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Ensure database schema and migrations are applied, and dedicated Admin profile is seeded
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        // Automatically create and migrate tables on cloud deployment
        db.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Database migration skipped or encountered an error. Proceeding with application startup.");
    }

    try
    {
        var users = db.Users.ToList();
        bool changed = false;

    // 1. Ensure all regular users have active status and the first user (demo) is a standard User
    foreach (var u in users)
    {
        if (!u.IsActive)
        {
            u.IsActive = true;
            changed = true;
        }

        // Revert demo account or any non-dedicated admin account to regular User
        if (u.Email.Equals("demo@gmail.com", StringComparison.OrdinalIgnoreCase) && u.Role == "Admin")
        {
            u.Role = "User";
            changed = true;
        }
        else if (string.IsNullOrWhiteSpace(u.Role))
        {
            u.Role = "User";
            changed = true;
        }
    }

    if (changed)
    {
        db.SaveChanges();
    }

    // 2. Ensure dedicated Administrator account exists
    const string adminEmail = "admin@financemanager.com";
    var existingAdmin = db.Users.FirstOrDefault(u => u.Email.ToLower() == adminEmail.ToLower());
    if (existingAdmin == null)
    {
        var dedicatedAdmin = new FinanceManager.API.Models.User
        {
            Name = "System Administrator",
            Email = adminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(dedicatedAdmin);
        db.SaveChanges();

        db.AuditLogs.Add(new FinanceManager.API.Models.AuditLog
        {
            UserId = dedicatedAdmin.Id,
            UserEmail = dedicatedAdmin.Email,
            Action = "SYSTEM_INIT",
            Details = $"Dedicated System Administrator profile initialized ({adminEmail}).",
            Timestamp = DateTime.UtcNow
        });
        db.SaveChanges();
    }
    else
    {
        if (existingAdmin.Role != "Admin" || !existingAdmin.IsActive)
        {
            existingAdmin.Role = "Admin";
            existingAdmin.IsActive = true;
            db.SaveChanges();
        }
    }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "User initialization skipped or encountered an error. Proceeding with application startup.");
    }
}

app.UseCors("AngularClient");
app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();