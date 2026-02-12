namespace StudentEvent.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // Campos para o Reset de Senha
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }
}