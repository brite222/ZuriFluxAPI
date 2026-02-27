using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;
using ZuriFluxAPI.Repositories;
using ZuriFluxAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────
builder.Services.AddDbContext<ZuriFluxDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Repositories ──────────────────────────────────────
builder.Services.AddScoped<IBinRepository, BinRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICollectionRepository, CollectionRepository>();
builder.Services.AddScoped<ICreditRepository, CreditRepository>();
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

// ── Services ──────────────────────────────────────────
builder.Services.AddScoped<IBinService, BinService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICollectionService, CollectionService>();
builder.Services.AddScoped<ICreditService, CreditService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// ── JWT Authentication ─────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

// ── Controllers + Validation ───────────────────────────
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .SelectMany(e => e.Value.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return new BadRequestObjectResult(new
            {
                status = 400,
                message = "Validation failed.",
                errors = errors,
                timestamp = DateTime.UtcNow
            });
        };
    });

// ── Swagger ────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "ZuriFlux API",
        Version = "v1",
        Description = "AI-Powered Waste Management API for Lagos. Built by ZuriFlux GreenTech Ltd."
    });

    // Pick up XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // JWT in Swagger
    options.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Paste your JWT token here. Get it from POST /api/Auth/login"
    });

    options.AddSecurityRequirement(new()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS - allow frontend and mobile to call your API
builder.Services.AddCors(options =>
{
    options.AddPolicy("ZuriFluxPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()     // during development - allows all
            .AllowAnyMethod()     // GET, POST, PUT, DELETE, PATCH
            .AllowAnyHeader();    // Authorization, Content-Type, etc.
    });
});

// ── Build ──────────────────────────────────────────────
var app = builder.Build();

// Order matters here
app.UseMiddleware<ExceptionMiddleware>();  // catch all errors first
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();                  // who are you?
app.UseAuthorization();                   // are you allowed?
app.MapControllers();
app.UseCors("ZuriFluxPolicy");
app.Run();