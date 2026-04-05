using FileManager.WebApi.Handlers;
using FileManager.WebApi.Option;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);
builder.Services.AddOpenApi();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers();

builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder
    .Services
    .Configure<ApplicationInfo>(builder.Configuration.GetSection(ApplicationInfo.SectionName));

var corsSection = builder.Configuration.GetSection(CorsOptions.SectionName);
builder.Services.Configure<CorsOptions>(corsSection);

builder.Services.AddCors(options =>
{
    var corsConfig = corsSection.Get<CorsOptions>() ?? new CorsOptions();

    options.AddPolicy(CorsOptions.PolicyName, policy =>
    {
        policy.WithOrigins(corsConfig.AllowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var auth0Section = builder.Configuration.GetSection(Auth0Options.SectionName);
builder.Services.Configure<Auth0Options>(auth0Section);
var auth0Config = auth0Section.Get<Auth0Options>() ?? new Auth0Options();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://{auth0Config.Domain}/";
        options.Audience = auth0Config.Audience;
    });

builder.Services.AddAuthorization(options =>
{
    var requireAuth = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.DefaultPolicy = requireAuth;
    options.FallbackPolicy = requireAuth;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference(options =>
    {
        options.DarkMode = true;
        options.Theme = ScalarTheme.Saturn;
    });
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors(CorsOptions.PolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

public partial class Program { }
