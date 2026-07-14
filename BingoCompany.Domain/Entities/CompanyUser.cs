namespace BingoCompany.Domain.Entities;

public sealed class CompanyUser
{
    private CompanyUser() { }
    public CompanyUser(Guid companyId, string name, string email, string passwordHash)
    {
        Id = Guid.CreateVersion7();
        CompanyId = companyId;
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = DateTimeOffset.UtcNow;
    }
    public Guid Id { get; private set; }
    public Guid CompanyId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }

    public void UpdatePasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
    }
}
