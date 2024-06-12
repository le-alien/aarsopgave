using c_ApiLayout.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using ShaNext.ShaNext;


namespace User.Controller
{
    [ApiController]
    [Route("api/UserController")]
    public class apiLayoutController : ControllerBase
    {
        private readonly IMongoCollection<BsonDocument> _UserCollection;
        private readonly IConfiguration _configuration;
        public apiLayoutController(IConfiguration configuration, IMongoClient mongoClient)
        {
            _configuration = configuration;

            var client = mongoClient;
            var userDatabase = client.GetDatabase("test");
            _UserCollection = userDatabase.GetCollection<BsonDocument>("users");
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserDto userForm)
        {
            try
            {
                string Username = userForm.Username;
                string Password = userForm.Password;
                string HashPassword = ShaNextHashing.GenerateSaltedHash(Password);
                bool Admin = false;


                var filter = Builders<BsonDocument>.Filter.Eq("username", Username);
                var document = await _UserCollection.Find(filter).FirstOrDefaultAsync();

                if (document != null)
                {
                    return Conflict(new { error = "Username taken." });
                }

                var userEntry = new BsonDocument
        {
            { "username", Username },
            { "password", HashPassword },
            { "admin", Admin }
        };
                await _UserCollection.InsertOneAsync(userEntry);

                return Ok(new { Username });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> LoginEndpoint([FromBody] LoginDto form)
        {
            try
            {



                string Username = form.Username;
                string Password = form.Password;

                var filter = Builders<BsonDocument>.Filter.Eq("username", Username);
                var document = await _UserCollection.Find(filter).FirstOrDefaultAsync();
                bool Admin = document["admin"].AsBoolean;
                if (document != null)
                {
                    string HashPassord = document["password"].AsString;
                    bool compare = ShaNextCompare.VerifySaltedHash(Password, HashPassord);
                    if (compare)
                    {
                        return Ok(new { Username, Password, Admin });
                    }
                    else
                    {
                        return BadRequest(new { message = "error" });
                    }
                }

                else
                {
                    return BadRequest(new { message = "Error" });
                }
            }
            catch (Exception ex)
            {
                return await Task.FromResult(BadRequest($"Invalid input, ID: {Guid.NewGuid().ToString()}"));
            }
        }

        [HttpPost("Update")]
        public async Task<IActionResult> Update([FromBody] UserDto userForm)
        {
            try
            {
                string Username = userForm.Username;
                bool setAdminTrue = true;

                var filter = Builders<BsonDocument>.Filter.Eq("username", Username);
                var document = await _UserCollection.Find(filter).FirstOrDefaultAsync();

                bool Admin = document["admin"].AsBoolean;

                if (document == null)
                {
                    return NotFound(new { message = "User not found." });
                }
                bool useradmin = document["admin"].AsBoolean;

                if (useradmin)
                {
                    return Ok(new { message = "Bruker er admin" });
                }
                else
                {
                    return Ok(new { message = "Bruker er ikke admin" });
                }
                var update = Builders<BsonDocument>.Update

                    .Set("admin", setAdminTrue);

                await _UserCollection.UpdateOneAsync(filter, update);

                return Ok(new { message = "Updated" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred: {ex.Message}" });
            }
        }



        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Empty;
                var documents = await _UserCollection.Find(filter).ToListAsync();
                var users = documents.Select(document => new
                {
                    Username = document["username"].AsString,
                    Admin = document.Contains("admin") && document["admin"].IsBoolean ? document["admin"].AsBoolean : false
                }).ToList();

                return Ok(new { Users = users });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
