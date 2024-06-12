using c_ApiLayout.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;



namespace Post.Controller
{
    [ApiController]
    [Route("api/UserController")]


    public class apiLayoutController : ControllerBase
    {
        private readonly IMongoCollection<BsonDocument> _PostCollection;
        private readonly IConfiguration _configuration;
        public apiLayoutController(IConfiguration configuration, IMongoClient mongoClient)
        {
            _configuration = configuration;

            var client = mongoClient;
            var userDatabase = client.GetDatabase("test");
            _PostCollection = userDatabase.GetCollection<BsonDocument>("posts");
        }
        [HttpPost("CreatePost")]
        public async Task<IActionResult> CreatePost([FromBody] PostDto postForm)
        {
            try
            {
                string Title = postForm.Title;
                string Content = postForm.Content;
                string Author = postForm.Author;
                int likes = postForm.Likes;

                var filter = Builders<BsonDocument>.Filter.Eq("title", Title);
                var document = await _PostCollection.Find(filter).FirstOrDefaultAsync();

                if (document != null)
                {
                    return Conflict(new { error = "Title taken." });
                }

                var postEntry = new BsonDocument
        {
            { "title", Title },
            { "content", Content },
            { "author", Author },
            { "likes", likes }
        };
                await _PostCollection.InsertOneAsync(postEntry);

                return Ok(new { Title });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("GetPost")]
        public async Task<IActionResult> GetPosts()
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Empty;
                var documents = await _PostCollection.Find(filter).ToListAsync();
                var posts = new List<PostDto>();

                foreach (var document in documents)
                {
                    

                    posts.Add(new PostDto
                    {
                        Title = document["title"].AsString,
                        Content = document["content"].AsString,
                        Author = document["author"].AsString,
                        Likes = document["likes"].AsInt32
                    });
                }

                return Ok(new { posts });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"An error occurred: {ex.Message}" });
            }
        }

    }
}
