using BookApplicationApi;
using BookData;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Serilog;
using System.Net;
using System.Security.Claims;
using System.Text;
using testbook.ConfigurationClasses;
using testbook.MiddleWare;
using testbook.ModelData;

var builder = WebApplication.CreateBuilder(args);

//Configurationlogging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
    .CreateLogger();


// Add Database
var defaultConnection = builder.Configuration["ConnectionStrings:DefaultConnection"] ?? string.Empty;
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseNpgsql(defaultConnection));
// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddBookServices();
builder.Services.AddBookData(builder.Configuration);

// Configuration SecretKey in appsetting.json
var configuration = builder.Configuration;
builder.Services.Configure<Appsetting>(builder.Configuration.GetSection("Appsetting"));
var secretkey = configuration["Appsetting:SecretKey"];

// Configuration JWT Authentication
var key = Encoding.UTF8.GetBytes(secretkey ?? string.Empty);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,

        ValidateIssuerSigningKey = true,

        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero,
    };
});
//Rate Limit
builder.Services.AddDistributedMemoryCache();
//Retry
builder.Services.AddHttpClient("RetryClient")
    .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder
        .OrResult(response => response.StatusCode == HttpStatusCode.NotFound)
        .WaitAndRetryAsync(3, retryAttempt =>
        {
            var waitTime = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
            string logg = $"Retry attempt {retryAttempt}, waiting for {waitTime.TotalSeconds} seconds";
            Log.Information(logg);
            return waitTime;
        })
    );
//AUthorization
builder.Services.AddAuthorizationBuilder()
                   //AUthorization
                   .AddPolicy("RequireUserRole", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => (c.Type == ClaimTypes.Role && c.Value == "User") ||
                                       (c.Type == ClaimTypes.Role && c.Value == "Admin"))))
                   //AUthorization
                   .AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Admin"));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OrderActionsBy(apiDesc =>
    {
        var httpMethodOrder = new Dictionary<string, int>
        {
            { "GET", 1 },
            { "PUT", 3 },
            { "DELETE", 4 },
            { "POST", 2 },
        };
        var httpMethod = apiDesc.HttpMethod;
        var order = httpMethodOrder.ContainsKey(httpMethod ?? string.Empty) ? httpMethodOrder[httpMethod ?? string.Empty] : 5;
        var finalOrder = order.ToString();
        return finalOrder;
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});
//Basic Authentication
builder.Services.Configure<BasicAuthenticationOptions>(builder.Configuration.GetSection("BasicAuthentication"));
builder.Services.Configure<BasicAuthenticationOptions>(builder.Configuration.GetSection("BasicAuthentication"));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseMiddleware<BasicAuthenticationMiddleware>();
if (app.Environment.IsDevelopment())
{

}
app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<RateLimitMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();

