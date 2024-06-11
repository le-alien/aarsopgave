namespace c_ApiLayout.Models
{
    public class PostDto
    {
        public required string Title { get; set; }
        public required string Content { get; set; }
        public required string Author { get; set; }
        public required int Likes { get; set; }
    }
}
