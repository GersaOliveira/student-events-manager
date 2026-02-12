using System;

namespace StudentEvent.Domain.Entities;

public class Event
{
    public Guid Id { get; set; }
    public string MicrosoftEventId { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    // Chave Estrangeira
    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;
}