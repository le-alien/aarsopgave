using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ShaNext.ShaNext;


namespace c_ApiLayout.Controllers
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

                var filter = Builders<BsonDocument>.Filter.Eq("username", Username);
                var document = await _UserCollection.Find(filter).FirstOrDefaultAsync();

                if (document != null)
                {
                    return Conflict("Username taken.");
                }

                var userEntry = new BsonDocument
        {
            { "username", Username },
            { "password", HashPassword },
        };
                await _UserCollection.InsertOneAsync(userEntry);

                return Ok("Registered");
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
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

                if (document != null)
                {
                    string HashPassord = document["password"].AsString;
                    bool compare = ShaNextCompare.VerifySaltedHash(Password, HashPassord);
                    if (compare)
                    {
                        return Ok("suksess");
                    }
                    else
                    {
                        return BadRequest("Error");
                    }
                }

                else
                {
                    return BadRequest("Error");
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

                var filter = Builders<BsonDocument>.Filter.Eq("username", Username);
                var document = await _UserCollection.Find(filter).FirstOrDefaultAsync();

                bool Admin = document["admin"].AsBoolean;

                if (document == null)
                {
                    return NotFound("User not found.");
                }
                bool useradmin = document["admin"].AsBoolean;

                if (useradmin)
                {
                    return Ok("Bruker er admin");
                }
                else
                {
                    return Ok("Bruker er ikke admin");
                }
                var update = Builders<BsonDocument>.Update

                    .Set("admin", !Admin);

                await _UserCollection.UpdateOneAsync(filter, update);

                return Ok("Updated");
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }


    }
}
