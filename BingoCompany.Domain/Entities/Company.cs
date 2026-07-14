namespace BingoCompany.Domain.Entities;

public sealed class Company
{
    private Company() { }
    public Company(string name)
    {
        Id = Guid.CreateVersion7();
        Name = name;
        CreatedAt = DateTimeOffset.UtcNow;
    }
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
}
