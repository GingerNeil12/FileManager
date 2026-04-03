using FileManager.WebApi.Handlers;
using FileManager.WebApi.Option;
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.DarkMode = true;
        options.Theme = ScalarTheme.Saturn;
    });
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
