namespace LeaveMgmt.Domain.Users;

public sealed class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Email { get; private set; } = default!;
    public string FullName { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string PasswordSalt { get; private set; } = default!;
    public string RolesCsv { get; private set; } = "Employee"; // default role

    private User() { }

    public User(string email, string fullName, string passwordHash, string passwordSalt, IEnumerable<string>? roles = null)
    {
        Email = email.Trim().ToLowerInvariant();
        FullName = fullName.Trim();
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
        if (roles is not null && roles.Any())
            RolesCsv = string.Join(",", roles.Distinct(StringComparer.OrdinalIgnoreCase));
    }

    public IReadOnlyCollection<string> Roles =>
        RolesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public void SetPassword(string hash, string salt)
    {
        PasswordHash = hash;
        PasswordSalt = salt;
    }
}
