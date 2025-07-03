using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
    public ICollection<MyTask> MyTasks { get; set; } = new List<MyTask>();
}

