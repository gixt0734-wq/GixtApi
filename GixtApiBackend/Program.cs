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
using GixtApiBackend.Application.UseCases.Users;
using GixtApiBackend.Application.UseCases.Workers;
using GixtApi.Infrastructure.Repositories;
using GixtApi.Infraestructure;

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


// =======================================================
// ?? REPOSITORIOS
// =======================================================

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWorkerRepository, WorkerRepository>();
builder.Services.AddScoped<EmailService>();

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