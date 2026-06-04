using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using HospitalManagement.Presentation.Middleware;
using HospitalManagement.BusinessLogic.Services;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.BusinessLogic.Validators;
using HospitalManagement.DataAccess.Repositories;
using HospitalManagement.DataAccess.Context;
using HospitalManagement.DataAccess.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

// ── Bootstrap Serilog logger before anything else ────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/hospital-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // ── Database ──────────────────────────────────────────────────────────────
    var pgConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddScoped<HospitalManagement.DataAccess.Interceptors.AuditInterceptor>();
    builder.Services.AddDbContext<AppDbContext>((sp, opts) =>
    {
        var interceptor = sp.GetRequiredService<HospitalManagement.DataAccess.Interceptors.AuditInterceptor>();
        opts.UseNpgsql(pgConnectionString,
            npgsqlOpts => npgsqlOpts.MigrationsAssembly("HospitalManagement.DataAccess"))
            .AddInterceptors(interceptor)
            .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
            .EnableDetailedErrors(builder.Environment.IsDevelopment());
    });


    // ── Repository / UnitOfWork ───────────────────────────────────────────────
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // ── Distributed Caching (Redis with Memory Fallback) ──────────────────────
    var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
    if (!string.IsNullOrWhiteSpace(redisConnectionString))
    {
        builder.Services.AddStackExchangeRedisCache(opts =>
        {
            opts.Configuration = redisConnectionString;
            opts.InstanceName = "HospitalHMS_";
        });
        Log.Information("Configured Redis Distributed Cache.");
    }
    else
    {
        builder.Services.AddDistributedMemoryCache();
        Log.Warning("Redis connection string not found. Falling back to In-Memory Distributed Cache.");
    }

    // ── Application Services ──────────────────────────────────────────────────
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IBillingService, BillingService>();
    builder.Services.AddScoped<IAdminService, AdminService>();
    builder.Services.AddScoped<IDoctorService, DoctorService>();
    builder.Services.AddScoped<IPatientService, PatientService>();
    builder.Services.AddScoped<IVisitService, VisitService>();
    builder.Services.AddScoped<IConsultationService, ConsultationService>();
    builder.Services.AddScoped<IAppointmentService, AppointmentService>();
    builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
    builder.Services.AddScoped<ILabReportService, LabReportService>();
    builder.Services.AddScoped<IEmrService, EmrService>();
    builder.Services.AddScoped<IQueueService, QueueService>();
    builder.Services.AddScoped<IScheduleService, ScheduleService>();
    builder.Services.AddScoped<IDepartmentService, DepartmentService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<IPatientConsentService, PatientConsentService>();
    builder.Services.AddScoped<IDocumentService, DocumentService>();
    builder.Services.AddScoped<ISystemSettingService, SystemSettingService>();
    builder.Services.AddScoped<IReportService, ReportService>();
    builder.Services.AddScoped<IDoctorReviewService, DoctorReviewService>();
    builder.Services.AddScoped<IDashboardService, DashboardService>();
    builder.Services.AddScoped<IChatService, ChatService>();
    builder.Services.AddScoped<IIpdService, IpdService>();
    builder.Services.AddScoped<IPharmacyService, PharmacyService>();

    // ── FluentValidation ──────────────────────────────────────────────────────
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

    // ── JWT Authentication ────────────────────────────────────────────────────
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"]
        ?? throw new InvalidOperationException("JwtSettings:SecretKey must be configured.");

    builder.Services.AddAuthentication(opts =>
    {
        opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization(opts => 
    {
        opts.AddPolicy(HospitalManagement.DataAccess.Constants.AppConstants.Policies.ViewAuditLogs, policy => 
            policy.RequireRole(HospitalManagement.DataAccess.Constants.AppConstants.Roles.Admin, "SuperAdmin"));
    });

    // ── Controllers ───────────────────────────────────────────────────────────
    builder.Services.AddControllers()
        .AddJsonOptions(opts =>
        {
            opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            // Preserve enum names (not ints) in JSON
            opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    // ── Swagger / OpenAPI ─────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSignalR(); // Add SignalR
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Hospital Management API",
            Version = "v1",
            Description = "Complete Hospital Appointment & Patient Management System API"
        });

        // JWT in Swagger
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token. Example: Bearer {token}"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    // ── CORS (development only — restrict in production) ──────────────────────
    builder.Services.AddCors(opts =>
        opts.AddPolicy("Development", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

    // ── File upload size limit (20 MB) ────────────────────────────────────────
    builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(opts =>
    {
        opts.MultipartBodyLengthLimit = 20 * 1024 * 1024;
    });

    // ── Build & Configure pipeline ────────────────────────────────────────────
    var app = builder.Build();

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<GlobalExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hospital Management API v1");
            c.RoutePrefix = "swagger";
        });
        app.UseCors("Development");
    }

    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    app.UseStaticFiles();   // For wwwroot/uploads
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    
    app.MapHub<HospitalManagement.BusinessLogic.Hubs.QueueHub>("/hubs/queue");
    app.MapHub<HospitalManagement.BusinessLogic.Hubs.ChatHub>("/hubs/chat");
    app.MapHub<HospitalManagement.Presentation.Hubs.NotificationHub>("/hubs/notifications");
    app.MapHub<HospitalManagement.Presentation.Hubs.AppointmentHub>("/hubs/appointments");

    // ── Auto-migrate in development ───────────────────────────────────────────
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }

    Log.Information("Hospital Management API starting on {Urls}", string.Join(", ", app.Urls));
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
