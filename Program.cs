using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scoutify.FeaturesApi.Configs;
using Scoutify.FeaturesApi.Services;

var builder = WebApplication.CreateBuilder(args);

var jwtSection = builder.Configuration.GetSection("Jwt");
var signingKey = jwtSection["SigningKey"]
                 ?? Environment.GetEnvironmentVariable("JWT__SIGNINGKEY")
                 ?? throw new InvalidOperationException("Jwt:SigningKey (or JWT__SIGNINGKEY) is required, min 32 characters.");
if (signingKey.Length < 32)
{
    throw new InvalidOperationException("JWT signing key must be at least 32 characters.");
}

var signingBytes = Encoding.UTF8.GetBytes(signingKey);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"] ?? "scoutify-auth",
            ValidAudience = jwtSection["Audience"] ?? "scoutify-clients",
            IssuerSigningKey = new SymmetricSecurityKey(signingBytes),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();
builder.Services.Configure<StocksApiOptions>(builder.Configuration.GetSection("StocksApi"));
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IFeatureDataService, FeatureDataService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHealthChecks("/health");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
