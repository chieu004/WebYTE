using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebYTE.Application.Interfaces;
using WebYTE.Core.Entities;
using WebYTE.Infrastructure.AI;
using WebYTE.Infrastructure.Data;
using WebYTE.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? "super_secret_key_webyte_2026_!@#$%");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    
    // Configure JWT for SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
            {
                context.Token = accessToken;
            }
            
            return Task.CompletedTask;
        }
    };
});

// 4. DI Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

// AI Services - Tăng timeout cho LM Studio (model có thể chậm)
builder.Services.AddHttpClient<IAiTriageService, AiTriageService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5); // 5 phút cho model chậm
});
builder.Services.AddHttpClient<IAiDiagnosisService, AiDiagnosisService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5); // 5 phút cho model chậm
});

// Background Service for Appointment Reminders
builder.Services.AddHostedService<AppointmentReminderService>();

// SignalR
builder.Services.AddSignalR();

builder.Services.AddControllers();

// 5. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 6. CORS - cho phép Blazor Client truy cập
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorCors", policy =>
    {
        policy
            .WithOrigins("http://localhost:5191", "https://localhost:5192")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Required for SignalR
    });
});

var app = builder.Build();

// 7. Auto migrate database + seed data khi startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        // Tự động apply tất cả pending migrations
        await db.Database.MigrateAsync();
        
        // Seed Roles + Admin account
        await DataSeeder.SeedRolesAndAdminAsync(services);
        
        // Seed Sample Data (Specialties, Doctors, Patients, Staff, Appointments)
        await DataSeeder.SeedSampleDataAsync(services);
        
        Console.WriteLine("Database migrated and seeded successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"DB setup error: {ex.Message}");
    }
}

app.UseSwagger();
app.UseSwaggerUI();

// CORS phải đặt TRƯỚC Authentication/Authorization
app.UseCors("BlazorCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<WebYTE.Infrastructure.Hubs.NotificationHub>("/notificationHub");

app.Run();
