using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Api.Filters;
using ProductCatalog.Api.Middleware;
using ProductCatalog.Application.Interfaces.Caching;
using ProductCatalog.Application.Interfaces.Products;
using ProductCatalog.Application.Services.Products;
using ProductCatalog.Application.Validators.Products;
using ProductCatalog.Infrastructure.Caching;
using ProductCatalog.Infrastructure.Persistence;
using ProductCatalog.Infrastructure.Persistence.Repositories;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//RegisterSQL
builder.Services.AddDbContext<ProductCatalogDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

//Register Redis
var redisConnectionString =
    builder.Configuration["Redis:ConnectionString"];

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnectionString!));

//Redis CacheLock
builder.Services.AddSingleton<ICacheLock ,ProductCacheLock>();

//Register DI
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICacheService, RedisCacheService>();

//Register FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();

var app = builder.Build();

//Register Global Exception Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.

    app.UseSwagger();
    app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
