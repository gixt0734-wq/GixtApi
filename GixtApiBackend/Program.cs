using System.Text;
using FirebaseAdmin;
using GixtApiBackend.Infrastructure;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Application.UseCases.Users;
using GixtApiBackend.Application.UseCases.Workers;
using GixtApi.Infrastructure.Repositories;
using GixtApiBackend.Application.UseCases.Advertisements;
using GixtApiBackend.Infraestructure;
using GixtApiBackend.Application.UseCases.Categories;
using GixtApiBackend.Infrastructure.Repositories;
using GixtApiBackend.Application.UseCases.Services;
using GixtApiBackend.Application.UseCases.Favorites;
using GixtApiBackend.Application.UseCases.Locations;
using GixtApiBackend.Application.UseCases.Expresss;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen(options =>
{
    //// JWT CONFIG
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token así: Bearer {tu token}"
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
            new string[] {}
        }
    });
});

// =======================================================
// ?? CONEXIÓN A POSTGRESQL
// =======================================================
var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// =======================================================
// ?? SERVICIOS BASE
// =======================================================

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<FcmService>();

// =======================================================
// ?? REPOSITORIOS
// =======================================================

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWorkerRepository, WorkerRepository>();
builder.Services.AddScoped<IAdvertisementRepository, AdvertisementRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IExpressRepository, ExpressRepository>();

// =======================================================
// ?? USE CASES - USERS
// =======================================================
builder.Services.AddScoped<CreateUser>();
builder.Services.AddScoped<GetUser>();
builder.Services.AddScoped<UpdateUser>();
builder.Services.AddScoped<DeleteUser>();
builder.Services.AddScoped<GetUserById>();
builder.Services.AddScoped<VerficationEmail>();

// =======================================================
// ?? USE CASES - WORKERS
// =======================================================
builder.Services.AddScoped<CreateWorker>();
builder.Services.AddScoped<CreateInfoWorker>();
builder.Services.AddScoped<GetWorkers>();
builder.Services.AddScoped<UpdateWorker>();
builder.Services.AddScoped<DeleteWorker>();
builder.Services.AddScoped<GetWorkerById>();
builder.Services.AddScoped<GetInfoWorkerById>();

// =======================================================
// ?? USE CASES - Advertisement
// =======================================================
builder.Services.AddScoped<CreateAdvertisement>();
builder.Services.AddScoped<GetAdvertisement>();
builder.Services.AddScoped<UpdateAdvertisement>();
builder.Services.AddScoped<DeleteAdvertisement>();

// =======================================================
// ?? USE CASES - CATEGORY
// =======================================================
builder.Services.AddScoped<CreateCategory>();
builder.Services.AddScoped<GetCategory>();
builder.Services.AddScoped<UpdateCategory>();
builder.Services.AddScoped<DeleteCategory>();

// =======================================================
// ?? USE CASES - Services
// =======================================================
builder.Services.AddScoped<CreateService>();
builder.Services.AddScoped<GetService>();
builder.Services.AddScoped<UpdateService>();
builder.Services.AddScoped<DeleteService>();
builder.Services.AddScoped<GetServiceById>();
builder.Services.AddScoped<GetServiceByIdWorker>();
builder.Services.AddScoped<GetServiceByCategory>();

// =======================================================
// ?? USE CASES - Services Favorites
// =======================================================
builder.Services.AddScoped<CreateFavorite>();
builder.Services.AddScoped<GetFavorite>();
builder.Services.AddScoped<UpdateFavorite>();
builder.Services.AddScoped<DeleteFavorite>();
builder.Services.AddScoped<GetFavoriteById>();

// =======================================================
// ?? USE CASES - LOCATION 
// =======================================================
builder.Services.AddScoped<CreateLocation>();
builder.Services.AddScoped<GetLocation>();
builder.Services.AddScoped<UpdateLocation>();
builder.Services.AddScoped<DeleteLocation>();
builder.Services.AddScoped<GetLocationById>();
builder.Services.AddScoped<GetLocationByUserId>();

// =======================================================
// ?? USE CASES - Express
// =======================================================
builder.Services.AddScoped<CreateExpress>();
builder.Services.AddScoped<UpdateExpressStatus>();
builder.Services.AddScoped<GetExpress>();
builder.Services.AddScoped<CancelExpress>();
builder.Services.AddScoped<DeleteExpress>();
builder.Services.AddScoped<SendAccept>();
builder.Services.AddScoped<AcceptExpress>();
builder.Services.AddScoped<GetExpressReviewById>();
builder.Services.AddScoped<GetExpressById>();
//builder.Services.AddScoped<GetExpressByUserId>();
builder.Services.AddScoped<GetExpressByWorkerId>();
//builder.Services.AddScoped<GetExpressWorkerById>();

// =======================================================
// ?? JWT
// =======================================================
builder.Services.AddScoped<TokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
        )
    };

    // ?? IMPORTANTE PARA SIGNALR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/gpsHub"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// =======================================================
// ?? CORS (CONFIGURACIÓN CORRECTA)
// =======================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://171849687ff8.ngrok-free.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


var app = builder.Build();

// =======================================================
// ?? MIDDLEWARE PIPELINE (ORDEN CORRECTO)
// =======================================================
app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("CorsPolicy"); // ?? ANTES DE AUTH

app.UseAuthentication();
//app.UseMiddleware<SessionMiddleware>();
app.UseAuthorization();

app.MapControllers();

// =======================================================
// ?? SWAGGER
// =======================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.Run();