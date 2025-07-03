using System.Text.Json.Serialization;

public class MyTask
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool Done { get; set; }
    [JsonIgnore]
    public virtual string? UserId { get; set; }
    [JsonIgnore]
    public virtual ApplicationUser? User { get; set; }
}


