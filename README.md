# ProductCatalog

A simple Product Catalog REST API built with ASP.NET Core 8.

This project was built to practice backend development concepts such as Clean Architecture, Entity Framework Core, Redis caching, pagination, validation, exception handling, Docker and Health Checks.

## Tech Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- Redis
- StackExchange.Redis
- FluentValidation
- Swagger / OpenAPI
- Docker
- Docker Compose

## Project Structure

```text
ProductCatalog
│
├── ProductCatalog.Api
├── ProductCatalog.Application
├── ProductCatalog.Domain
└── ProductCatalog.Infrastructure
```

### ProductCatalog.Api

Contains:

- Controllers
- Middleware
- Filters
- Swagger configuration
- Dependency Injection and application startup

### ProductCatalog.Application

Contains:

- DTOs
- Interfaces
- Services
- Validators
- Application logic

### ProductCatalog.Domain

Contains the domain entities.

### ProductCatalog.Infrastructure

Contains:

- EF Core DbContext
- Repositories
- SQL Server configuration
- Redis implementation
- Cache services

## Features

### Product CRUD

The API supports:

- Create product
- Get product by ID
- Get products
- Update product
- Delete product

### Pagination

Product listing supports pagination using `page` and `pageSize`.

Example:

```text
GET /api/Products?page=1&pageSize=10
```

### Search

Products can be searched by name or description.

Example:

```text
GET /api/Products?search=gaming
```

### Sorting

Products can be sorted by different fields and sort orders.

Example:

```text
GET /api/Products?sortBy=price&sortOrder=desc
```

### Redis Caching

Redis is used to cache:

- Individual products
- Product list results

Individual product cache keys use:

```text
product:{id}
```

Product list cache keys contain the query parameters:

```text
products:page=1:size=10:search=gaming:sort=price:order=desc
```

Cached data has a TTL of 5 minutes.

The project also uses a cache lock with a double-check strategy when loading a product from the database after a cache miss.

### Cache Invalidation

When a product is created, updated or deleted, the related product list cache is removed.

When a product is updated or deleted, its individual cache is also removed.

### Validation

FluentValidation is used for validating product requests.

For example:

- Price must be greater than zero
- Stock cannot be negative

Invalid requests return `400 Bad Request`.

### Global Exception Handling

Unhandled exceptions are handled using a global exception middleware.

The middleware logs the exception and returns a consistent response:

```json
{
  "statusCode": 500,
  "message": "An unexpected error occurred."
}
```

Logging is implemented using `ILogger`.

### Health Checks

Health Checks are configured for:

- SQL Server
- Redis

Health endpoint:

```text
GET /health
```

Example response:

```text
Healthy
```

SQL Server and Redis also have Docker health checks configured in Docker Compose.

## Docker

The complete application can run using Docker Compose.

The following services are included:

```text
ProductCatalog API
SQL Server
Redis
```

Start the application:

```bash
docker compose up -d --build
```

Check running containers:

```bash
docker ps
```

The API runs on:

```text
http://localhost:8080
```

Swagger:

```text
http://localhost:8080/swagger
```

Health Check:

```text
http://localhost:8080/health
```

Stop the containers:

```bash
docker compose down
```

View API logs:

```bash
docker logs productcatalog-api
```

## Database

SQL Server runs inside Docker.

The SQL Server container is exposed on:

```text
localhost:1550
```

When the API runs inside Docker, it connects to SQL Server using:

```text
Server=sqlserver,1433
```

The database is managed using Entity Framework Core migrations.

To apply migrations:

```bash
dotnet ef database update --project ProductCatalog.Infrastructure --startup-project ProductCatalog.Api
```

## Redis

Redis runs inside Docker and is exposed on:

```text
localhost:6379
```

When the API runs inside Docker, it connects to Redis using:

```text
redis:6379
```

## Running Locally

Make sure SQL Server and Redis are running.

Then run:

```bash
dotnet run --project ProductCatalog.Api
```

The API will be available on the URL shown by ASP.NET Core.

Swagger is available at:

```text
https://localhost:7256/swagger
```

## Running with Docker

The easiest way to run the complete project is:

```bash
docker compose up -d --build
```

After the containers are started:

```text
Swagger:
http://localhost:8080/swagger

Health:
http://localhost:8080/health
```

Check the containers:

```bash
docker ps
```

Expected services:

```text
productcatalog-api
productcatalog-sqlserver
productcatalog-redis
```

## API Endpoints

### Products

```text
GET     /api/Products
GET     /api/Products/{id}
POST    /api/Products
PUT     /api/Products/{id}
DELETE  /api/Products/{id}
```

### Health

```text
GET /health
```

Swagger can be used to test all API endpoints:

```text
http://localhost:8080/swagger
```

## Git Workflow

The project uses a simple branch workflow:

```text
feature/*
    ↓
develop
    ↓
master
```

Features are developed in separate branches and merged into `develop`.

The `master` branch is kept for stable versions of the project.
