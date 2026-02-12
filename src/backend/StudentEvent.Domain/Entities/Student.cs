using System;
using System.Collections.Generic;

namespace StudentEvent.Domain.Entities;

public class Student
{
    public Guid Id { get; set; }
    public string MicrosoftId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    // Relacionamento: Um estudante tem muitos eventos
    public ICollection<Event> Events { get; set; } = new List<Event>();
}