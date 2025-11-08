using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using UsersApi.Models;

namespace UsersApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMongoCollection<User> _users;

    public UsersController(IConfiguration configuration)
    {
        var connectionString = configuration.GetValue<string>("MongoDB:ConnectionString") ?? "mongodb://localhost:27017";
        var client = new MongoClient(connectionString);
        var db = client.GetDatabase("UsersDb");
        _users = db.GetCollection<User>("Users");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> Get()
    {
        var list = await _users.Find(_ => true).ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<User>> Get(string id)
    {
        var user = await _users.Find(x => x.Id == id).FirstOrDefaultAsync();
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<User>> Post([FromBody] User user)
    {
        // Игнорируем входящее Id, чтобы MongoDB сгенерировал корректный ObjectId
        user.Id = null;
        await _users.InsertOneAsync(user);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Put(string id, [FromBody] User user)
    {
        user.Id = id;
        var result = await _users.ReplaceOneAsync(x => x.Id == id, user);
        return result.MatchedCount == 0 ? NotFound() : NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _users.DeleteOneAsync(x => x.Id == id);
        return result.DeletedCount == 0 ? NotFound() : NoContent();
    }
}


