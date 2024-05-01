using Src.Models;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System;
using StackExchange.Redis;
using Newtonsoft.Json;
using Src.Utils;

[ApiController]
public class GeneralController : ControllerBase
{
    private readonly IDatabase _redisConnection;
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;
    public GeneralController(AppDbContext dbContext, IConfiguration configuration,IConnectionMultiplexer muxer)
    {
        _redisConnection = muxer.GetDatabase();
        _dbContext = dbContext;
        _configuration = configuration;
    }

    [HttpGet("/")]
    public ActionResult Home()
    {
        // todo, may have add some postgresql & redis connection check
        _redisConnection.StringSet("keeey", "vall");
        return Ok(new {api_service_status_is = "Ok.", redis_ping_response_time=_redisConnection.Ping().ToString()});
    }

    private string GenerateJwtToken(Users user)
    {

        var tokenHandler = new JwtSecurityTokenHandler();
        string jwtKey = _configuration["JWT_KEY"];
        if(jwtKey == null){
            jwtKey = "KeyFQGhfT!Jb^BVBBqE48O0wnueyX!ERtt*";
        }
        var key = Encoding.ASCII.GetBytes(jwtKey);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {

            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, user.email),
                new Claim("type_id", user.type_id.ToString()),
                new Claim("user_id", user.id.ToString()),
            }),
            
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        // nothing important, make your choose for your purpose
        if (user.type_id > 1) {
            tokenDescriptor.Expires = DateTime.UtcNow.AddDays(1);
        }else{
            tokenDescriptor.Expires = DateTime.UtcNow.AddHours(1);
        }

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }


    private bool validateUserCredentials(string userPassword, string password)
    {
        string saltStr = _configuration["SECURITY_PASSWORD_SALT"];
        if (saltStr == null)
        {
            saltStr = "BVBBqE48O0wnueyX!ERtt*KeyFQGhfT!Jb^";
        }
        // hash password
        string hashedPassword = Util.HashPassword(password, saltStr);

        // Chech Hash
        return userPassword == hashedPassword;

    }

    [HttpPost("/api/login")]
    public ActionResult Login([FromBody] LoginViewModel model)
    {

        if (model.email == "admin" && model.password == "root")
        {
            Users user = new Users{
                name_surname= "First Login",
                type_id=0,
                email="admin@root",
                id=0,
            };
            // for first login
            var token = GenerateJwtToken(user);
            
            //saveTokenToDB(user.id, token);

            return Ok(new { Token = token, user_id = user.id, validTo = ""}); // todo for validTo
        }

        if (ModelState.IsValid)
        {
            var user = _dbContext.UserModel.Where(x => x.deleted_at == null && x.email == model.email).OrderByDescending(x => x.id).FirstOrDefault();

            if (user == null)
                return NotFound();
            
            bool checkPasswd = validateUserCredentials(user.password, model.password);

            var otherCompanies = new List<int>();

            if (user != null && checkPasswd)
            {
                var token = GenerateJwtToken(user);
                
                saveTokenToDB(user.id, token);

                return Ok(new { Token = token, user_id = user.id, validTo = ""}); 
            }
        }
        return BadRequest(ModelState);
    }

    private bool saveTokenToDB(int userId, string token) 
    {

        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        var u = _dbContext.UserModel.Find(userId);

        if (u == null)
        {
            return false;
        }
        _dbContext.Entry(u).State = EntityState.Modified;

        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            u.last_login_time = DateTimeOffset.Now.ToUniversalTime();
            u.last_token = token;
            u.last_user_agent = userAgent;
            u.last_ip = ipAddress;

            _dbContext.SaveChanges();

            // redis save set key token, value user model
            string serializedUser = JsonConvert.SerializeObject(u);
            _redisConnection.StringSet(token, serializedUser);

            return true;

        }
        catch (Exception e)
        {
            Debug.WriteLine("saveTokenToDB ERR : "+ e.Message);
        }

        return false;
    }


    [HttpGet("/api/checkUser/{mailAddress}")]
    public ActionResult CheckEmail(string mailAddress)
    {
        try
        {

        var cmps = _dbContext.UserModel.Where(x => x.deleted_at == null && x.email == mailAddress).ToList();

        if (cmps.Count == 0)
        {
            return NotFound(new {is_exist = false, message = "Record not avaible!" });
        }

        return Ok(new {is_exist = true });

        } catch (Exception ex)
        {
            Debug.WriteLine("ERR : ", ex.Message);
            return StatusCode(500);
        }
    }

}