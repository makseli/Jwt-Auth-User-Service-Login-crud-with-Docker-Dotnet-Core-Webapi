using Src.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Src.Utils;

public class JwtSettings
{
    public string Key { get; set; }
    public string Issuer { get; set; }
}

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });


    public void ConfigureServices(IServiceCollection services)
    {
        var builder = new ConfigurationBuilder()
            .AddEnvironmentVariables();

        IConfiguration configuration = builder.Build();

        services.AddSingleton<IConfiguration>(configuration);
        Util.Configure(configuration);

        //cors setting, now *
        services.AddCors(p => p.AddPolicy("corsapp", builder =>
        {
            builder.WithOrigins(Configuration["CORS_ALLOWED_HOST"]).AllowAnyMethod().AllowAnyHeader();
        }));
        
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = "Bearer";
            options.DefaultAuthenticateScheme = "Bearer";
            options.DefaultChallengeScheme = "Bearer";
            options.DefaultSignInScheme = "Bearer";
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateSignatureLast = true,
                //ValidateIssuerSigningKey = true,
                ValidIssuer = Configuration["JWT_ISSUER"],
                ValidAudience = Configuration["JWT_AUDIENCE"],
                //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JET_KEY"]))
            };
        });

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(Configuration["POSTGRES_CONNECTION_STRING"]);
            
        });

        services.AddIdentity<Users, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 4;
                //options.Password.RequireNonAlphanumeric = true;
                //options.Password.RequireUppercase = true;
                //options.Password.RequireDigit = true;
                //options.SignIn.RequireConfirmedAccount = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();


        services.AddAuthorization();

        services.AddControllers();

   
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseCors("corsapp");
        
        app.UseRouting();
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseMiddleware<JwtMiddleware>();


        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        //app.UseHttpsRedirection();
   
        

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
        
        
    }
}
