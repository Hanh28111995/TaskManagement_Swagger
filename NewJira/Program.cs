using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Application.Interfaces.Services;
using NewJira.Infrastructure.Data;
using NewJira.Infrastructure.Repositories;
using NewJira.Infrastructure.Services;
using NewJira.Hubs;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

var envListPath = Environment.GetEnvironmentVariable("ENV_LIST_PATH");

if (!string.IsNullOrWhiteSpace(envListPath))
{
    if (!File.Exists(envListPath))
    {
        throw new FileNotFoundException("Không tìm thấy file env-list.json.", envListPath);
    }

    builder.Configuration.AddJsonFile(
        envListPath,
        optional: false,
        reloadOnChange: false);
}

builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
{
    ["JwtSettings:Secret"] = builder.Configuration["JWT_SECRET_KEY"],
    ["EmailSettings:SenderEmail"] = builder.Configuration["EMAIL_USER"],
    ["EmailSettings:SenderPassword"] = builder.Configuration["EMAIL_PASS"],
    ["Cors:FrontendUrl"] = builder.Configuration["FRONT_END_URL"]
});

var firebaseCredentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
if (!string.IsNullOrWhiteSpace(firebaseCredentialsPath) && File.Exists(firebaseCredentialsPath))
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.GetApplicationDefault()
    });
}

builder.Services.AddDbContext<JiraDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProjectUserRepository, ProjectUserRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IStatusRepository, StatusRepository>();
builder.Services.AddScoped<IPriorityRepository, PriorityRepository>();
builder.Services.AddScoped<ITaskTypeRepository, TaskTypeRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();



builder.Services.AddSignalR();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
//var jwtSecret = jwtSettings["Secret"]
//    ?? throw new InvalidOperationException("JWT_SECRET_KEY chưa được cấu hình.");

var jwtSecret = "DefaultSuperSecretKeyForDevelopmentOnly123456789@";


var jwtKey = SHA256.HashData(Encoding.UTF8.GetBytes(jwtSecret));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Giữ nguyên tên Claim, không bị hệ thống tự đổi type
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = "role", 
            ValidateIssuer = false,
            ValidateAudience = false,
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("chat.group.create", p => p.RequireClaim("perm", "chat.group.create"));
    o.AddPolicy("user.manage", p => p.RequireClaim("perm", "user.manage"));
    o.AddPolicy("project.manage", p => p.RequireClaim("perm", "project.manage"));
    o.AddPolicy("task.update.all", p => p.RequireClaim("perm", "task.update.all"));
    o.AddPolicy("role.assign", p => p.RequireClaim("perm", "role.assign"));
});

var frontendUrlsString = builder.Configuration["Cors:FrontendUrl"];
var allowedOrigins = !string.IsNullOrWhiteSpace(frontendUrlsString)
    ? frontendUrlsString.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(url => url.Trim().TrimEnd('/')).ToArray()
    : Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins);
        }
        else
        {
            policy.AllowAnyOrigin();
        }

        policy.AllowAnyHeader()
            .AllowAnyMethod();

        if (allowedOrigins.Length > 0)
        {
            policy.AllowCredentials();
        }
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "NewJira API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

var enableSwagger = app.Environment.IsDevelopment()
    || builder.Configuration.GetValue<bool>("ENABLE_SWAGGER");

if (enableSwagger)
{
    app.UseSwagger(c =>
    {
        c.SerializeAsV2 = true;
    });

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NewJira API v1");
        c.RoutePrefix = "swagger";
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.Run();