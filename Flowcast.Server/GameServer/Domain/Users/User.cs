using SharedKernel;

namespace Domain.Users;

public sealed class User : Entity<Guid>
{
    public string Email { get; set; }
    public string DisplayName { get; private set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAtUtc { get; private set; }
    public bool IsBanned { get; private set; }

    private User() { } // EF/ORM constructor

    public User(Guid id, string displayName, string email)
    {
        Id = id;
        DisplayName = displayName;
        Email = email;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void ChangeDisplayName(string newName)
    {
        DisplayName = newName;
    }

    public void Ban() => IsBanned = true;
}