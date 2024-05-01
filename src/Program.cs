using Src.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using StackExchange.Redis;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(c =>
{

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter the JWT token in the field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

string redisConnStr = builder.Configuration["REDIS_CONNECTION_STRING"];

if(redisConnStr == null)
{
    redisConnStr = "0.0.0.0:64341";
}

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnStr));

string postgresConnStr = builder.Configuration["POSTGRES_CONNECTION_STRING"];
if (postgresConnStr == null)
{
    postgresConnStr = "Host=localhost;Port=62434;Database=jwt-auth;Username=postgres;Password=AfS!Fb2cV0!dyLAS";
}

builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(postgresConnStr));

// CORS Settings TODO make your own info
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAny",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS Setting
app.UseCors("AllowAny");

//app.UseHttpsRedirection();
app.UseAuthentication();  // Authentication middleware
app.UseAuthorization();
app.UseMiddleware<JwtMiddleware>();

app.MapControllers();


app.Run();
