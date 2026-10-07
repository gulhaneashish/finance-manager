using FinanceManager.API.Data;
using FinanceManager.API.Exceptions;
using FinanceManager.API.Hubs;
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

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Read CORS allowed origins from environment variable (comma or semicolon separated) or appsettings
var envCors = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");
string[] allowedOrigins;
if (!string.IsNullOrWhiteSpace(envCors))
{
    allowedOrigins = envCors.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
else
{
    allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
}

// In local development, default to local Angular server if no origins specified
if (builder.Environment.IsDevelopment() && allowedOrigins.Length == 0)
{
    allowedOrigins = new[] { "http://localhost:4200", "https://localhost:4200" };
}

// In production, strictly reject fail-open CORS (empty origins or wildcard)
if (builder.Environment.IsProduction())
{
    if (allowedOrigins.Length == 0)
    {
        throw new InvalidOperationException(
            "CRITICAL SECURITY CONFIGURATION ERROR: No CORS allowed origins configured for Production. " +
            "Set the 'CORS_ALLOWED_ORIGINS' environment variable to your specific frontend URL(s). Fail-open CORS is strictly prohibited.");
    }

    if (allowedOrigins.Contains("*"))
    {
        throw new InvalidOperationException(
            "CRITICAL SECURITY CONFIGURATION ERROR: Wildcard '*' CORS origin is not permitted in Production. " +
            "Specify exact domains in 'CORS_ALLOWED_ORIGINS' (e.g. 'https://my-app.vercel.app').");
    }
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        if (builder.Environment.IsDevelopment() && allowedOrigins.Contains("*"))
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
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
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddSignalR();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });
// Database connection string from environment (DATABASE_URL, CONNECTION_STRING, or ConnectionStrings__DefaultConnection)
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("CRITICAL CONFIGURATION ERROR: Database connection string is missing. Please set 'ConnectionStrings__DefaultConnection' or 'DATABASE_URL' in the environment.");
}

builder.Services.AddDbContext<FinanceDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

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

// JWT Configuration from environment (JWT_KEY, JWT__KEY, or appsettings)
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
    ?? Environment.GetEnvironmentVariable("JWT__KEY")
    ?? builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    if (builder.Environment.IsProduction())
    {
        throw new InvalidOperationException("CRITICAL SECURITY ERROR: 'JWT_KEY' (or 'Jwt:Key') environment variable is mandatory in production.");
    }
    jwtKey = "LOCAL_DEV_FALLBACK_KEY_AT_LEAST_32_CHARS_LONG_123456789";
}

if (builder.Environment.IsProduction())
{
    if (jwtKey.Length < 32 || jwtKey.Contains("FALLBACK") || jwtKey.Contains("DEFAULT_SECRET") || jwtKey.Contains("CHANGE_THIS"))
    {
        throw new InvalidOperationException("CRITICAL SECURITY ERROR: In production, 'JWT_KEY' must be a strong, random secret of at least 32 characters (256 bits). Default/fallback keys are rejected.");
    }
}

var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? builder.Configuration["Jwt:Issuer"]
    ?? "FinanceManager.API";

var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? builder.Configuration["Jwt:Audience"]
    ?? "FinanceManager.Client";

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
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
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
    await db.Database.MigrateAsync();
    logger.LogInformation("Database migrations applied successfully.");
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Database migration failed. Application cannot start.");

    if (app.Environment.IsProduction())
    {
        throw;
    }
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

    // 2. Ensure dedicated Administrator account exists (configured via environment variables)
    var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL")
        ?? builder.Configuration["Admin:Email"];
    var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
        ?? builder.Configuration["Admin:Password"];

    // In local development only, fallback to dev credentials if not specified
    if (string.IsNullOrWhiteSpace(adminEmail) && builder.Environment.IsDevelopment())
    {
        adminEmail = "admin@financemanager.com";
        adminPassword = "Admin@123";
    }

    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
    {
        var existingAdmin = db.Users.FirstOrDefault(u => u.Email.ToLower() == adminEmail.ToLower());
        if (existingAdmin == null)
        {
            var adminName = Environment.GetEnvironmentVariable("ADMIN_NAME")
                ?? builder.Configuration["Admin:Name"]
                ?? "System Administrator";

            var dedicatedAdmin = new FinanceManager.API.Models.User
            {
                Name = adminName,
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
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
            logger.LogInformation("Dedicated Administrator account initialized from environment configuration ({AdminEmail}).", adminEmail);
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
        // 3. Ensure standard demo account exists for testing (demo@gmail.com / password123)
        var existingDemo = db.Users.FirstOrDefault(u => u.Email.ToLower() == "demo@gmail.com");
        if (existingDemo == null)
        {
            var demoUser = new FinanceManager.API.Models.User
            {
                Name = "Demo User",
                Email = "demo@gmail.com",
                Username = "demo",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(demoUser);
            db.SaveChanges();

            var demoCategories = new List<FinanceManager.API.Models.Category>
            {
                new() { UserId = demoUser.Id, Name = "Salary", Type = "INCOME" },
                new() { UserId = demoUser.Id, Name = "Freelance / Side Gig", Type = "INCOME" },
                new() { UserId = demoUser.Id, Name = "Investments & Dividends", Type = "INCOME" },
                new() { UserId = demoUser.Id, Name = "Other Income", Type = "INCOME" },
                new() { UserId = demoUser.Id, Name = "Food & Dining", Type = "EXPENSE" },
                new() { UserId = demoUser.Id, Name = "Groceries", Type = "EXPENSE" },
                new() { UserId = demoUser.Id, Name = "Housing & Rent", Type = "EXPENSE" },
                new() { UserId = demoUser.Id, Name = "Utilities & Bills", Type = "EXPENSE" },
                new() { UserId = demoUser.Id, Name = "Transportation & Fuel", Type = "EXPENSE" },
                new() { UserId = demoUser.Id, Name = "Shopping", Type = "EXPENSE" },
                new() { UserId = demoUser.Id, Name = "Healthcare & Medical", Type = "EXPENSE" },
                new() { UserId = demoUser.Id, Name = "Entertainment & Leisure", Type = "EXPENSE" }
            };
            db.Categories.AddRange(demoCategories);

            var demoAccount = new FinanceManager.API.Models.Account
            {
                UserId = demoUser.Id,
                Name = "Main Checking",
                AccountType = "SAVINGS",
                OpeningBalance = 10000m,
                IsActive = true
            };
            db.Accounts.Add(demoAccount);
            db.SaveChanges();

            logger.LogInformation("Demo account initialized (demo@gmail.com / password123).");
        }
        else
        {
            if (!BCrypt.Net.BCrypt.Verify("password123", existingDemo.PasswordHash))
            {
                existingDemo.PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123");
                existingDemo.IsActive = true;
                db.SaveChanges();
                logger.LogInformation("Demo account password reset to default.");
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

// Enable Swagger in Development, or if explicitly enabled by ENABLE_SWAGGER environment variable
var enableSwagger = app.Environment.IsDevelopment()
    || string.Equals(Environment.GetEnvironmentVariable("ENABLE_SWAGGER"), "true", StringComparison.OrdinalIgnoreCase)
    || app.Configuration.GetValue<bool>("EnableSwagger");

if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.Run();