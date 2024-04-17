using c_ApiLayout.Utilities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace c_ApiLayout.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class apiLayoutController : ControllerBase
    {
        private readonly IMongoCollection<BsonDocument> _testCollection;
        private readonly IConfiguration _configuration;
        public apiLayoutController(IConfiguration configuration, IMongoClient mongoClient)
        {
            _configuration = configuration;

            var client = mongoClient;
            var userDatabase = client.GetDatabase("test");
            _testCollection = userDatabase.GetCollection<BsonDocument>("users");
        }

        [HttpPost("testDtoEndpoint")]
        public IActionResult dtoEndpoint([FromBody] UserDto userForm)
        {
            string username = userForm.username;
            string password = userForm.password;
            Log.LogEvent(_testCollection, username, password);
            var ting = username + ":" + password;
            return Ok(ting);
        }
      
    }
}
