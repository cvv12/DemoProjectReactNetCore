using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using webapi.Filters;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Services;
using Serilog;
using webapi.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(options =>
{
    options.Filters.Add<HttpResponseExceptionFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//Dependency Injection
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddMemoryCache();
// Configure CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", builder =>
    {
        builder.WithOrigins("http://localhost:3000")
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Initialize with the configuration
IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(app.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json")
    .Build();
EncryptionHelper.Initialize(configuration);
GeneratorHelper.Initialize(configuration);
ValidationHelper.Initialize(configuration);

app.UseRouting(); 

app.UseCors("AllowReactApp"); 

app.UseAuthorization();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>(); //Only for update account balance method
app.MapControllers();

app.Run();
