
using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public JwtMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task Invoke(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        
        if (context.Request.Path.StartsWithSegments("/api/v1"))
        {
            if (string.IsNullOrEmpty(token))
                {
                    
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
                }
            else
            {
                AttachToContext(context, token);

                if (context.Items["type_id"] == null)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized.");
                    return;
                }
            }
        }

        await _next(context);
    }

    private void AttachToContext(HttpContext context, string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        
        string jwtKey = _configuration["JWT_KEY"];
        if(jwtKey == null){
            jwtKey = "KeyFQGhfT!Jb^BVBBqE48O0wnueyX!ERtt*";
        }

        var key = Encoding.ASCII.GetBytes(jwtKey);

        try{
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userType = jwtToken.Claims.First(x => x.Type == "type_id").Value;
            var userId = jwtToken.Claims.First(x => x.Type == "user_id").Value;

            context.Items["user_id"] = userId;
            context.Items["type_id"] = userType;

        } catch (Exception ex)
        {
            Debug.WriteLine("AttachToContext ERR : "+ex.Message);
        }
    }
}
