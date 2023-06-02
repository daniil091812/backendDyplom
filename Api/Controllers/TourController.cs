using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class TourController : ControllerBase
{
    private  IMongoCollection<Tour> _tours;
    private IMongoCollection<User> _users;

    public TourController(MongoClient client)
    {
        var db = client.GetDatabase("tourism");
        _users = db.GetCollection<User>("users");
        _tours = db.GetCollection<Tour>("tours");
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return new JsonResult(_tours.Find(_=>true).ToEnumerable());
    }

    [HttpGet]
    [Authorize]
    public IActionResult GetAffordable()
    {
        var user = GetUser(User.Claims);
        if (user is null) return BadRequest();
        var result = _tours.Find(t => t.Price <= user.Points).ToEnumerable();
        return new JsonResult(result);
    }

    [HttpGet]
    [Authorize]
    public IActionResult Book(int id)
    {
        var user = GetUser(User.Claims);
        if (user is null) return BadRequest();

        var tour = _tours.Find(t => t.InternalId == id).FirstOrDefault();
        if (tour is null) return BadRequest("This tour does not exist");

        if (user.Points < tour.Price) return BadRequest("Not enough points");

        user.Points -= tour.Price;
        user.TourIds ??= Array.Empty<int>();
        user.TourIds = user.TourIds.Append(tour.InternalId).ToArray();

        _users.FindOneAndReplace(u => u.Id == user.Id, user);

        return new JsonResult(tour);
    }

    [HttpGet]
    [Authorize]
    public IActionResult MyTours()
    {
        var user = GetUser(User.Claims);
        if (user is null) return BadRequest();

        var tours = user.TourIds?.Select(id => _tours.Find(t => t.InternalId == id).FirstOrDefault())
            .Where(t => t is not null);
        return new JsonResult(tours);
    }

    private User? GetUser(IEnumerable<Claim> claims)
    {
        var userIdString = claims.FirstOrDefault(c => c.Type == "id")?.Value;
        if (userIdString == null) return null;
        if (!int.TryParse(userIdString, out int userId)) return null;
        var user = _users.Find(u => u.InternalId == userId).FirstOrDefault();
        return user;
    }
}