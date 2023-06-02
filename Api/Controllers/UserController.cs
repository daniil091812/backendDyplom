using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using WebApplication1.Models;

namespace WebApplication1.Controllers;
[ApiController]
[Route("[controller]/[action]")]
public class UserController : ControllerBase
{

    public UserController(MongoClient client)
    {
        var db = client.GetDatabase("tourism");
        _users = db.GetCollection<User>("users");
    }

    private IMongoCollection<User> _users;

    [HttpPost]
    public IActionResult Token([FromBody] Creds cred)
    {
        var identity = GetIdentity(cred.Username, cred.Password);
        if (identity == null)
        {
            return BadRequest(new { errorText = "Invalid username or password." });
        }
 
        var now = DateTime.UtcNow;

        var jwt = new JwtSecurityToken(
            issuer: TourJwtOptions.Issuer,
            audience: TourJwtOptions.Audience,
            notBefore: now,
            expires: now.AddDays(7),
            claims: identity.Claims,
            signingCredentials: new SigningCredentials(TourJwtOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
        var response = new
        {
            access_token = encodedJwt,
            username = identity.Name
        };
        return new JsonResult(response);
    }

    [HttpGet]
    public IActionResult IsLoggedIn()
    {
        var isAuth = User.Identity?.IsAuthenticated;
        isAuth ??= false;
        return new JsonResult(isAuth);
    }

    [HttpGet]
    [Authorize]
    public IActionResult MyProfile()
    {
        var userIdString = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
        if (userIdString == null) return BadRequest();
        if (!int.TryParse(userIdString, out int userId)) return BadRequest();
        var user = _users.Find(u => u.InternalId == userId).FirstOrDefault();
        if (user is null) return BadRequest();

        return new JsonResult(new { name = user.Login, points = user.Points});
    }

    private ClaimsIdentity? GetIdentity(string username, string password)
    { 
        var user = _users.Find(x => x.Login == username && x.Password == password).FirstOrDefault();
        if (user is not { Login: not null, Password: not null }) return null;
        var claims = new List<Claim>
        {
            new Claim("id", user.InternalId.ToString()),
            new(ClaimsIdentity.DefaultNameClaimType, user.Login),
        };
        var claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
            ClaimsIdentity.DefaultRoleClaimType);
        return claimsIdentity;
    }
}