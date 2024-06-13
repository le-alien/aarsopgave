using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

public class UserDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required bool Admin { get; set; }

}

